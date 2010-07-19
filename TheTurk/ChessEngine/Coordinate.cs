using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine.Pieces;
namespace ChessEngine
{
    /// <summary>
    /// Defines Square coordinates as rank and file
    /// </summary>
    public struct Coordinate
    {
        public int rank;
        public int file;
        const string fileLetters = "abcdefgh";
        public static readonly Coordinate[] fourDirection;
        public static readonly Coordinate[] crossFourDirection;
        public static readonly Coordinate[] allDirection;
        static Coordinate()
        {


            fourDirection = new Coordinate[]{Coordinate.Directions.East,Coordinate.Directions.West,
                Coordinate.Directions.South,Coordinate.Directions.North};

            crossFourDirection = new Coordinate[] { Coordinate.Directions.SouthEast, Coordinate.Directions.SouthWest,
                Coordinate.Directions.NorthWest, Coordinate.Directions.NorthEast };

            allDirection = fourDirection.Concat(crossFourDirection).ToArray();
        }
        public static class Directions
        {
            public static readonly Coordinate South = new Coordinate(-1, 0), North = new Coordinate(1, 0), West = new Coordinate(0, -1),
                East = new Coordinate(0, 1), NorthEast = new Coordinate(1, 1), NorthWest = new Coordinate(1, -1), SouthEast = new Coordinate(-1, 1),
                SouthWest = new Coordinate(-1, -1);
        }
        public static readonly Coordinate
            a1 = new Coordinate(1, 1),
            c1 = new Coordinate(1, 3),
            d1 = new Coordinate(1, 4),
            e1 = new Coordinate(1, 5),
            f1 = new Coordinate(1, 6),
            g1 = new Coordinate(1, 7),
            h1 = new Coordinate(1, 8),
            a8 = new Coordinate(8, 1),
            c8 = new Coordinate(8, 3),
            d8 = new Coordinate(8, 4),
            e8 = new Coordinate(8, 5),
            f8 = new Coordinate(8, 6),
            g8 = new Coordinate(8, 7),
            h8 = new Coordinate(8, 8);
        public Coordinate(int rank, int file)
        {

            this.rank = rank;
            this.file = file;

        }
        public override string ToString()
        {
            return fileLetters[file - 1] + rank.ToString();
        }
        public bool IsOnboard()
        {
            return rank >= 1 && rank <= 8 && file >= 1 && file <= 8;
        }
        public Coordinate To(Coordinate direction)
        {
            Coordinate to;
            to.rank = rank + direction.rank;
            to.file = file + direction.file;
            return to;
        }

        public bool IsEmpty(Board board)
        {
            return board[this] == null;
        }
        public Piece GetPiece(Board board)
        {
            return board[this];
        }
        public bool Equals(Coordinate Square)
        {
            return rank == Square.rank && file == Square.file;
        }
    }
}
