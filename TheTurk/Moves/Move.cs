using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{
    public abstract class Move
    {
        public Piece piece;
        public readonly Coordinate from;

        public Move(Piece piece)
        {
            this.piece = piece;
            this.from = piece.from;
        }
        
        public abstract void MakeMove(Board board);

        public abstract void UnMakeMove(Board board);
        
        public virtual string IONotation(){

            return piece.from.ToString();
        }
        public virtual string Notation()
        {
            return piece.notationLetter.ToString();
        }
        
    }
}
