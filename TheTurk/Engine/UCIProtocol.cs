using System.Text;
using System.Threading.Channels;
using TheTurk.Moves;
using TheTurk.Pieces;

namespace TheTurk.Engine;

public class UCIProtocol
{
    private readonly ChessEngine engine;

    public UCIProtocol()
    {
        engine = new ChessEngine(new Board());
    }

    public async Task Start()
    {
        var channel = Channel.CreateUnbounded<string[]>(new() { SingleReader = true, SingleWriter = true });

        var listener = CommandListener(channel.Writer);

        var handler = CommandHandlerAsync(channel.Reader);

        await Task.WhenAll(listener, handler);

    }

    public async Task CommandListener(ChannelWriter<string[]> writer)
    {

        using var stream = Console.OpenStandardInput();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (true)
        {
            var input = await reader.ReadLineAsync();

            var command = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (engine.ExitRequested)
            {
                continue;
            }

            if (command is ["stop"])
            {
                engine.ExitRequested = true;
                continue;
            }

            await writer.WriteAsync(command);
        }
    }

    private async Task CommandHandlerAsync(ChannelReader<string[]> reader)
    {

        await foreach (var command in reader.ReadAllAsync())
        {
            ProcessCommand(command);
        }
    }

    private void ProcessCommand(string[] command)
    {
        switch (command)
        {
            case []: break;
            case ["uci"]:
                Console.WriteLine("id name TheTurk");
                Console.WriteLine("id author Yakup Ipek");
                Console.WriteLine("uciok");
                break;

            case ["isready"]:
                Console.WriteLine("readyok");
                break;

            case ["ucinewgame"]:
                engine.Board.SetUpBoard();
                break;

            case ["position", "fen", string f, string e, string n, string s, string t, string r, "moves", .. var moves]:
                engine.Board.SetUpBoard(string.Join(' ', f, e, n, s, t, r));
                ApplyMoves(moves);
                break;

            case ["position", "fen", .. var fen]:
                engine.Board.SetUpBoard(string.Join(' ', fen));
                break;

            case ["position", "startpos"]:
                engine.Board.SetUpBoard();
                break;

            case ["position", "startpos", "moves", .. var moves]:
                engine.Board.SetUpBoard();
                ApplyMoves(moves);
                break;

            case ["go", "movetime", string time] when int.TryParse(time, out var duration):
                HandleGo(duration);
                break;

            case ["go", "infinite"]:
                HandleGo(int.MaxValue);
                break;

            case ["show"]:
                engine.Board.ShowBoard();
                break;

            case ["quit"]:
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("[DEBUG] Unknown command!");
                break;
        }
    }

    private void ApplyMoves(string[] moves)
    {
        foreach (var moveNotation in moves)
        {
            var from = Coordinate.NotationToSquare(moveNotation);//convert string notation coordinate

            var board = engine.Board;

            var king = board.Side == Color.White ? board.WhiteKing : board.BlackKing;

            var move = from.GetPiece(engine.Board)
                           .GenerateMoves(engine.Board)
                           .FirstOrDefault(m => m.IONotation() == moveNotation);

            if (move is null)
            {
                Console.WriteLine("illegal move : " + moveNotation);
                return;
            }

            var state = board.MakeMove(move);

            var attacked = king.From.IsAttackedSquare(board, king.OppenentColor);


            if (attacked)
            {
                Console.WriteLine($"illegal move : {moveNotation} - Reason : King exposed to check.");

                board.UndoMove(move, state);
                return;
            }
        }
    }

    private void HandleGo(int time)
    {
        var results = engine.Search(time);

        var bestLine = Array.Empty<Move>();

        foreach (var result in results)
        {
            WriteOutput(result);
            bestLine = result.BestLine;
        }

        engine.Board.MakeMove(bestLine.First());

        Console.WriteLine("bestmove " + bestLine.First().IONotation());
    }


    public static void WriteOutput(EngineResult result)
    {
        if (result.BestLine.Any())
        {
            var score = result.MateIn != 0 ? $"mate {result.MateIn}" : $"cp {result.Score}";

            Console.Write($"info depth {result.Ply} score {score} nodes {result.NodesCount} time {result.ElapsedTime} pv ");

            foreach (var move in result.BestLine)
            {
                Console.Write(move.IONotation() + " ");
            }

            Console.WriteLine();
        }
    }

}
