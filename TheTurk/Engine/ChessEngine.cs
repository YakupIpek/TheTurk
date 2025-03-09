using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
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
                //transpositions = new(50000);
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

                var result = new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, nodes, bestLine);

                yield return result;

                if (result.MateIn != 0 && bestLine.Count <= iterationPly)
                    break;

                iterationPly++;
            }

            ExitRequested = false;
        }

        private static IEnumerable<Move> ToEnumerable(Node<Move>? pv)
        {
            for (var current = pv; current != null; current = current.Next)
            {
                yield return current.Value;
            }
        }

        private bool CanSearch()
        {
            return HaveTime() && !ExitRequested;
        }

        (int score, Node<Move>? line) Search(int alpha, int beta, int plyLeft, int depth, bool nullMoveActive, bool isCapture, bool collectPV)
        {
            nodes++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch() && iterationPly > 1)
                return (Board.Draw, null);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, null);

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (Board.GetCheckMateOrStaleMateScore(depth), null);

            if (plyLeft <= 0 )
            {
                return (QuiescenceSearch(alpha, beta, depth), null);
            }

            if (TranspositionTable.TryGetBestMove(Board.ZobristKey, plyLeft, alpha, beta, out var tScore, out var tMove))
            {
                return (tScore, new(tMove));
            }

            if (nullMoveActive && !Board.InCheck() && plyLeft > 2 && !isCapture)
            {
                int R = (plyLeft > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.MakeNullMove();

                var (score, _) = Search(-beta, -beta + 1, plyLeft - R, depth + 1, false, false, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (beta, null);
            }

            var sortedMoves = SortMoves(moves, depth);

            var movesIndex = 0;

            Node<Move> bestMoveSoFar = null;

            var entryType = HashEntryType.Alpha;

            foreach (var move in sortedMoves)
            {
                var state = Board.MakeMove(move);

                movesIndex++;

                var score = 0;

                var isCaptureMove = move is Ordinary c && c.CapturedPiece is not null;

                var inCheckLazy = new Lazy<bool>(Board.InCheck, false);

                var importantMove = (movesIndex < 3) || plyLeft >= 3 || move is Promote or EnPassant|| (isCapture && movesIndex < 9) || inCheckLazy.Value;

                Node<Move>? line = null;

                if (!importantMove)// Late Move Reduction
                {
                    (score, _) = Search(-beta, -alpha, plyLeft - 3, depth + 1, false, isCapture, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    var r = inCheckLazy.Value || move is Promote or EnPassant ? 0 : 1;

                    var pvNode = movesIndex == 1 && bestLine.ElementAtOrDefault(depth)?.Equals(move) == true;
                    var fullSearch = () => Search(-beta, -alpha, plyLeft - r, depth + 1, false, isCaptureMove, collectPV).Negate();

                    if (pvNode) // Principal Variation Search
                        (score, line) = fullSearch();
                    else
                    {
                        (score, _) = Search(-alpha - 1, -alpha, plyLeft - 1, depth + 1, false, isCaptureMove, false).Negate();

                        if (score > alpha && score < beta)
                            (score, line) = fullSearch();
                    }
                }

                Board.UndoMove(move, state);

                if (score >= beta)
                {
                    killerMoves.Add(move, depth);

                    TranspositionTable.Store(Board.ZobristKey, plyLeft, score, HashEntryType.Beta, move);

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

            TranspositionTable.Store(Board.ZobristKey, plyLeft, alpha, entryType, bestMoveSoFar?.Value);

            return (alpha, bestMoveSoFar);
        }

        int QuiescenceSearch(int alpha, int beta, int depth)
        {
            nodes++;

            var eval = Board.Evaluate(depth);

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
