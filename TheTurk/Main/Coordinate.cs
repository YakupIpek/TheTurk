using System;
using System.Linq;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    /// <summary>
    /// Defines Square coordinates as rank and file
    /// </summary>
    public struct Coordinate
    {
        #region Fields

        const string fileLetters = "abcdefgh";
        public static readonly Coordinate[] fourDirectionDelta;
        public static readonly Coordinate[] crossFourDirectionDelta;
        public static readonly Coordinate[] allDirectionDelta;

        public static readonly Coordinate
            a1 = new Coordinate(1, 1),
            b1 = new Coordinate(1, 2),
            c1 = new Coordinate(1, 3),
            d1 = new Coordinate(1, 4),
            e1 = new Coordinate(1, 5),
            f1 = new Coordinate(1, 6),
            g1 = new Coordinate(1, 7),
            h1 = new Coordinate(1, 8),
            a8 = new Coordinate(8, 1),
            b8 = new Coordinate(8, 2),
            c8 = new Coordinate(8, 3),
            d8 = new Coordinate(8, 4),
            e8 = new Coordinate(8, 5),
            f8 = new Coordinate(8, 6),
            g8 = new Coordinate(8, 7),
            h8 = new Coordinate(8, 8);

        private int file;
        public int File { get { return file; } }
        private int rank;
        public int Rank { get { return rank; } }
        static Coordinate()
        {


            fourDirectionDelta = new Coordinate[]{Coordinate.Directions.East,Coordinate.Directions.West,
                                                  Coordinate.Directions.South,Coordinate.Directions.North};

            crossFourDirectionDelta = new Coordinate[] { Coordinate.Directions.SouthEast, Coordinate.Directions.SouthWest,
                                                         Coordinate.Directions.NorthWest, Coordinate.Directions.NorthEast };

            allDirectionDelta = fourDirectionDelta.Concat(crossFourDirectionDelta).ToArray();



        }
        public static class Directions
        {
            public static readonly Coordinate South = new Coordinate(-1, 0), North = new Coordinate(1, 0), West = new Coordinate(0, -1),
                                              East = new Coordinate(0, 1), NorthEast = new Coordinate(1, 1), NorthWest = new Coordinate(1, -1), SouthEast = new Coordinate(-1, 1),
                                              SouthWest = new Coordinate(-1, -1);
        }

        #endregion

        public Coordinate(int rank, int file)
        {

            this.rank = rank;
            this.file = file;


        }
        public bool IsOnboard()
        {
            return rank >= 1 && rank <= 8 && file >= 1 && file <= 8;
        }
        /// <summary>
        /// return square on spesified direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Coordinate To(Coordinate direction)
        {
            Coordinate to;
            to.rank = rank + direction.rank;
            to.file = file + direction.file;
            return to;
        }
        /// <summary>
        /// Determine whether this square is empty or not
        /// </summary>
        /// <param name="board">board</param>
        /// <returns></returns>
        public bool IsEmpty(Board board)
        {
#if(DEBUG)
            if (!IsOnboard())
            {
                throw new OutOfBoardException();
            }
            
#endif

            return GetPiece(board) == null;
        }
        /// <summary>
        /// Get piece which sit on this square
        /// </summary>
        /// <param name="board">Board</param>
        /// <returns>Piece on this square</returns>
        public Piece GetPiece(Board board)
        {
            if (IsOnboard())
            {
                return board[this];
            }
            return null;
        }
        public bool Equals(Coordinate square)
        {
            return rank == square.rank && file == square.file;
        }
        public bool IsEdgeOfBoard()
        {
            return rank == 8 || rank == 1 || file == 8 || file == 1;
        }
        public static Coordinate NotationToSquare(string notation)
        {
            Coordinate coordinate = new Coordinate();
            coordinate.rank = int.Parse(notation[1].ToString());
            for (int i = 0; i < fileLetters.Length; i++)
            {
                if (fileLetters[i] == notation[0])
                    coordinate.file = ++i;
            }
            return coordinate;
        }
        public int VerticalDistance(Coordinate square)
        {
            return Math.Abs(rank - square.rank);
        }
        public int HorizontalDistance(Coordinate square)
        {
            return Math.Abs(this.file - square.file);
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
            Piece piece;
            foreach (var direction in Coordinate.crossFourDirectionDelta) //Check cross directions for queen and bishop
            {
                Coordinate to = this;
                while ((to = to.To(direction)).IsOnboard())
                {
                    piece = to.GetPiece(board);
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
                    piece = to.GetPiece(board);
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
                Coordinate to = this;
                to = to.To(direction);
                if (to.IsOnboard())
                {
                    piece = to.GetPiece(board);
                    if (piece != null)
                    {
                        if ((piece is Knight) && piece.Color == enemyColor)
                        {
                            return true;
                        }
                    }
                }
            }
            King opponentking = enemyColor == Color.White ? board.WhiteKing : board.BlackKing;

            if (this.IsNeighboreSquare(opponentking.From)) //Check for oppenent king
            {
                return true;
            }
            if (enemyColor == Color.Black) //Check for pawns
            {

                piece = this.To(Coordinate.Directions.NorthEast).GetPiece(board);
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
                piece = this.To(Coordinate.Directions.SouthEast).GetPiece(board);
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
            return new Coordinate(9 - rank, file);
        }
        public override string ToString()
        {
            return fileLetters[file - 1] + rank.ToString();
        }
    }
}
