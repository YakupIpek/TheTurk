using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessEngine.Moves;
using System.Linq;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    public class Engine
    {
        private int iterationPly;
        public bool exit;
        private long timeLimit;
        private List<Move> previousPV;
        private Stopwatch elapsedTime;
        public readonly Board Board;
        public readonly IProtocol Protocol;

        private enum NullMove
        {
            Enabled, Disabled
        }
        public class Result
        {
            public int Ply;
            public int Score;
            public long ElapsedTime;
            public int NodesCount;
            public List<Move> BestLine;
            public Result(int ply, int score, long elapsedTime, int nodesCount, List<Move> bestLine)
            {
                Ply = ply;
                Score = score;
                ElapsedTime = elapsedTime / 10L;
                NodesCount = nodesCount;
                BestLine = bestLine;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="protocol">Protocol will be used to write output of bestline</param>
        public Engine(Board board, IProtocol protocol)
        {
            exit = false;
            Board = board;
            this.Protocol = protocol;
            elapsedTime = new Stopwatch();
        }
        /// <summary>
        /// Calculates best line in given depth
        /// </summary>
        /// <param name="maxDepth"></param>
        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        public Result Search(long timeLimit)
        {
            this.timeLimit = timeLimit;
            int infinity = int.MaxValue;
            int alpha = -infinity, beta = infinity, depth = 0, nodesCount = 0;
            Result previousResult = null;
            elapsedTime.Restart();
            Result result;
            var pv = new List<Move>();
            exit = false;
            for (iterationPly = 1; ; )
            {
                var score = AlphaBeta(alpha, beta, iterationPly, depth, pv, NullMove.Disabled, ref nodesCount);

                if (score <= alpha || score >= beta)
                {
                    //Aspiration window failed so make full window search again.
                    alpha = -infinity;
                    beta = infinity;
                    continue;
                }

                if ((!HaveTime() || exit)&& iterationPly>1) //time control and stop mode
                {
                    return previousResult;
                }
                alpha = score - Pawn.Piecevalue / 4; //Narrow Aspiration window
                beta = score + Pawn.Piecevalue / 4;

                result = new Result(iterationPly, score, elapsedTime.ElapsedMilliseconds, nodesCount, pv);
                previousResult = new Result(iterationPly, score, elapsedTime.ElapsedMilliseconds, nodesCount, pv.ConvertAll(x => x));
                previousPV = new List<Move>();
                previousPV.AddRange(pv);

                if (result.BestLine.Count > 0)
                    Protocol.WriteOutput(result);

                if (Math.Abs(score) == Board.CheckMateValue || exit) break;
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
        int AlphaBeta(int alpha, int beta, int ply, int depth, List<Move> pv, NullMove nullmove, ref int nodeCount)
        {
            if ((!HaveTime()||exit)&& iterationPly>1) return 0;
            nodeCount++;

            var moves = Board.GenerateMoves();
            if (moves.Count == 0) return -Board.IsCheckMateOrStaleMate(ply);
            if (ply <= 0) return QuiescenceSearch(alpha, beta, ref nodeCount);
            var localpv = new List<Move>();

            if (nullmove == NullMove.Enabled && !Board.IsInCheck())
            {
                int R = 2;
                Board.ToggleSide();
                int score = -AlphaBeta(-beta, -beta + 1, ply - 1 - R, depth + 1, localpv, NullMove.Disabled, ref nodeCount);
                Board.ToggleSide();
                if (score >= beta) return score;
            }
            moves = SortMoves(moves, depth);
            foreach (var move in moves)
            {
                Board.MakeMove(move);

                int score = -AlphaBeta(-beta, -alpha, ply - 1, depth + 1, localpv, NullMove.Enabled, ref nodeCount);

                Board.TakeBackMove(move);

                if (score >= beta)
                {
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                    //collect principal variation
                    pv.Clear();
                    pv.Add(move);
                    pv.AddRange(localpv);
                    localpv.Clear();
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
        List<Move> MVVLVASorting(List<Move> moves)
        {
            return moves.OfType<Ordinary>().Where(move => move.CapturedPiece != null).
                OrderByDescending(move => Math.Abs(move.CapturedPiece.PieceValue) - Math.Abs(move.piece.PieceValue)).ToList<Move>();
        }
        /// <summary>
        /// Sort moves best to worst
        /// </summary>
        /// <param name="moves"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        List<Move> SortMoves(List<Move> moves, int depth)
        {
            //Puts previous iteration's best move to beginning
            moves = moves.OrderByDescending(x => x.MovePriority()).ToList();
            if (previousPV != null && previousPV.Count > depth)
            {
                var move = moves.Find(x => x.Equals(previousPV[depth]));
                if (move != null)
                {
                    moves.Remove(move);
                    moves.Insert(0, move);
                }
            }
            return moves;

        }
        bool HaveTime()
        {
            return timeLimit > elapsedTime.ElapsedMilliseconds;
        }
    }
}
