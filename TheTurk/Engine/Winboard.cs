﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TheTurk.Moves;
namespace TheTurk.Engine
{
    class Winboard : IProtocol
    {
        private BlockingCollection<string> commandConsumer;
        private ChessClock timer;
        private bool force;
        private readonly Engine engine;
        private Stack<Move> GameHistory;
        public Winboard()
        {
            commandConsumer = new BlockingCollection<string>();
            GameHistory = new Stack<Move>();
            engine = new Engine(new Board(), this);
            timer = new ChessClock(0, 0, 10);
            force = true;
        }
        /// <summary>
        /// Protocol starts to listen interface and respond
        /// </summary>
        public void Start()
        {
            Task.Factory.StartNew(ProcessQueue);//start new thread for consume commands
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "?") //if it is stop command, immediately stop engine without put it into queue
                {
                    engine.ExitRequested = true;
                    commandConsumer.Clear();
                    continue;
                }
                commandConsumer.Add(input);
            }
        }
        /// <summary>
        /// Process incoming commands
        /// </summary>
        /// <param name="input">Incoming string from interface</param>
        public void Comunication(string input)
        {
            string command = input.Split(' ').First();
            string messageBody = input.Substring(command.Length).Trim();
            switch (command)
            {
                case "protover":
                    {
                        if (messageBody == "2")
                        {
                            Console.WriteLine(@"feature  setboard=1 usermove=1 colors=0 reuse=1 time=1 myname=""The Turk"" name=1 done=1");
                        }
                        break;
                    }
                case "force":
                    {
                        force = !force;
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
                        engine.Board.SetUpBoard(messageBody);
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
                        if (force) //Make move after received a move
                        {
                            Console.WriteLine("force mode is on");
                            var result = engine.Search(timer.timeForPerMove);
                            Console.WriteLine("move " + result.BestLine.First().IONotation());
                            engine.Board.MakeMove(result.BestLine.First());
                            GameHistory.Push(result.BestLine.First());
                        }
                        break;
                    }
                case "go":
                    {
                        force = true;
                        var result = engine.Search(timer.timeForPerMove);
                        Console.WriteLine("move " + result.BestLine.First().IONotation());
                        engine.Board.MakeMove(result.BestLine.First());
                        GameHistory.Push(result.BestLine.First());
                        break;
                    }
                case "analyze":
                    {
                        force = true;
                        engine.Search(timer.timeForPerMove);
                        break;
                    }
                case "undo":
                    {
                        engine.Board.UndoMove(GameHistory.Pop());
                        break;
                    }
                case "level":
                    {
                        timer = new ChessClock(messageBody);
                        break;
                    }
                case "st":
                    {
                        timer = new ChessClock("0 0 " + messageBody);
                        break;
                    }
                case "show": //just for testing purposes
                    {
                        engine.Board.ShowBoard();
                        break;
                    }
            }
        }
        public void ProcessQueue()
        {
            while (true)
            {
                Comunication(commandConsumer.Take());
            }

        }
        private void ReceivedMove(string moveNotation)
        {
            var from = Coordinate.NotationToSquare(moveNotation.Substring(0, 2));//convert string notation coordinate

            var move = from.GetPiece(engine.Board)
                           .GenerateMoves(engine.Board)
                           .FirstOrDefault(m => m.IONotation() == moveNotation);

            if (move != null)
            {
                GameHistory.Push(move);
                engine.Board.MakeMove(move);
                return;
            }

            Console.WriteLine("illegal move : " + moveNotation);
        }

        public void WriteOutput(EngineResult result)
        {
            if (result.BestLine.Count > 0)
            {
                Console.Write("{0} {1} {2} {3} ", result.Ply, result.Score, result.ElapsedTime, result.NodesCount);

                result.BestLine.ForEach(move => Console.Write(move.Notation() + " "));

                Console.WriteLine();
            }
        }
    }
}
