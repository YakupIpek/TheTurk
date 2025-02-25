using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    /// <summary>
    /// Defines Square coordinates as rank and file
    /// </summary>
    public readonly record struct Coordinate
    {
        #region Fields

        const string fileLetters = "abcdefgh";
        public static readonly Coordinate[] fourDirectionDelta;
        public static readonly Coordinate[] crossFourDirectionDelta;
        public static readonly Coordinate[] allDirectionDelta;

        public static readonly Coordinate
            a1 = new(1, 1),
            b1 = new(1, 2),
            c1 = new(1, 3),
            d1 = new(1, 4),
            e1 = new(1, 5),
            f1 = new(1, 6),
            g1 = new(1, 7),
            h1 = new(1, 8),
            a8 = new(8, 1),
            b8 = new(8, 2),
            c8 = new(8, 3),
            d8 = new(8, 4),
            e8 = new(8, 5),
            f8 = new(8, 6),
            g8 = new(8, 7),
            h8 = new(8, 8);

        public int File { get; }
        public int Rank { get; }

        public int Index { get; }

        static Coordinate()
        {

            fourDirectionDelta = [Directions.East, Directions.West, Directions.South, Directions.North];

            crossFourDirectionDelta = [Directions.SouthEast, Directions.SouthWest, Directions.NorthWest, Directions.NorthEast];

            allDirectionDelta = fourDirectionDelta.Concat(crossFourDirectionDelta).ToArray();



        }
        public static class Directions
        {
            public static readonly Coordinate
                South = new(-1, 0),
                North = new(1, 0),
                West = new(0, -1),
                East = new(0, 1),
                NorthEast = new(1, 1),
                NorthWest = new(1, -1),
                SouthEast = new(-1, 1),
                SouthWest = new(-1, -1);
        }

        #endregion

        public Coordinate(int rank, int file)
        {

            Rank = rank;
            File = file;
            Index = (rank-1) * 8 + file - 1;
        }
        public bool IsOnboard()
        {
            return Rank >= 1 && Rank <= 8 && File >= 1 && File <= 8;
        }


        /// <summary>
        /// return square on spesified direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Coordinate To(Coordinate direction)
        {
            return new Coordinate(Rank + direction.Rank, File + direction.File);
        }
        /// <summary>
        /// Determine whether this square is empty or not
        /// </summary>
        /// <param name="board">board</param>
        /// <returns></returns>
        public bool IsEmpty(Board board)
        {
            return GetPiece(board) == null;
        }
        /// <summary>
        /// Get piece which sit on this square
        /// </summary>
        /// <param name="board">Board</param>
        /// <returns>Piece on this square</returns>
        public Piece GetPiece(Board board)
        {

                return board[this];
        }
        public bool Equals(Coordinate square)
        {
            return Rank == square.Rank && File == square.File;
        }
        public bool IsEdgeOfBoard()
        {
            return Rank == 8 || Rank == 1 || File == 8 || File == 1;
        }
        public static Coordinate NotationToSquare(string notation)
        {
            int rank = int.Parse(notation[1].ToString());
            int file = 0;
            for (int i = 0; i < fileLetters.Length; i++)
            {
                if (fileLetters[i] == notation[0])
                    file = i + 1;
            }
            return new Coordinate(rank, file);
        }
        public int VerticalDistance(Coordinate square)
        {
            return Math.Abs(Rank - square.Rank);
        }
        public int HorizontalDistance(Coordinate square)
        {
            return Math.Abs(this.File - square.File);
        }
        public bool IsNeighboreSquare(Coordinate square)
        {
            return this.HorizontalDistance(square) <= 1 && this.VerticalDistance(square) <= 1;
        }
        /// <summary>
        /// Is square attacked by oppenent pieces
        /// </summary>
        /// <param name="board">Board</param>
        /// <param name="enemyColor">Enemy side color</param>
        /// <returns></returns>
        public bool IsAttackedSquare(Board board, Color enemyColor)
        {

            foreach (var direction in Coordinate.crossFourDirectionDelta) //Check cross directions for queen and bishop
            {
                Coordinate to = this;
                while ((to = to.To(direction)).IsOnboard())
                {
                    var piece = to.GetPiece(board);
                    if (piece != null)
                    {
                        if ((piece is Queen || piece is Bishop) && piece.Color == enemyColor)
                        {
                            return true;
                        }
                        break;
                    }
                }
            }
            foreach (var direction in Coordinate.fourDirectionDelta)//check horizonal and vertical directions for queen and rook
            {
                Coordinate to = this;
                while ((to = to.To(direction)).IsOnboard())
                {
                    var piece = to.GetPiece(board);
                    if (piece != null)
                    {
                        if ((piece is Queen || piece is Rook) && piece.Color == enemyColor)
                        {
                            return true;
                        }
                        break;
                    }
                }
            }

            foreach (var direction in Knight.Directions) // Knight directions
            {
                var to = this.To(direction);

                if (to.IsOnboard())
                {
                    var piece = to.GetPiece(board);

                    if (piece != null && (piece is Knight) && piece.Color == enemyColor)
                    {
                        return true;
                    }
                }
            }

            var opponentking = enemyColor == Color.White ? board.WhiteKing : board.BlackKing;

            if (this.IsNeighboreSquare(opponentking.From)) //Check for oppenent king
            {
                return true;
            }
            if (enemyColor == Color.Black) //Check for pawns
            {

                var piece = this.To(Coordinate.Directions.NorthEast).GetPiece(board);
                if (piece != null && piece is Pawn && piece.Color == enemyColor)
                {
                    return true;
                }
                piece = this.To(Coordinate.Directions.NorthWest).GetPiece(board);
                if (piece != null && piece is Pawn && piece.Color == enemyColor)
                {
                    return true;
                }
            }
            else // Black Pawns
            {
                var piece = this.To(Coordinate.Directions.SouthEast).GetPiece(board);
                if (piece != null && piece is Pawn && piece.Color == enemyColor)
                {
                    return true;
                }
                piece = this.To(Coordinate.Directions.SouthWest).GetPiece(board);
                if (piece != null && piece is Pawn && piece.Color == enemyColor)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Determine white square's mirror by black
        /// </summary>
        /// <returns>return black mirror square for white square</returns>
        public Coordinate GetMirror()
        {
            return new Coordinate(9 - Rank, File);
        }
        public override string ToString()
        {
            return fileLetters[File - 1] + Rank.ToString();
        }
    }
}
