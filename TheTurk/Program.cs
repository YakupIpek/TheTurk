using System;
using System.Diagnostics;
using ChessEngine;
using ChessEngine.Test;

namespace Main
{
    class Program
    {
        static void Main(string[] args)
        {
            Board board=new Board();
            board.SetUpBoard();
            Stopwatch süre= new Stopwatch();
            süre.Start();
            MoveGeneration.MinMax(board,5);
            süre.Stop();
            Console.WriteLine(süre.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
