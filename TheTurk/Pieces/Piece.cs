using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public abstract class Piece
    {
        public Coordinate from;
        public virtual bool sliding { get { return true; } }
        public enum Color
        {
            White, Black
        }
        public Piece(Coordinate from, Color color)
        {
            this.from = from;
            this.color = color;

        }
        public virtual List<Move> GenerateMoves(Board board)
        {
            List<Move> moves = new List<Move>();

            foreach (var direction in this.pieceDirection)
            {
                Coordinate destination = from;
                Piece piece;
                while ((destination = destination.To(direction)).IsOnboard())
                {
                    piece = board[destination];
                    if (piece != null && this.color == piece.color) break;

                    moves.Add(new Ordinary(board, this, destination));

                    if (!sliding||(piece != null && piece.color != this.color)) break;
                    
                }

            }
            return moves;
        }
        public virtual int Evaluation()
        {
            return 0;
        }
        public void MoveTo(Board board, Coordinate to)
        {
            RemoveMe(board);
            board[to] = this;
            from = to;
        }
        /// <summary>
        /// Remove itself on board
        /// </summary>
        public void RemoveMe(Board board)
        {
            board[from] = null;
        }
        public void PutMe(Board board)
        {
            board[from] = this;
        }
        public abstract char notationLetter { get; }
        public abstract int PieceValue { get; }
        public abstract Coordinate[] pieceDirection { get; }
        public Color color { get; private set; }

    }
}
