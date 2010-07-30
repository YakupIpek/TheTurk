using System;
using System.Linq;
using System.Threading;

namespace ChessEngine.Main
{
    class Winboard
    {
        bool force = false;
        private readonly Engine engine;
        public Winboard()
        {
            engine = new Engine(new Board(),SendOutput);
        }
        public void Start()
        {
            Thread.Sleep(200);
            Console.WriteLine("protover 2");
            Thread.Sleep(100);
            Console.WriteLine("feature  setboard=1 usermove=1 reuse=1 myname=\"the turk\" name=1");
            while (true)
            {
                string input = Console.ReadLine();
                Comminication(input);
            }
        }
        void Comminication(string input)
        {

            string command = input.Split(' ').First();
            string messageBody = input.Substring(command.Length).Trim();
            switch (command)
            {
                case "force":
                    {
                        force = force == true ? false : true;
                        break;
                    }
                case "new":
                    {
                        engine.Board.SetUpBoard();
                        force = false;
                        break;
                    }
                case "setboard":
                    {
                        engine.Board.Fen = messageBody;
                        break;
                    }
                case "quit":
                    {
                        Environment.Exit(Environment.ExitCode);
                        break;
                    }
                case "usermove":
                    {
                        ReceivedMove(messageBody);
                        if (!force)
                        {
                            var result = engine.Search(4);
                            Console.WriteLine("move " + result.BestLine[0].IONotation());
                            engine.Board.MakeMove(result.BestLine[0]);
                        }
                        break;
                    }
                case "go":
                    {
                        force = false;
                        var result = engine.Search(4);
                        Console.WriteLine("move " + result.BestLine[0].IONotation());
                        engine.Board.MakeMove(result.BestLine[0]);
                        break;
                    }
                case "fen":
                    {
                        engine.Board.Fen = "7k/8/8/8/1p6/P7/8/7K b - - 0 1";break;
                    }
                case "score":
                    {
                        var score =Evaluation.Evaluate(engine.Board);
                        engine.Board.ShowBoard();
                        Console.WriteLine("score : "+score);
                        break;
                    }
                case "show":
                    {
                        engine.Board.ShowBoard();
                        break;
                    }
            }
        }
        void ReceivedMove(string moveNotation)
        {
            var moves = engine.Board.GenerateMoves();
            foreach (var move in moves)
            {
                if (move.IONotation() == moveNotation)
                {
                    engine.Board.MakeMove(move);
                    break;
                }
            }
        }
        void SendOutput(Engine.Result result)
        {
            Console.Write(result.Score + " ");
            foreach (var move in result.BestLine)
            {
                Console.Write(move.Notation() + " ");

            }
            Console.WriteLine();
        }
    }
}
