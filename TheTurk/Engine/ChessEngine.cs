using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
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
        private int iterationPly;
        private long timeLimit;
        private Stopwatch elapsedTime;
        public bool ExitRequested { get; set; }
        public readonly Board Board;
        const int Infinity = int.MaxValue;

        int nodes;

        private TranspositionTable TranspositionTable;
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
            TranspositionTable = new TranspositionTable(30);
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
                beta = Infinity,
                depth = 0;

            iterationPly = 1;

            while (CanSearch())
            {
                nodes = 0;
                var (score, pv) = Search(alpha, beta, iterationPly, depth, true, false, true);

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

                alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                beta = score + Pawn.Piecevalue / 4;

                bestLine = ToEnumerable(pv).ToList();

                var result = new EngineResult(iterationPly, Board.TotalMoves, score, elapsedTime.ElapsedMilliseconds, nodes, bestLine);

                yield return result;

                //if (result.MateIn != 0 && Math.Abs(result.MateIn) + 2 <= iterationPly)
                //    break;

                iterationPly++;
            }

            ExitRequested = false;
        }

        private static IEnumerable<Move> ToEnumerable(Node<Move>? pv)
        {
            for (var current = pv; current?.Value != null; current = current.Next)
            {
                yield return current.Value;
            }
        }

        private bool CanSearch()
        {
            return HaveTime() && !ExitRequested;
        }

        (int score, Node<Move>? line) Search(int alpha, int beta, int depth, int ply, bool nullMoveActive, bool isCapture, bool collectPV)
        {
            nodes++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch() && iterationPly > 1)
                return (Board.Draw, null);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, null);

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (Board.GetCheckMateOrStaleMateScore(ply), null);

            if (depth <= 0)
            {
                var score = QuiescenceSearch(alpha, beta, ply);
                return (score, null);
            }

            if (TranspositionTable.TryGetBestMove(Board.ZobristKey, depth, ref alpha, ref beta) is { Valid: true, Score: var tScore, BestMove: var tMove })
            {
                return (tScore, tMove);
            }

            if (nullMoveActive && !Board.InCheck() && depth > 2 && !isCapture)
            {
                int R = (depth > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.MakeNullMove();

                var (score, _) = Search(-beta, -beta + 1, depth - R, ply + 1, false, false, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (beta, null);
            }

            var sortedMoves = SortMoves(moves, ply);

            var movesIndex = 0;

            Node<Move> bestMoveSoFar = null;

            var entryType = HashEntryType.UpperBound;
            var bestScore = alpha;

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
                    (score, _) = Search(-beta, -alpha, depth - 3, ply + 1, true, isCapture, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    var r = inCheckLazy.Value || move is Promote or EnPassant ? 0 : 1;

                    var pvNode = movesIndex == 1 && bestLine.ElementAtOrDefault(ply)?.Equals(move) == true;
                    var fullSearch = (bool nullEnabled) => Search(-beta, -alpha, depth - r, ply + 1, nullEnabled, isCaptureMove, collectPV).Negate();

                    if (pvNode) // Principal Variation Search
                        (score, line) = fullSearch(false);
                    else
                    {
                        (score, _) = Search(-alpha - 1, -alpha, depth - 1, ply + 1, false, isCaptureMove, false).Negate();

                        if (score > alpha && score < beta)
                            (score, line) = fullSearch(true);
                    }
                }

                Board.UndoMove(move, state);

                bestScore = Math.Max(score, bestScore);

                if (score >= beta)
                {
                    killerMoves.Add(move, ply);

                    TranspositionTable.Store(Board.ZobristKey, depth, bestScore, HashEntryType.LowerBound, null);

                    return (beta, null);
                }

                if (score > alpha)
                {
                    entryType = HashEntryType.Exact;
                    alpha = score;
                    historyMoves.AddMove(move);

                    if (collectPV)
                    {
                        bestMoveSoFar = new Node<Move>(move, line);
                    }
                }
            }

            TranspositionTable.Store(Board.ZobristKey, depth, bestScore, entryType, bestMoveSoFar);

            return (alpha, bestMoveSoFar);
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
        IEnumerable<Move> SortMoves(IEnumerable<Move> moves, int depth)
        {
            var previousBestMove = bestLine.ElementAtOrDefault(depth);
            var killer = killerMoves.BestMoves.ElementAtOrDefault(depth);
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            return moves.OrderByDescending(move =>
            {

                int priority = move.MovePriority();

                if (previousBestMove?.Equals(move) == true)
                    priority += 1000;

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
