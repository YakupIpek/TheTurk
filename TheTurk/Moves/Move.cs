using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public abstract class Move
    {
        public readonly Coordinate from;
        public Piece piece;

        public Move(Piece piece)
        {
            this.piece = piece;
            this.from = piece.From;
        }

        public abstract void MakeMove(Board board);
        public abstract void UnMakeMove(Board board);
        public abstract int MovePriority();
        public virtual string IONotation()
        {

            return piece.From.ToString();
        }
        public virtual string Notation()
        {
            return piece.NotationLetter.ToString().Trim();
        }
        public virtual bool Equals(Move move)
        {
            return null != move && move.GetType() == GetType() && move.from.Equals(from);
        }

    }
}
