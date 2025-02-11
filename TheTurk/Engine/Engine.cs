using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheTurk.Moves;
using System.Linq;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    public partial class Engine
    {
        private KillerMoves killerMoves;
        private HistoryMoves historyMoves;
        private List<Move> previousPV;
        private int iterationPly;
        private long timeLimit;
        private Stopwatch elapsedTime;
        public bool Exit { get; set; }
        public readonly Board Board;
        public readonly IProtocol Protocol;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="protocol">Protocol will be used to write output of bestline</param>
        public Engine(Board board, IProtocol protocol)
        {
            Exit = false;
            Board = board;
            Protocol = protocol;
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
        public EngineResult Search(long timeLimit)
        {
            this.timeLimit = timeLimit;
            int infinity = int.MaxValue;
            int alpha = -infinity, beta = infinity, depth = 0, nodesCount = 0;
            EngineResult previousResult = null;
            elapsedTime.Restart();
            EngineResult result;
            var pv = new List<Move>();
            previousPV = [];
            Exit = false;
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();

            for (iterationPly = 1; ;)
            {
                var copiedPV = pv.ToList();

                var score = AlphaBeta(alpha, beta, iterationPly, depth, pv, false, ref nodesCount);

                if (score <= alpha || score >= beta)
                {
                    // Make full window search again
                    alpha = -infinity;
                    beta = infinity;

                    pv = copiedPV;

                    continue;
                }


                if ((!HaveTime() || Exit) && iterationPly > 1) //time control and stop mode
                {
                    return previousResult;
                }

                alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                beta = score + Pawn.Piecevalue / 4;

                //Save principal variation for next iteration
                previousPV = pv.ToList();

                result = new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, nodesCount, pv);

                previousResult = new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, nodesCount, previousPV);

                if (result.BestLine.Count > 0)
                    Protocol.WriteOutput(result);

                if (Math.Abs(score) == Board.CheckMateValue || Exit || iterationPly>30)
                    break;

                iterationPly++;
                nodesCount = 0;
            }
            return result;
        }
        /// <summary>
        /// AlphaBeta algorithm.Calculates best line in given depth
        /// </summary>
        /// <param name="alpha">Lowest value</param>
        /// <param name="beta">highest value</param>
        /// <param name="ply"></param>
        /// <param name="depth"></param>
        /// <param name="pv">Holds principal variation</param>
        /// <param name="nullmove"></param>
        /// <param name="nodeCount">Holds value of calculated moves</param>
        /// <returns>Returning value is the score of best line</returns>
        int AlphaBeta(int alpha, int beta, int ply, int depth, List<Move> pv, bool nullMoveActive, ref int nodeCount)
        {
            //if time out or exit requested after 1st iteration,so leave thinking.
            if ((!HaveTime() || Exit) && iterationPly > 1)
                return 0;

            nodeCount++;

            var moves = Board.GenerateMoves();
            if (!moves.Any())
                return -Board.GetCheckMateOrStaleMateScore(depth);

            if (ply <= 0)
                return QuiescenceSearch(alpha, beta, ref nodeCount);

            var localpv = new List<Move>();
            var pvSearch = false;

            #region Null Move Prunning
            if (nullMoveActive && !Board.IsInCheck())
            {
                Board.ToggleSide();

                int score = -AlphaBeta(-beta, -beta + 1, ply - 1 - 2, depth + 1, localpv, false, ref nodeCount);
                Board.ToggleSide();
                if (score >= beta) return score;
            }
            #endregion
            var sortedMoves = SortMoves(moves, depth);
            foreach (var move in sortedMoves)
            {
                Board.MakeMove(move);
                int score;
                if (Board.threeFoldRepetetion.IsThreeFoldRepetetion) score = Board.Draw;
                else
                {
                    #region Late Move Reduction

                    if (!Board.IsInCheck())
                    {
                        score = -AlphaBeta(-alpha - 1, -alpha, ply - 2, depth + 1, localpv, true, ref nodeCount);
                    }
                    else score = alpha + 1;
                    #endregion
                    if (score > alpha)
                    {
                        if (pvSearch)
                        {
                            score = -AlphaBeta(-alpha - 1, -alpha, ply - 1, depth + 1, localpv, false, ref nodeCount);

                            if (score > alpha && score < beta)
                            {
                                score = -AlphaBeta(-beta, -alpha, ply - 1, depth + 1, localpv, false, ref nodeCount);
                            }
                        }
                        else
                        {
                            score = -AlphaBeta(-beta, -alpha, ply - 1, depth + 1, localpv, true, ref nodeCount);
                        }

                    }
                }



                Board.TakeBackMove(move);

                if (score >= beta)
                {
                    killerMoves.Add(move, depth);

                    return beta;//beta cut-off
                }
                if (score > alpha)
                {
                    historyMoves.AddMove(move);
                    pvSearch = true;
                    alpha = score;

                    pv.Clear();
                    pv.Add(move);
                    pv.AddRange(localpv);
                }
            }
            return alpha;
        }
        /// <summary>
        /// Look for capture variations for horizon effect
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        int QuiescenceSearch(int alpha, int beta, ref int nodeCount)
        {
            nodeCount++;



            int eval = Evaluation.Evaluate(Board);

            if (eval >= beta)
                return beta;


            if (eval > alpha)
            {

                alpha = eval;
            }

            var moves = MVVLVASorting(Board.GenerateMoves());

            foreach (var capture in moves)
            {
                Board.MakeMove(capture);

                eval = -QuiescenceSearch(-beta, -alpha, ref nodeCount);

                Board.TakeBackMove(capture);



                if (eval >= beta) // The move is too good
                    return beta;

                if (eval > alpha)// Best move so far
                {
                    alpha = eval;
                }
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
            var previousBestMove = (previousPV != null && previousPV.Count > depth) ? previousPV[depth] : null;
            var killer = killerMoves.BestMoves[depth];
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            // List to hold sorting criteria, starting with MovePriority at the end
            var sortCriteria = moves.OrderBy(m => true);

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
