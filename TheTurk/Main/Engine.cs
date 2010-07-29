using System.Collections.Generic;
using ChessEngine.Moves;
using ChessEngine.Pieces;
using System.Linq;
namespace ChessEngine.Main
{
    class Engine
    {
        
        const int MateValue = 320000;
        private List<Move> PrincipalVariation;
        private Stack<Move> Line;
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
        public Engine(Board board)
        {
            Line = new Stack<Move>();
            Board = board;
        }
        public Result Search(int maxDepth)
        {
            int alpha = -int.MaxValue, beta = int.MaxValue, ply = maxDepth;

            var score = AlphaBeta(alpha, beta, ply);
            PrincipalVariation.Reverse();

            return new Result(score, PrincipalVariation);
        }
        int AlphaBeta(int alpha, int beta, int ply)
        {
            //if (moves.Count == 0) return -(MateValue+ply);
            if (ply <= 0) return Evaluation.Evaluate(Board);
            var moves = Board.GenerateMoves();
            foreach (var move in moves)
            {
                Board.MakeMove(move);
                Line.Push(move);

                int score = -AlphaBeta(-beta, -alpha, ply - 1);

                Board.TakeBackMove(move);
                if (score >= beta)
                {
                    Line.Pop();
                    return beta;
                }
                if (score > alpha)
                {
                    alpha = score;
                    if (ply == 1)
                        PrincipalVariation = Line.ToList();

                }
                Line.Pop();
            }
            return alpha;
        }
    }
}
