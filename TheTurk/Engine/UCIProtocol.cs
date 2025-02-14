using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TheTurk.Moves;

namespace TheTurk.Engine;

public class UCIProtocol : IProtocol
{
    private BlockingCollection<string> commandQueue;
    private readonly Engine engine;
    private Stack<Move> gameHistory;
    private bool isUciMode = false;

    public UCIProtocol()
    {
        commandQueue = new BlockingCollection<string>();
        gameHistory = new Stack<Move>();
        engine = new Engine(new Board(), this);
    }

    public void Start()
    {
        Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        while (true)
        {
            string input = Console.ReadLine();
            commandQueue.Add(input);
        }
    }

    private void ProcessCommand(string input)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (tokens)
        {
            case []: break;
            case ["uci"]:
                Console.WriteLine("id name TheTurk");
                Console.WriteLine("id author Yakup Ipek");
                Console.WriteLine("uciok");
                isUciMode = true;
                break;

            case ["isready"]:
                Console.WriteLine("readyok");
                break;

            case ["ucinewgame"]:
                engine.Board.SetUpBoard();
                gameHistory.Clear();
                break;

            case ["position", "fen", string f, string e, string n, string s, string t, string r, "moves", .. var moves]:
                engine.Board.SetUpBoard(string.Join(' ', f, e, n, s, t, r));
                gameHistory.Clear();
                ApplyMoves(moves);
                break;

            case ["position", "fen", .. var fen]:
                engine.Board.SetUpBoard(string.Join(' ', fen));
                gameHistory.Clear();
                break;

            case ["position", "startpos"]:
                engine.Board.SetUpBoard();
                gameHistory.Clear();
                break;

            case ["position", "startpos", "moves", .. var moves]:
                gameHistory.Clear();
                engine.Board.SetUpBoard();
                ApplyMoves(moves);
                break;

            case ["go", "movetime", string time] when int.TryParse(time, out var duration):
                HandleGo(duration);
                break;

            case ["go", "infinite"]:
                engine.Search(int.MaxValue);
                break;

            case ["back"]:
                engine.Board.UndoMove(gameHistory.Pop());
                break;

            case ["stop"]:
                engine.Exit = true;
                Console.WriteLine("bestmove " + engine.previousPV.First().IONotation());
                break;

            case ["show"]:
                engine.Board.ShowBoard();
                break;

            case ["quit"]:
                Environment.Exit(0);
                break;
        }
    }

    private void ApplyMoves(string[] moves)
    {
        foreach (var moveNotation in moves)
        {
            var from = Coordinate.NotationToSquare(moveNotation);//convert string notation coordinate

            var move = from.GetPiece(engine.Board)
                           .GenerateMoves(engine.Board)
                           .FirstOrDefault(m => m.IONotation() == moveNotation);

            if (move is null)
            {
                Console.WriteLine("illegal move : " + moveNotation);
                return;
            }

            gameHistory.Push(move);
            engine.Board.MakeMove(move);
        }
    }

    private void HandleGo(int time)
    {
        var result = engine.Search(time);
        engine.Board.MakeMove(result.BestLine.First());
        gameHistory.Push(result.BestLine.First());
        Console.WriteLine("bestmove " + result.BestLine.First().IONotation());
    }

    private string GetBestMove()
    {
        var result = engine.Search(1000);
        return result.BestLine.First().IONotation();
    }

    private void ProcessQueue()
    {
        while (true)
        {
            ProcessCommand(commandQueue.Take());
        }
    }

    public void WriteOutput(EngineResult result)
    {
        if (result.BestLine.Count > 0)
        {
            var score = result.MateIn != 0 ? $"mate {result.MateIn}" : $"cp {result.Score}";

            Console.Write($"info depth {result.Ply} score {score} nodes {result.NodesCount} time {result.ElapsedTime} pv ");

            result.BestLine.ForEach(move => Console.Write(move.IONotation() + " "));

            Console.WriteLine();
        }
    }
}
