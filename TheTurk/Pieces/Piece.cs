using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public enum Color
    {
        White=1, Black=-1
    }
    public abstract class Piece
    {
        public Piece(Coordinate from, Color color)
        {
            this.From = from;
            this.Color = color;

        }

        public Coordinate From { get; protected set; }
        public virtual bool Sliding { get { return true; } }
        public Color Color { get; private set; }
        public Color OppenentColor
        {
            get { return  Color == Pieces.Color.White ? Pieces.Color.Black: Pieces.Color.White; }
        }
        public abstract char NotationLetter { get; }
        public abstract int PieceValue { get; }
        public abstract Coordinate[] PieceDirection { get; }

        public virtual List<Move> GenerateMoves(Board board)
        {
            List<Move> moves = new List<Move>();

            foreach (var direction in this.PieceDirection)
            {
                Coordinate destination = From;
                Piece piece;
                while ((destination = destination.To(direction)).IsOnboard())
                {
                    piece = board[destination];
                    if (piece != null && this.Color == piece.Color) break;

                    moves.Add(new Ordinary(board, this, destination));

                    if (!Sliding||(piece != null && piece.Color != this.Color)) break;
                    
                }

            }
            return moves;
        }

        /// <summary>
        /// piece goes specified square
        /// </summary>
        /// <param name="board"></param>
        /// <param name="to"></param>
        public void MoveTo(Board board, Coordinate to)
        {
            RemoveMe(board);
            board[to] = this;
            From = to;
        }
        public virtual int Evaluation()
        {
            return 0;
        }
        /// <summary>
        /// Remove itself on board
        /// </summary>
        public void RemoveMe(Board board)
        {
            board[From] = null;
        }
        public void PutMe(Board board)
        {
            board[From] = this;
        }


    }
}
