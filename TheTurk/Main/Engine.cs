using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessEngine.Moves;
using System.Linq;
namespace ChessEngine.Main
{
    public class Engine
    {
        private List<Move> previousPV;
        private Stopwatch ElapsedTime;
        public readonly Board Board;
        public readonly IProtocol Protocol;
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
        public Engine(Board board, IProtocol protocol)
        {
            Board = board;
            this.Protocol = protocol;
            ElapsedTime = new Stopwatch();
        }
        public Result Search(int maxDepth)
        {
            int alpha = -int.MaxValue, beta = int.MaxValue;
            int nodesCount = 0;
            int depth = 0;
            Result result = null;
            var pv = new List<Move>();
            for (int ply = 1; ply <= maxDepth; )
            {
                ElapsedTime.Reset();
                ElapsedTime.Start();


                var score = AlphaBeta(alpha, beta, ply, depth, pv, ref nodesCount);
                ElapsedTime.Stop();

                result = new Result(ply, score, ElapsedTime.ElapsedMilliseconds, nodesCount, pv);

                previousPV = new List<Move>();
                previousPV.AddRange(pv);

                Protocol.WriteOutput(result);

                if (Math.Abs(score) == Board.CheckMateValue) break;
                ply++;
            }

            return result;
        }
        int AlphaBeta(int alpha, int beta, int ply, int depth, List<Move> pv, ref int nodeCount)
        {
            nodeCount++;

            var moves = Board.GenerateMoves();
            if (moves.Count == 0) return -Board.IsCheckMateOrStaleMate(ply);
            if (ply <= 0) return QuiescenceSearch(alpha, beta, ref nodeCount);

            var localpv = new List<Move>();
            moves = SortMoves(moves, depth);
            foreach (var move in moves)
            {
                Board.MakeMove(move);

                int score = -AlphaBeta(-beta, -alpha, ply - 1, depth + 1, localpv, ref nodeCount);

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
        List<Move> MVVLVASorting(List<Move> moves)
        {
            return moves.OfType<Ordinary>().Where(move => move.CapturedPiece != null).
                OrderByDescending(move => Math.Abs(move.CapturedPiece.PieceValue) - Math.Abs(move.piece.PieceValue)).ToList<Move>();
        }
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
    }
}
