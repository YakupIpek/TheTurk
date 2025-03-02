using System.Diagnostics;
using TheTurk.Moves;
using TheTurk.Pieces;

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
        public bool ExitRequested { get; set; }
        public readonly Board Board;
        const int Infinity = 1_000_000_000;

        int node;

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

            //var eval = Evaluation.Evaluate(Board);
            //alpha = eval - Pawn.Piecevalue;
            //beta = beta + Pawn.Piecevalue;

            iterationPly = 1;

            while (CanSearch())
            {
                //transpositions = new(50000);
                node = 0;
                var (score, line) = Search(alpha, beta, iterationPly, depth, true);

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

                var result = new EngineResult(iterationPly, score, elapsedTime.ElapsedMilliseconds, node, line.Reverse().ToArray());

                yield return result;

                if (result.MateIn != 0 || ExitRequested)
                    break;

                iterationPly++;
            }

            ExitRequested = false;
        }

        private bool CanSearch()
        {
            return HaveTime() && !ExitRequested;
        }

        (int score, Move[] line) Search(int alpha, int beta, int plyLeft, int depth, bool nullMoveActive, bool isCapture = false)
        {
            node++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch() && iterationPly > 1)
                return (Board.Draw, []);

            if (Board.threeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, []);

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (-Board.GetCheckMateOrStaleMateScore(depth), []);

            if (plyLeft <= 0)
            {
                return (QuiescenceSearch(alpha, beta), []);
            }

            if (nullMoveActive && !Board.InCheck() && plyLeft > 2 && !isCapture)
            {
                int R = (plyLeft > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.MakeNullMove();

                var (score, _) = Search(-beta, -beta + 1, plyLeft - R, depth + 1, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (beta, []);
            }

            var sortedMoves = SortMoves(moves, depth);

            var movesIndex = 0;

            var pv = new Move[0];

            foreach (var move in sortedMoves)
            {

                var state = Board.MakeMove(move);

                movesIndex++;

                var score = 0;

                var isCaptureMove = move is Ordinary c && c.CapturedPiece is not null;

                var importantMove = (movesIndex < 3) || Board.InCheck() || (isCapture && movesIndex < 4);

                var line = Array.Empty<Move>();

                if (!importantMove)
                {
                    (score, line) = Search(-beta, -alpha, plyLeft - 3, depth + 1, isCapture).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    //var r = Board.InCheck() ? 0 : 1;

                    (score, line) = Search(-beta, -alpha, plyLeft - 1, depth + 1, false, isCaptureMove).Negate();
                }

                Board.UndoMove(move, state);

                if (score >= beta)
                {
                    killerMoves.Add(move, depth);

                    return (beta, [.. line, move]); //beta cut-off
                }

                if (score > alpha)
                {
                    alpha = score;
                    historyMoves.AddMove(move);

                    pv = [.. line, move];
                }
            }

            return (alpha, pv);
        }

        int QuiescenceSearch(int alpha, int beta)
        {
            node++;

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

                var score = -QuiescenceSearch(-beta, -alpha);

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
            var killer = killerMoves.BestMoves.ElementAtOrDefault(depth);
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            // List to hold sorting criteria, starting with MovePriority at the end
            var sortCriteria = moves.OrderBy(m => false);

            // Add the sorting criteria only if they are not null
            if (previousBestMove != null)
            {
                sortCriteria = moves.OrderBy(move => move.Equals(previousBestMove));
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
