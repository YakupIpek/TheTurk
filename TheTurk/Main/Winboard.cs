using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ChessEngine.Moves;

namespace ChessEngine.Main
{
    class Winboard : IProtocol
    {
        bool force = true;
        private readonly Engine engine;
        private Stack<Move> GameHistory;
        public Winboard()
        {
            GameHistory=new Stack<Move>();
            engine = new Engine(new Board(), this);
        }
        public void Start()
        {
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
                case "protover":
                    {
                        if (messageBody == "2")
                        {
                            Console.WriteLine("feature  setboard=1 usermove=1 reuse=1 myname=\"The Turk\" name=1");
                        }

                        break;
                    }
                case "force":
                    {
                        force = force == true ? false : true;
                        break;
                    }
                case "new":
                    {
                        engine.Board.SetUpBoard();
                        GameHistory.Clear();
                        force = true;
                        break;
                    }
                case "setboard":
                    {
                        engine.Board.Fen = messageBody;
                        GameHistory.Clear();
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
                        if (force)//Make move after received a move
                        {
                            var result = engine.Search(5);
                            if (result.BestLine.Count > 0)
                            {
                                Console.WriteLine("move " + result.BestLine.First().IONotation());
                                engine.Board.MakeMove(result.BestLine.First());
                                GameHistory.Push(result.BestLine.First());
                            }
                        }
                        break;
                    }
                case "go":
                    {
                        force = true;
                        var result = engine.Search(5);
                        if (result.BestLine.Count > 0)
                        {
                            Console.WriteLine("move " + result.BestLine.First().IONotation());
                            engine.Board.MakeMove(result.BestLine.First());
                            GameHistory.Push(result.BestLine.First());
                        }
                        break;
                    }
                case "undo":
                    {
                        engine.Board.TakeBackMove(GameHistory.Pop());
                        break;
                    }
                case "fen"://just for testing purposes
                    {
                        engine.Board.Fen = "rn4kr/4p1bp/2pp4/1p3P1Q/2P1P3/6R1/PBp3PP/RN4K1 w - - 0 24 "; 
                        GameHistory.Clear();
                        break;
                    }
                case "score"://just for testing purposes
                    {
                        var score = Evaluation.Evaluate(engine.Board);
                        engine.Board.ShowBoard();
                        Console.WriteLine("score : " + score);
                        break;
                    }
                case "show"://just for testing purposes
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
                    GameHistory.Push(move);
                    engine.Board.MakeMove(move);
                    return;
                }
            }
            Console.WriteLine("illegal move : {0}", moveNotation);

        }
        public void WriteOutput(Engine.Result result)
        {
            if (result.BestLine.Count > 0)
            {
                Console.Write("{0} {1} {2} {3} ", result.Ply, result.Score, result.ElapsedTime, result.NodesCount);

                foreach (var move in result.BestLine)
                {
                    Console.Write(move.Notation() + " ");
                }
                Console.WriteLine();
            }

        }
    }
}
