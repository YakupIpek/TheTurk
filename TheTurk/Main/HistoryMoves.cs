using ChessEngine.Moves;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    class HistoryMoves
    {
        /// <summary>
        /// Side,fromRank,fromFile,toRank,toFile
        /// </summary>
        private int[, , , ,] historyMoves;
        public Move WhiteBestMove { get; private set; }
        public Move BlackBestMove { get; private set; }
        public int WhiteBestMoveScore { get; private set; }
        public int BlackBestMoveScore { get; private set; }
        public HistoryMoves()
        {
            const int side = 2, fromRank = 9, fromFile = 9, toRank = 9, toFile = 9;
            historyMoves = new int[side, fromRank, fromFile, toRank, toFile];
            WhiteBestMoveScore = 0;
            BlackBestMoveScore = 0;
        }
        public void AddMove(Move move)
        {
            int side = move.Piece.Color == Color.White ? 0 : 1;//0 for white and 1 for black
            int newMoveScore = ++historyMoves[side, move.From.Rank, move.From.File, move.To.Rank, move.To.File];
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
            return historyMoves[move.Piece.Color == Color.White ? 0 : 1, move.From.Rank, move.From.File, move.To.Rank, move.To.File];
        }
    }
}
