using System.Collections.Generic;
using TheTurk.Engine;
using TheTurk.Moves;

namespace TheTurk.Pieces
{
    public enum Color
    {
        White = 1, Black = -1
    }

    public static class ColorExtensions
    {
        public static Color Oppenent(this Color color)
        {
            return color == Color.White ? Color.Black : Color.White;
        }
    }
    public abstract class Piece
    {
        protected Piece(Coordinate from, Color color)
        {
            From = from;
            Color = color;

        }
        public Coordinate From { get; protected set; }
        public virtual bool Sliding { get { return true; } }
        public Color Color { get; private set; }
        public Color OppenentColor => Color == Color.White ? Color.Black : Color.White;
        public abstract char NotationLetter { get; }
        public abstract int PieceValue { get; }
        public abstract Coordinate[] PieceDirection { get; }
        public abstract int[,] PieceSquareTable { get; }
        /// <summary>
        /// Generate pseudo moves
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public virtual IEnumerable<Move> GenerateMoves(Board board)
        {
            foreach (var direction in this.PieceDirection)
            {
                Coordinate destination = From;

                while ((destination = destination.To(direction)).IsOnboard())
                {
                    var enemyPiece = board[destination];

                    if (Color == enemyPiece?.Color)
                        break;

                    yield return new Ordinary(board, this, destination);

                    if (!Sliding || (enemyPiece != null && enemyPiece.Color != this.Color))
                        break;

                }
            }
        }
        public virtual int PieceSquareValue()
        {
            if (Color == Color.White)
            {
                var mirrorSquare = From.GetMirror();
                return PieceSquareTable[mirrorSquare.Rank - 1, mirrorSquare.File - 1];
            }
            else
            {
                return -PieceSquareTable[From.Rank - 1, From.File - 1];
            }
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
        /// <summary>
        /// Evaluate piece
        /// </summary>
        /// <returns></returns>
        public virtual int Evaluation()
        {
            int evaluation = PieceValue;
            evaluation += PieceSquareValue();
            return evaluation;
        }
        /// <summary>
        /// Remove itself on board
        /// </summary>
        public void RemoveMe(Board board)
        {
            board[From] = null;
        }
        /// <summary>
        /// Put itself on board if it is captured.
        /// </summary>
        /// <param name="board"></param>
        public void PutMe(Board board)
        {
            board[From] = this;
        }
        public override string ToString()
        {
            return GetType().Name;
        }
        public abstract int Number { get; }
    }
}
