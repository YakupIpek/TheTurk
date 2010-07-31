using System;
using ChessEngine.Main;

namespace ChessEngine
{
    class Entrance
    {
        static void Main(string[] args)
        {
            var input = Console.ReadLine();
            if (input.Contains("xboard"))
            {
                var winboard = new Winboard();
                winboard.Start();
            }

        }
    }
}
