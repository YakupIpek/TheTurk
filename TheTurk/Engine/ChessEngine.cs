using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Sources;
using System.Transactions;
using TheTurk.Moves;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    public record class Node<T>(T Value, Node<T>? Next = null);

    public static class ResultExtensions
    {
        public static (int score, Node<Move>? line) Negate(this (int score, Node<Move>? line) result) => (-result.score, result.line);
    }
    public partial class ChessEngine
    {
        private KillerMoves killerMoves;
        private HistoryMoves historyMoves;
        private int searchDepth;
        private long timeLimit;
        private Stopwatch elapsedTime;
        public bool ExitRequested { get; set; }
        public readonly Board Board;
        const int Infinity = int.MaxValue;

        int nodes;

        public TranspositionTable TranspositionTable;
        private List<Move> bestLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="protocol">Protocol will be used to write output of bestline</param>
        public ChessEngine(Board board)
        {
            ExitRequested = false;
            Board = board;
            elapsedTime = new Stopwatch();
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
            TranspositionTable = new TranspositionTable(100);
        }

        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        public IEnumerable<EngineResult> Run(long timeLimit)
        {
            this.timeLimit = timeLimit;

            elapsedTime.Restart();
            ExitRequested = false;
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
            bestLine = [];

            int alpha = -Infinity,
                beta = Infinity;

            TranspositionTable.IncrementAge();

            searchDepth = 1;

            do
            {
                nodes = 0;

                var (score, pv) = Search(alpha, beta, searchDepth, height: 0, nullMoveActive: true, isCapture: false, collectPV: true);

                if (!CanSearch())
                    break;

                var validScore = score > alpha && score < beta;

                if (validScore is not true)
                {
                    // Make full window search again
                    alpha = -Infinity;
                    beta = Infinity;
                    continue;
                }

                //alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                //beta = score + Pawn.Piecevalue / 4;

                bestLine = ToEnumerable(pv).ToList();

                //bestLine = FixPartialPV(bestLine, searchDepth).Reverse().ToList();

                var result = new EngineResult(searchDepth, score, elapsedTime.ElapsedMilliseconds, nodes, bestLine);

                //StoreInTranspositions(result, iterationPly, pv);

                yield return result;

                if (Board.GetCheckmateInfo(score) is { IsCheckmate: true, MateIn: var mateIn } && Math.Abs(mateIn) + 1 <= searchDepth)
                    break;

                searchDepth++;
            } while (CanSearch());

            ExitRequested = false;
        }

        //private IEnumerable<Move> FixPartialPV(List<Move> bestLine, int searchDepth)
        //{
        //    var stack = new Stack<(Move, BoardState)>();

        //    var i = 0;
        //    while (i < searchDepth)
        //    {
        //        var move = bestLine.ElementAtOrDefault(i);

        //        if (move is null)
        //            move = TranspositionTable.TryGetBestMove(Board.ZobristKey, searchDepth - i);

        //        if (move is null)
        //            break;

        //        var state = Board.MakeMove(move);
        //        stack.Push((move, state));
        //        i++;
        //    }

        //    foreach (var (move,state) in stack)
        //    {
        //        Board.UndoMove(move, state);

        //        yield return move;
        //    }
        //}

        //private void StoreInTranspositions(EngineResult result, int depth, Node<Move> node)
        //{
        //    if (node is null)
        //        return;

        //    TranspositionTable.Store(Board.ZobristKey, depth, result.Score, HashEntryType.Exact, node);

        //    var state = Board.MakeMove(node.Value);

        //    StoreInTranspositions(result, depth - 1, node.Next);

        //    Board.UndoMove(node.Value, state);
        //}

        private static IEnumerable<Move> ToEnumerable(Node<Move>? pv)
        {
            for (var current = pv; current?.Value != null; current = current.Next)
            {
                yield return current.Value;
            }
        }

        private bool CanSearch()
        {
            return HaveTime() && !ExitRequested || !bestLine.Any();
        }

        (int score, Node<Move>? line) Search(int alpha, int beta, int depth, int height, bool nullMoveActive, bool isCapture, bool collectPV)
        {
            nodes++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch())
                return (Board.Draw, null);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, null);

            var isPvNode = alpha + 1 == beta;

            if (height != 0 && TranspositionTable.TryGetBestMove(Board.ZobristKey, depth, height, isPvNode, alpha, beta) is { Valid: true, Score: var tScore, BestMove: var tMove })
            {
                return (tScore, tMove);
            }

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (Board.GetCheckMateOrStaleMateScore(height), null);

            if (depth <= 0)
            {
                var score = QuiescenceSearch(alpha, beta, height);
                return (score, null);
            }

            if (nullMoveActive && !Board.InCheck() && depth > 2 && !isCapture)
            {
                int R = (depth > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.MakeNullMove();

                var (score, _) = Search(-beta, -beta + 1, depth - R, height + 1, false, false, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (score, null);
            }

            var sortedMoves = SortMoves(moves, height, null);

            var movesIndex = 0;

            Node<Move>? variation = null;
            Node<Move>? bestMove = null;

            var entryType = HashEntryType.UpperBound;
            var bestScore = -Infinity;

            foreach (var move in sortedMoves)
            {
                var state = Board.MakeMove(move);

                movesIndex++;

                var score = 0;

                var isCaptureMove = move is Ordinary c && c.CapturedPiece is not null;

                var inCheckLazy = new Lazy<bool>(Board.InCheck, false);

                var importantMove = (movesIndex < 3) || depth >= 3 || move is Promote or EnPassant || (isCapture && movesIndex < 9) || inCheckLazy.Value;

                Node<Move>? line = null;

                if (!importantMove)// Late Move Reduction
                {
                    (score, line) = Search(-beta, -alpha, depth - 3, height + 1, true, isCapture, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    var r = inCheckLazy.Value || move is Promote or EnPassant ? 0 : 1;

                    var pvNode = movesIndex == 1 && bestLine.ElementAtOrDefault(height)?.Equals(move) == true;
                    (int score, Node<Move>? line) fullSearch(bool nullEnabled) => Search(-beta, -alpha, depth - r, height + 1, nullEnabled, isCaptureMove, collectPV).Negate();

                    if (pvNode) // Principal Variation Search in full
                        (score, line) = fullSearch(false);
                    else
                    {
                        (score, line) = Search(-alpha - 1, -alpha, depth - 1, height + 1, false, isCaptureMove, false).Negate();

                        if (score > alpha && score < beta)
                            (score, line) = fullSearch(true);
                    }
                }


                Board.UndoMove(move, state);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Node<Move>(move, line);



                    if (score > alpha)
                    {
                        entryType = HashEntryType.Exact;
                        alpha = score;
                        historyMoves.AddMove(move);

                        variation = new Node<Move>(move, line);
                        if (collectPV)
                        {
                        }

                        if (alpha >= beta)
                        {
                            killerMoves.Add(move, height);

                            entryType = HashEntryType.LowerBound;
                            break;
                        }
                    }
                }
            }

            TranspositionTable.Store(Board.ZobristKey, depth, height, bestScore, entryType, bestMove);

            return (bestScore, variation);
        }

        int QuiescenceSearch(int alpha, int beta, int depth)
        {
            nodes++;

            var eval = Board.Evaluate();

            if (eval >= beta)
                return beta;

            if (eval > alpha)
            {
                alpha = eval;
            }

            var moves = MVVLVASorting(Board.GenerateMoves());

            foreach (var capture in moves)
            {
                var state = Board.MakeMove(capture);

                var score = -QuiescenceSearch(-beta, -alpha, depth + 1);

                Board.UndoMove(capture, state);

                if (score >= beta) // The move is too good
                    return beta;

                if (score > alpha)// Best move so far
                    alpha = score;
            }

            return alpha;
        }

        /// <summary>
        /// Filter uncapture moves and sort captured moves
        /// </summary>
        /// <param name="moves"></param>
        /// <returns></returns>
        IEnumerable<Move> MVVLVASorting(IEnumerable<Move> moves)
        {
            return moves.Where(move => (move is Ordinary m && m.CapturedPiece != null) || (move is Promote p && (p.PromotedPiece is Queen or Knight))).
                OrderByDescending(move => move.MovePriority());
        }

        /// <summary>
        /// Sort moves best to worst
        /// </summary>
        /// 
        /// <returns></returns>
        IEnumerable<Move> SortMoves(IEnumerable<Move> moves, int height, Move? tMove)
        {
            var previousBestMove = bestLine.ElementAtOrDefault(height);
            var killer = killerMoves.BestMoves.ElementAtOrDefault(height);
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            return moves.OrderByDescending(move =>
            {

                int priority = move.MovePriority();

                if (previousBestMove?.Equals(move) == true)
                    priority += 1000;

                //if (tMove?.Equals(move) == true)
                //    priority += 500;

                if (killer?.Equals(move) == true)
                    priority += 700;

                if (bestHistoryMove?.Equals(move) == true)
                    priority += 650;

                return priority;
            });
        }

        bool HaveTime()
        {
            return timeLimit > elapsedTime.ElapsedMilliseconds;
        }
    }
}
