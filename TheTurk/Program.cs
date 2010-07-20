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
            
            
            var board= new ChessEngine.Board();
            board.SetUpBoard();
            
            
            board[new Coordinate(2,5)] = null;
            board.ShowBoard();
            var square = new Coordinate(2,6);
            var piece = board[square];
            var moves =piece.GenerateMoves(board);
            foreach (var move in moves)
            {
                Console.WriteLine(move.Notation());
            }
            Console.ReadKey();
        }
    }
}
