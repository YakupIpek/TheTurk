using System.Diagnostics;
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
        private int searchDepth;
        private long timeLimit;
        private Stopwatch elapsedTime;
        public bool ExitRequested { get; set; }
        public readonly Board Board;
        const int Infinity = int.MaxValue;
        int nodes;
        public TranspositionTable TranspositionTable;
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
            TranspositionTable = new TranspositionTable(100);
        }

        public IEnumerable<EngineResult> Run(long timeLimit)
        {
            try
            {
                return RunInternal(timeLimit);
            }
            finally
            {
                ExitRequested = false;
            }
        }

        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        private IEnumerable<EngineResult> RunInternal(long timeLimit)
        {
            this.timeLimit = timeLimit;

            elapsedTime.Restart();
            ExitRequested = false;
            historyMoves = new HistoryMoves();
            killerMoves = new KillerMoves();
            bestLine = [];

            var alpha = -Infinity;
            var beta = Infinity;

            TranspositionTable.IncrementAge();

            Board.ThreeFoldRepetetion.Migrate();

            if (Board.ThreeFoldRepetetion.IsThreeFoldRepetetion)
            {
                yield return new(0, Board.Draw, elapsedTime.ElapsedMilliseconds, 0, []);
                yield break;
            }

            searchDepth = 1;

            do
            {
                nodes = 0;

                var (score, pv) = Search(alpha, beta, searchDepth, height: 0, nullMoveActive: true, isCapture: false, collectPV: true);

                if (!CanSearch())
                    yield break;

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

                var result = new EngineResult(searchDepth, score, elapsedTime.ElapsedMilliseconds, nodes, bestLine);

                yield return result;

                if (Board.GetCheckmateInfo(score) is { IsCheckmate: true, MateIn: var mateIn } && Math.Abs(mateIn) + 1 <= searchDepth)
                    yield break;

                searchDepth++;
            } while (CanSearch());
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
            return HaveTime() && !ExitRequested || !bestLine.Any();
        }

        (int score, Node<Move>? line) Search(int alpha, int beta, int depth, int height, bool nullMoveActive, bool isCapture, bool collectPV)
        {
            nodes++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch())
                return (Board.Draw, null);

            if (Board.ThreeFoldRepetetion.IsThreeFoldRepetetion)
                return (Board.Draw, null);

            var isPvNode = alpha + 1 != beta;

            var isRoot = height == 0;
            var isLeaf = depth <= 0;

            var (valid, tScore, tMove) = TranspositionTable.TryGetBestMove(Board.ZobristKey, depth, height, isPvNode, alpha, beta);

            if (valid)
            {
                return (tScore, tMove);
            }

            var moves = Board.GenerateMoves();

            if (!moves.Any())
                return (Board.GetCheckMateOrStaleMateScore(height), null);

            if (isLeaf)
            {
                Board.DisableThreeFoldRepetition = true;
                var score = QuiescenceSearch(alpha, beta, height);
                Board.DisableThreeFoldRepetition = false;

                return (score, null);
            }

            if (nullMoveActive && !isPvNode && !Board.InCheck() && depth > 2 && !isCapture)
            {
                int R = (depth > 6) ? 3 : 2; // Adaptive Null Move Reduction

                var state = Board.MakeNullMove();

                var (score, _) = Search(-beta, -beta + 1, depth - R, height + 1, false, false, false).Negate();

                Board.UndoNullMove(state);

                if (score >= beta)
                    return (score, null);
            }

            var sortedMoves = SortMoves(moves, height, tMove?.Value);

            var movesIndex = -1;

            Node<Move>? variation = null;
            Node<Move>? bestMove = null;

            var entryType = HashEntryType.UpperBound;
            var bestScore = -Infinity;

            foreach (var move in sortedMoves)
            {
                movesIndex++;

                var state = Board.MakeMove(move);

                var score = 0;

                var isCaptureMove = move is Ordinary c && c.CapturedPiece is not null;

                var inCheckLazy = new Lazy<bool>(Board.InCheck, false);

                var importantMove = (movesIndex < 3) || depth >= 3 || move is Promote or EnPassant || (isCapture && movesIndex < 8) || inCheckLazy.Value;

                Node<Move>? line = null;

                if (!importantMove)// Late Move Reduction
                {
                    (score, line) = Search(-beta, -alpha, depth - 3, height + 1, true, isCapture, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    var r = inCheckLazy.Value || move is Promote or EnPassant ? 0 : 1;

                    var pvNode = movesIndex == 0 && move.Equals(bestLine.ElementAtOrDefault(height));
                    (int score, Node<Move>? line) fullSearch(bool nullEnabled) => Search(-beta, -alpha, depth - r, height + 1, nullEnabled, isCaptureMove, collectPV).Negate();

                    if (pvNode) // Principal Variation Search in full
                        (score, line) = fullSearch(false);
                    else
                    {
                        //PVS null window
                        (score, line) = Search(-alpha - 1, -alpha, depth - 1, height + 1, false, isCaptureMove, false).Negate();

                        if (score > alpha && score < beta)
                            (score, line) = fullSearch(true);
                    }
                }

                Board.UndoMove(move, state);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Node<Move>(move, line);

                    if (score > alpha)
                    {
                        entryType = HashEntryType.Exact;
                        alpha = score;
                        historyMoves.AddMove(move);

                        if (collectPV)
                        {
                            variation = new Node<Move>(move, line);
                        }

                        if (alpha >= beta)
                        {
                            killerMoves.Add(move, height);

                            entryType = HashEntryType.LowerBound;
                            break;
                        }
                    }
                }
            }

            TranspositionTable.Store(Board.ZobristKey, depth, height, bestScore, entryType, bestMove);

            return (bestScore, variation);
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
        IEnumerable<Move> SortMoves(IEnumerable<Move> moves, int height, Move? tMove)
        {
            var killer = killerMoves.BestMoves.ElementAtOrDefault(height);
            var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

            return moves.OrderByDescending(move =>
            {

                int priority = move.MovePriority();

                if (tMove?.Equals(move) == true)
                    priority += 2000;

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
