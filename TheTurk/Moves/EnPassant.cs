using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{
    public class EnPassant : Ordinary
    {



        public EnPassant(Board board, Piece piece,Coordinate to):base(board,piece,to)
        {
            
        }

        public override string Notation()
        {
            return from.ToString()[0]+ "x" + to.ToString() + " (e.p)";
        }
    }
}
