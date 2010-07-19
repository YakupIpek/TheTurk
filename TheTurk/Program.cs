using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine.Pieces;
using ChessEngine.Moves;
using ChessEngine;
namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            var from = new Coordinate(2,1);
            var queen = new  Pawn(from,Piece.Color.White);
            var board= new ChessEngine.Board();
            var moves =queen.GenerateMoves(board);
            foreach (var move in moves)
            {
                Console.WriteLine(move.Notation());
            }
            Console.ReadKey();
        }
    }
}
