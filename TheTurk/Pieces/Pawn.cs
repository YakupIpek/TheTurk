using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Pawn : Piece
    {
        static readonly int piecevalue = 100;
        public const char letter = ' ';
        public Pawn(Coordinate from, Color color)
            : base(from, color)
        {

        }
        public override List<Move> GenerateMoves(Board board)
        {
            var moves = new List<Move>();
            Move move;
            if (color == Piece.Color.White)
            {
                var upSquare = from.To(Coordinate.Directions.North);
                if (upSquare.IsEmpty(board))
                {
                    move = new Ordinary(board, this, upSquare);
                    moves.Add(move);
                    var twoUpSquare = upSquare.To(Coordinate.Directions.North);
                    if (from.rank == 2 && twoUpSquare.IsEmpty(board))
                    {
                        move = new Ordinary(board, this, twoUpSquare);
                        moves.Add(move);
                    }

                    Coordinate crossSquare = from.To(Coordinate.Directions.NorthEast);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.Black)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                    crossSquare = from.To(Coordinate.Directions.NorthWest);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.Black)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                }

                if (from.rank == 6)
                {
                    var crossSquare = from.To(Coordinate.Directions.NorthEast);
                    if (crossSquare.Equals(board.enPassantSquare))
                    {
                        move = new EnPassant(board, this, crossSquare);
                        moves.Add(move);
                    }
                    else
                    {
                        crossSquare = from.To(Coordinate.Directions.NorthWest);
                        if (crossSquare.Equals(board.enPassantSquare))
                        {
                            move = new EnPassant(board, this, crossSquare);
                            moves.Add(move);
                        }
                    }
                }
                if (from.rank == 7)
                {
                    upSquare = from.To(Coordinate.Directions.North);
                    if (upSquare.IsEmpty(board))
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, upSquare));

                    }
                    var crossSquare = from.To(Coordinate.Directions.NorthEast);
                    if (!crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.Black)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                    crossSquare = from.To(Coordinate.Directions.NorthWest);
                    if (!crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.Black)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                }

            }
            else // For black pawn
            {
                var downSquare = from.To(Coordinate.Directions.South);
                if (downSquare.IsEmpty(board))
                {
                    move = new Ordinary(board, this, downSquare);
                    moves.Add(move);
                    var twoDownSquare = downSquare.To(Coordinate.Directions.South);

                    if (twoDownSquare.IsEmpty(board))
                    {
                        move = new Ordinary(board, this, twoDownSquare);
                        moves.Add(move);
                    }

                }
                var crossSquare = from.To(Coordinate.Directions.SouthEast);
                if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.White)
                {
                    move = new Ordinary(board, this, crossSquare);
                    moves.Add(move);
                }
                crossSquare = from.To(Coordinate.Directions.SouthWest);
                if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.White)
                {
                    move = new Ordinary(board, this, crossSquare);
                    moves.Add(move);
                }
                if (from.rank == 3)
                {
                    crossSquare = from.To(Coordinate.Directions.SouthEast);
                    if (crossSquare.Equals(board.enPassantSquare))
                    {
                        move = new EnPassant(board, this, crossSquare);
                        moves.Add(move);
                    }
                    else
                    {
                        crossSquare = from.To(Coordinate.Directions.SouthWest);
                        if (crossSquare.Equals(board.enPassantSquare))
                        {
                            move = new EnPassant(board, this, crossSquare);
                            moves.Add(move);
                        }
                    }
                }
                if (from.rank == 2)
                {
                    downSquare = from.To(Coordinate.Directions.South);
                    if (downSquare.IsEmpty(board))
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, downSquare));

                    }
                    crossSquare = from.To(Coordinate.Directions.SouthEast);
                    if (!crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.White)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                    crossSquare = from.To(Coordinate.Directions.SouthWest);
                    if (!crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).color == Color.White)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                }

            }



            return moves;
        }
        public override bool sliding
        {
            get
            {
                return false;
            }
        }

        public override char notationLetter
        {
            get { return letter; }
        }
        public override int PieceValue
        {
            get { return color == Color.White ? piecevalue : -piecevalue; }
        }

        public override Coordinate[] pieceDirection
        {
            get { return null; }
        }
    }
}
