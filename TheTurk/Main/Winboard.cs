using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChessEngine.Moves;
namespace ChessEngine.Main
{
    class Winboard : IProtocol
    {
        private BlockingCollection<string> commandConsumer;
        private XElement XLogger;
        private StringBuilder Logger;
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
            Logger = new StringBuilder();
            XLogger = new XElement("root");
        }
        /// <summary>
        /// Protocol starts to listen interface and respond
        /// </summary>
        public void Start()
        {
            Action<string> com = Comminication;
            Task.Factory.StartNew(ProcessQueue);//start new thread for consume commands
            while (true)
            {
                string input = Console.ReadLine();
                Logger.AppendLine(input);
                XLogger.Add(new XElement("incoming", input));
                if (input == "?") //if it is stop command, immediately stop engine without put it into queue
                {
                    engine.Exit = true;
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
        public void Comminication(string input)
        {
            string command = input.Split(' ').First();
            string messageBody = input.Substring(command.Length).Trim();
            switch (command)
            {
                case "protover":
                    {
                        if (messageBody == "2")
                        {
                            WriteLine(@"feature  setboard=1 usermove=1 colors=0 reuse=1 time=1 myname=""The Turk"" name=1 done=1");
                        }
                        break;
                    }
                case "force":
                    {
                        force = force ? false : true;
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
                        if (force) //Make move after received a move
                        {
                            Console.WriteLine("force mode is on");
                            var result = engine.Search(timer.timeForPerMove);
                            WriteLine("move " + result.BestLine.First().IONotation());
                            engine.Board.MakeMove(result.BestLine.First());
                            GameHistory.Push(result.BestLine.First());
                        }
                        break;
                    }
                case "go":
                    {
                        force = true;
                        var result = engine.Search(timer.timeForPerMove);
                        WriteLine("move " + result.BestLine.First().IONotation());
                        engine.Board.MakeMove(result.BestLine.First());
                        GameHistory.Push(result.BestLine.First());
                        break;
                    }
                case "undo":
                    {
                        engine.Board.TakeBackMove(GameHistory.Pop());
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
                case "fen": //just for testing purposes
                    {
                        engine.Board.Fen = "rn4kr/4p1bp/2pp4/1p3P1Q/2P1P3/6R1/PBp3PP/RN4K1 w - - 0 24 ";
                        GameHistory.Clear();
                        break;
                    }
                case "score": //just for testing purposes
                    {
                        var score = Evaluation.Evaluate(engine.Board);
                        engine.Board.ShowBoard();
                        Console.WriteLine("score : " + score);
                        break;
                    }
                case "show": //just for testing purposes
                    {
                        engine.Board.ShowBoard();
                        break;
                    }
                case "exportlog"://just for testing purposes
                    {
                        TextWriter txtfile = new StreamWriter("TheTurkLog");
                        txtfile.Write(Logger);
                        txtfile.Flush();
                        txtfile.Close();
                        Process.Start("notepad.exe", "TheTurkLog");
                        break;
                    }
                case "xmllog"://just for testing purposes
                    {
                        XLogger.Save("xmllog.xml");
                        Process.Start("notepad.exe", "xmllog.xml");
                        break;
                    }
            }
        }
        public void ProcessQueue()
        {
            while (true)
            {
                Comminication(commandConsumer.Take());
            }

        }
        private void ReceivedMove(string moveNotation)
        {
            try
            {
                var from = Coordinate.NotationToSquare(moveNotation.Substring(0, 2));//convert string notation coordinate
                var moves = from.GetPiece(engine.Board).GenerateMoves(engine.Board);
                foreach (var move in moves)
                {
                    if (move.IONotation() == moveNotation)
                    {
                        GameHistory.Push(move);
                        engine.Board.MakeMove(move);
                        return;
                    }
                }
                throw new NullReferenceException();
            }
            catch (NullReferenceException)
            {
                WriteLine("illegal move : " + moveNotation);

            }
        }
        public void WriteOutput(Engine.Result result)
        {
            if (result.BestLine.Count > 0)
            {
                Console.Write("{0} {1} {2} {3} ", result.Ply, result.Score, result.ElapsedTime / 10L, result.NodesCount);

                foreach (var move in result.BestLine)
                {
                    Console.Write(move.Notation() + " ");
                }
                Console.WriteLine();
            }
        }
        private void WriteLine(string line)
        {
            Logger.AppendLine(line);
            Console.WriteLine(line);
        }
    }
}
