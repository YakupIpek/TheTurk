using System.Collections.Generic;
using ChessEngine.Moves;

namespace ChessEngine.Main
{
    public delegate void VariationWriter(Engine.Result result);
    public class Engine
    {
        
        const int MateValue = 320000;
        private List<Move> PrincipalVariation;
        private Stack<Move> Line;
        private VariationWriter WriteLines;
        public readonly Board Board;
        public class Result
        {
            public int Score;
            public List<Move> BestLine;
            public Result(int score, List<Move> bestLine)
            {
                Score = score;
                BestLine = bestLine;
            }
        }
        public Engine(Board board,VariationWriter writer)
        {
            Line = new Stack<Move>();
            Board = board;
            WriteLines = writer;
        }
        public Result Search(int maxDepth)
        {
            int alpha = -int.MaxValue, beta = int.MaxValue, ply = maxDepth;
            var pv=new List<Move>();
            var score = AlphaBeta(alpha, beta, 5,pv);
            WriteLines(new Result(score, pv));
            return new Result(score, pv);
        }
        int AlphaBeta(int alpha, int beta, int ply,List<Move> pv)
        {
            var localpv=new List<Move>();
            if (ply <= 0) return Evaluation.Evaluate(Board);
            var moves = Board.GenerateMoves();
            foreach (var move in moves)
            {
                Board.MakeMove(move);

                int score = -AlphaBeta(-beta, -alpha, ply - 1,localpv);

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
