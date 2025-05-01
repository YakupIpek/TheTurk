using TheTurk.Bitboards;

namespace TheTurk.Engine
{
    class HistoryMoves
    {
        /// <summary>
        /// Side,from,to
        /// </summary>
        private int[,,] historyMoves;
        public Move? WhiteBestMove { get; private set; }
        public Move? BlackBestMove { get; private set; }
        public int WhiteBestMoveScore { get; private set; }
        public int BlackBestMoveScore { get; private set; }
        public HistoryMoves()
        {
            const int side = 2, from = 64, to = 64;

            historyMoves = new int[side, from, to];
            WhiteBestMoveScore = 0;
            BlackBestMoveScore = 0;
        }
        public void AddMove(Move move)
        {
            int side = move.IsWhiteMove() ? 0 : 1;//0 for white and 1 for black
            int newMoveScore = ++historyMoves[side, move.FromSquare, move.ToSquare];
            if (side == 0 && newMoveScore > WhiteBestMoveScore && !move.Equals(WhiteBestMove))//if side is white
            {
                WhiteBestMove = move;
            }
            else if (side == 1 && newMoveScore > BlackBestMoveScore && !move.Equals(BlackBestMove))//if side is black
            {
                BlackBestMove = move;
            }
        }
        public int HistoryScoreOfMove(Move move)
        {
            return historyMoves[move.Flags.HasFlag(Color.White) ? 0 : 1, move.FromSquare, move.ToSquare];
        }
    }
}
