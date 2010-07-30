using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessEngine.Moves;

namespace ChessEngine.Main
{
    public class Engine
    {
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
            var pv = new List<Move>();
            ElapsedTime.Reset();
            ElapsedTime.Start();

            var score = AlphaBeta(alpha, beta, 5, pv, ref nodesCount);

            ElapsedTime.Stop();

            var result = new Result(maxDepth, score, ElapsedTime.ElapsedMilliseconds, nodesCount, pv);
            Protocol.WriteOutput(result);
            return result;
        }
        int AlphaBeta(int alpha, int beta, int ply, List<Move> pv, ref int nodeCount)
        {
            nodeCount++;

            var moves = Board.GenerateMoves();
            if (moves.Count == 0) return (Board.IsCheckMateOrStaleMate() + ply) * (int)Board.Side;
            if (ply <= 0) return Evaluation.Evaluate(Board);

            var localpv = new List<Move>();
            foreach (var move in moves)
            {
                Board.MakeMove(move);

                int score = -AlphaBeta(-beta, -alpha, ply - 1, localpv, ref nodeCount);

                Board.TakeBackMove(move);

                if (score >= beta)
                {
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                    if (pv.Count == 0)
                        pv.Add(move);
                    else
                        pv[0] = move;
                    
                    //pv.Clear();
                    //pv.Add(move);
                    //pv.AddRange(localpv);

                    for (int i = 0; i < localpv.Count; i++)
                    {
                        if (pv.Count - 2 < i)
                            pv.Insert(i + 1, localpv[i]);
                        else
                            pv[i + 1] = localpv[i];
                    }
                }
            }
            return alpha;
        }
    }
}
