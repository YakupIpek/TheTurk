using System.Diagnostics;
using TheTurk.Bitboards;

namespace TheTurk.Engine
{
    public record class Node<T>(T Value, Node<T>? Next = null);

    
    public static class ResultExtensions
    {
        public static (int score, Node<Move>? line) Negate(this (int score, Node<Move>? line) result) => (-result.score, result.line);
    }
    public partial class ChessEngine
    {
        public const int CheckMateValue = 1_000_000,
                         StaleMateValue = 0,
                         Draw = 0;

        //private KillerMoves killerMoves;
        //private HistoryMoves historyMoves;
        private int searchDepth;
        private long timeLimit;
        private Stopwatch elapsedTime;
        public bool ExitRequested { get; set; }
        public BoardState Board { get; set; }

        const int Infinity = int.MaxValue;
        int nodes;
        public TranspositionTable TranspositionTable;
        private List<Move> bestLine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="protocol">Protocol will be used to write output of bestline</param>
        public ChessEngine(BoardState board)
        {
            ExitRequested = false;
            Board = board;
            elapsedTime = new Stopwatch();
            //historyMoves = new HistoryMoves();
            //killerMoves = new KillerMoves();
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

        public static (bool IsCheckmate, int MateIn, int Score) GetCheckmateInfo(int score)
        {
            var sign = Math.Sign(score);

            score = sign * score; //abs()

            var isMate = score + 2000 > CheckMateValue;

            if (!isMate)
                return (false, 0, 0);

            var mateIn = CheckMateValue -  score;

            return (true, sign * mateIn, sign * score);
        }

        public static int GetCheckMateOrStaleMateScore(BoardState board, int ply)
        {
            var mate = CheckMateValue - ply;
            return board.InCheck() ? -mate : StaleMateValue;
        }

        /// <param name="timeLimit">Time limit in millisecond</param>
        /// <returns></returns>
        private IEnumerable<EngineResult> RunInternal(long timeLimit)
        {
            this.timeLimit = timeLimit;

            elapsedTime.Restart();
            ExitRequested = false;
            //historyMoves = new HistoryMoves();
            //killerMoves = new KillerMoves();
            bestLine = [];

            var alpha = -Infinity;
            var beta = Infinity;

            TranspositionTable.IncrementAge();

            //Board.ThreeFoldRepetetion.Migrate();

            //if (Board.ThreeFoldRepetetion.IsThreeFoldRepetetion)
            //{
            //    yield return new(0, Board.Draw, elapsedTime.ElapsedMilliseconds, 0, []);
            //    yield break;
            //}

            searchDepth = 1;

            do
            {
                nodes = 0;

                var (score, pv) = Search(Board, alpha, beta, searchDepth, height: 0, nullMoveActive: true, isCapture: false, collectPV: true);

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

                alpha = score - 25; //Narrow Aspiration window
                beta = score + 25;

                bestLine = ToEnumerable(pv).ToList();

                var result = new EngineResult(searchDepth, score, elapsedTime.ElapsedMilliseconds, nodes, bestLine);

                yield return result;

                //if (Board.GetCheckmateInfo(score) is { IsCheckmate: true, MateIn: var mateIn } && Math.Abs(mateIn) + 1 <= searchDepth)
                //    yield break;

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

        (int score, Node<Move>? line) Search(BoardState board, int alpha, int beta, int depth, int height, bool nullMoveActive, bool isCapture, bool collectPV)
        {
            nodes++;

            //if time out or exit requested after 1st iteration,so leave thinking.
            if (!CanSearch())
                return (Draw, null);

            //if (Board.ThreeFoldRepetetion.IsThreeFoldRepetetion)
            //    return (Draw, null);

            var isPvNode = alpha + 1 != beta;

            var isRoot = height == 0;
            var isLeaf = depth <= 0;

            //var (valid, tScore, tMove) = TranspositionTable.TryGetBestMove(board.ZobristKey, depth, height, isPvNode, alpha, beta);

            //if (valid)
            //{
            //    return (tScore, tMove);
            //}

            var moveGen = new MoveGen(board);
            var moves = moveGen.GenerateMoves();

            //if (isLeaf)
            //{
            //    Board.DisableThreeFoldRepetition = true;
            //    var score = QuiescenceSearch(alpha, beta, height);
            //    Board.DisableThreeFoldRepetition = false;

            //    return (score, null);
            //}

            //if (nullMoveActive && !isPvNode && !Board.InCheck() && depth > 2 && !isCapture)
            //{
            //    int R = (depth > 6) ? 3 : 2; // Adaptive Null Move Reduction

            //    var state = Board.MakeNullMove();

            //    var (score, _) = Search(-beta, -beta + 1, depth - R, height + 1, false, false, false).Negate();

            //    Board.UndoNullMove(state);

            //    if (score >= beta)
            //        return (score, null);
            //}

            //var sortedMoves = SortMoves(moves, height, tMove?.Value);

            var movesIndex = -1;

            Node<Move>? variation = null;
            Node<Move>? bestMove = null;

            var entryType = HashEntryType.UpperBound;
            var bestScore = -Infinity;


            foreach (var move in moves)
            {
                var nextPosition = board;

                if (!nextPosition.Play(move))
                    continue;

                movesIndex++;

                var score = 0;

                var isCaptureMove = move.CapturedPieceType() is not Piece.None;

                var inCheckLazy = new Lazy<bool>(Board.InCheck, false);

                var importantMove = (movesIndex < 3) || depth >= 3 || move.IsPromotion() || move.IsEnPassant() || (isCapture && movesIndex < 8) || inCheckLazy.Value;

                Node<Move>? line = null;

                if (!importantMove)// Late Move Reduction
                {
                    (score, line) = Search(nextPosition, -beta, -alpha, depth - 3, height + 1, true, isCapture, false).Negate();

                    importantMove = score > alpha && score < beta;
                }

                if (importantMove)
                {
                    var r = inCheckLazy.Value || move.IsPromotion() || move.IsEnPassant() ? 0 : 1;

                    var pvNode = movesIndex == 0 && move.Equals(bestLine.ElementAtOrDefault(height));
                    (int score, Node<Move>? line) fullSearch(bool nullEnabled) => Search(nextPosition,-beta, -alpha, depth - r, height + 1, nullEnabled, isCaptureMove, collectPV).Negate();

                    if (pvNode) // Principal Variation Search in full
                        (score, line) = fullSearch(false);
                    else
                    {
                        //PVS null window
                        (score, line) = Search(nextPosition,-alpha - 1, -alpha, depth - 1, height + 1, false, isCaptureMove, false).Negate();

                        if (score > alpha && score < beta)
                            (score, line) = fullSearch(true);
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = new Node<Move>(move, line);

                    if (score > alpha)
                    {
                        entryType = HashEntryType.Exact;
                        alpha = score;
                        //historyMoves.AddMove(move);

                        if (collectPV)
                        {
                            variation = new Node<Move>(move, line);
                        }

                        if (alpha >= beta)
                        {
                            //killerMoves.Add(move, height);

                            entryType = HashEntryType.LowerBound;
                            break;
                        }
                    }
                }
            }

            if(movesIndex == 0)
                return (GetCheckMateOrStaleMateScore(board, height), null);

            //TranspositionTable.Store(Board.ZobristKey, depth, height, bestScore, entryType, bestMove);

            return (bestScore, variation);
        }

        int QuiescenceSearch(BoardState board, int alpha, int beta, int depth)
        {
            nodes++;

            var eval = board.Evaulate();

            if (eval >= beta)
                return beta;

            if (eval > alpha)
            {
                alpha = eval;
            }

            var moves = new MoveGen(board).CollectCaptures().OrderByDescending(move => move.MvvLvaScore());

            foreach (var move in moves)
            {
                var nextPosition = board;

                if(!nextPosition.Play(move))
                    continue;

                var score = -QuiescenceSearch(nextPosition, -beta, -alpha, depth + 1);

                if (score >= beta) // The move is too good
                    return beta;

                if (score > alpha)// Best move so far
                    alpha = score;
            }

            return alpha;
        }


        /// <summary>
        /// Sort moves best to worst
        /// </summary>
        /// 
        /// <returns></returns>
        //IEnumerable<Move> SortMoves(IEnumerable<Move> moves, int height, Move? tMove)
        //{
        //    var killer = killerMoves.BestMoves.ElementAtOrDefault(height);
        //    var bestHistoryMove = Board.Side == Color.White ? historyMoves.WhiteBestMove : historyMoves.BlackBestMove;

        //    return moves.OrderByDescending(move =>
        //    {

        //        int priority = move.MvvLvaScore();

        //        if (tMove?.Equals(move) == true)
        //            priority += 2000;

        //        if (killer?.Equals(move) == true)
        //            priority += 700;

        //        if (bestHistoryMove?.Equals(move) == true)
        //            priority += 650;

        //        return priority;
        //    });
        //}

        bool HaveTime()
        {
            return timeLimit > elapsedTime.ElapsedMilliseconds;
        }
    }
}
