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

        public virtual string IONotation()
        {

            return piece.From.ToString();
        }
        public virtual string Notation()
        {
            return piece.NotationLetter.ToString().Replace('P', ' ');
        }

    }
}
