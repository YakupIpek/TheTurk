using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheTurk.Moves;
using System.Linq;
using TheTurk.Pieces;
using System.Threading.Tasks.Sources;
using System.Runtime.CompilerServices;

namespace TheTurk.Engine
{
    public class TableEntry
    {
        public int Score { get; set; }
        public int Depth { get; set; }
        public long HashKey { get; set; }
    }

    public static class ResultExtensions
    {
        public static (int score, Move[] line) Negate(this (int score, Move[] line) result) => (-result.score, result.line);
    }
    public partial class ChessEngine
    {
        private KillerMoves killerMoves;
        private HistoryMoves historyMoves;
        private int iterationPly;
        private long timeLimit;
        private Stopwatch elapsedTime;
        int node;
        public bool ExitRequested { get; set; }
        public readonly Board Board;

        Dictionary<long, TableEntry> transpositions;
        private Move[] bestLine;

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
        }
        /// <summary>
        /// Calculates best line in given depth
        /// </summary>
        /// <param name="maxDepth"></param>
        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        public IEnumerable<EngineResult> Search(long timeLimit, int maxDepth = int.MaxValue)
        {
            this.timeLimit = timeLimit;

            elapsedTime.Restart();
            ExitRequested = false;
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
            bestLine = [];

            int infinity = int.MaxValue;

            int alpha = -infinity,
                beta = infinity,
                depth = 0;

            for (iterationPly = 1; iterationPly <= maxDepth; )
            {
                //transpositions = new(50000);
                node = 0;
                var (score, line) = AlphaBeta(alpha, beta, iterationPly, depth, false);

                if ((!HaveTime() || ExitRequested) && iterationPly > 1) //time control and stop mode
                {
                    break;
                }

                if (score <= alpha || score >= beta)
                {
                    // Make full window search again
                    alpha = -infinity;
                    beta = infinity;

                    continue;
                }

                alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                beta = score + Pawn.Piecevalue / 4;

                bestLine = line;

                yield return new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, node, line); ;


                if (Math.Abs(score) >= Board.CheckMateValue || ExitRequested)
                    break;

                iterationPly++;
            }

            ExitRequested = false;
        }


        (int score, Move[] line) AlphaBeta(int alpha, int beta, int ply, int depth, bool nullMoveActive)
        {

            node++;

            //if (transpositions.TryGetValue(Board.Zobrist.ZobristKey, out var entry) && entry.Depth >= ply)
            //{
            //    if (entry.Score <= alpha)
            //        return alpha;
            //    if (entry.Score >= beta)
            //        return beta;

            //    return entry.Score;
            //}

            //if time out or exit requested after 1st iteration,so leave thinking.
            if ((!HaveTime() || ExitRequested) && iterationPly > 1)
                return (Board.Draw, []);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, []);

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (-Board.GetCheckMateOrStaleMateScore(ply), []);

            if (ply <= 0)
                return (QuiescenceSearch(alpha, beta), []);

            if (nullMoveActive && !Board.IsInCheck() && ply > 2)
            {
                int R = (ply > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.GetState();
                Board.MakeNullMove();

                var (score, _) = AlphaBeta(-beta, -beta + 1, ply - R, depth + 1, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (beta, []);
            }

            var sortedMoves = SortMoves(moves, depth);

            var movesIndex = 0;

            var pv = new Move[0];

            foreach (var move in sortedMoves)
            {
                var boardState = Board.GetState();
                Board.MakeMove(move);

                movesIndex++;

                var score = 0;
                var importantMove = Board.IsInCheck() && movesIndex < 2 && move is Ordinary o && o.CapturedPiece is not null;

                var line = Array.Empty<Move>();

                if (!importantMove)
                {
                    (score, line) = AlphaBeta(-beta, -alpha, ply - 2, depth + 1, false).Negate();

                    importantMove = score > alpha;
                }

                if (importantMove)
                {
                    (score, line) = AlphaBeta(-alpha - 1, -alpha, ply - 1, depth + 1, false).Negate();

                    if (score > alpha && score < beta)
                    {
                        (score, line) = AlphaBeta(-beta, -alpha, ply - 1, depth + 1, false).Negate();
                    }
                }

                Board.UndoMove(move, boardState);

                if (score >= beta)
                {
                    killerMoves.Add(move, depth);

                    return (beta, [move, .. line]); //beta cut-off
                }

                if (score > alpha)
                {
                    alpha = score;
                    historyMoves.AddMove(move);

                    pv = [move, .. line];
                }
            }

            // Transposition Table'a ekle
            //transpositions[Board.Zobrist.ZobristKey] = new TableEntry
            //{
            //    Score = bestScore,
            //    Depth = ply,
            //    HashKey = Board.Zobrist.ZobristKey
            //};

            return (alpha, pv);
        }
        /// <summary>
        /// Look for capture variations for horizon effect
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        int QuiescenceSearch(int alpha, int beta)
        {
            node++;

            var eval = Evaluation.Evaluate(Board);

            if (eval >= beta)
                return beta;

            if (eval > alpha)
            {
                alpha = eval;
            }

            var moves = MVVLVASorting(Board.GenerateMoves());

            foreach (var capture in moves)
            {
                var boardState = Board.GetState();
                Board.MakeMove(capture);

                var score = -QuiescenceSearch(-beta, -alpha);

                Board.UndoMove(capture, boardState);

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
            return moves.OfType<Ordinary>().Where(move => move.CapturedPiece != null).
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
            var killer = killerMoves.BestMoves[depth];
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            // List to hold sorting criteria, starting with MovePriority at the end
            var sortCriteria = moves.OrderBy(m => false);

            // Add the sorting criteria only if they are not null
            if (previousBestMove != null)
            {
                sortCriteria = sortCriteria.OrderByDescending(move => move.Equals(previousBestMove));
            }

            if (bestHistoryMove != null)
            {
                sortCriteria = sortCriteria.ThenByDescending(move => move.Equals(bestHistoryMove));
            }

            if (killer != null)
            {
                sortCriteria = sortCriteria.ThenByDescending(move => move.Equals(killer));
            }

            // Finally, add MovePriority sorting as the last criteria
            sortCriteria = sortCriteria.ThenByDescending(move => move.MovePriority());

            return sortCriteria;
        }


        bool HaveTime()
        {
            return timeLimit > elapsedTime.ElapsedMilliseconds;
        }
    }
}
