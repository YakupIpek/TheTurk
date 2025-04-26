using TheTurk.Engine;
using TheTurk.Pieces;

namespace TheTurk.Moves
{
    public abstract class Move
    {
        public Coordinate From { get; protected set; }
        public Coordinate To { get; protected set; }
        public Piece Piece { get; protected set; }

        public Move(Piece piece)
        {
            Piece = piece;
            From = piece.From;
        }

        public abstract void MakeMove(Board board);

        public abstract void UndoMove(Board board);

        /// <summary>
        /// Used for sorting.Better to worst
        /// </summary>
        /// <returns></returns>
        public abstract int MovePriority();
        /// <summary>
        /// This notation used for protocol comminication
        /// </summary>
        /// <returns></returns>
        public virtual string IONotation()
        {
            return From.ToString();
        }
        public virtual string Notation()
        {
            return Piece.NotationLetter.ToString().Trim();
        }
        public virtual bool Equals(Move? move)
        {
            return null != move && move.Piece == Piece && move.GetType() == GetType() && move.From.Equals(From);
        }

    }
}
