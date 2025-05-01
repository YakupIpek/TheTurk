using System.Text;
using System.Threading.Channels;
using TheTurk.Bitboards;

namespace TheTurk.Engine;

public class UCIProtocol
{
    public ChessEngine engine;
    public BoardState board;

    public UCIProtocol()
    {
        engine = new ChessEngine();
        board = Notation.GetStartingPosition();
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

    public void ProcessCommand(string[] command)
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
                engine = new ChessEngine(); //? remove or keep it ?
                board = Notation.GetStartingPosition();
                ApplyMoves([]);
                break;

            case ["position", "fen", string f, string e, string n, string s, string t, string r, "moves", .. var moves]:

                board = Notation.GetBoardState(string.Join(' ', f, e, n, s, t, r));
                ApplyMoves(moves);

                break;

            case ["position", "fen", .. var fen]:

                board = Notation.GetBoardState(string.Join(' ', fen));
                ApplyMoves([]);

                break;

            case ["position", "startpos"]:
                board = Notation.GetStartingPosition();
                ApplyMoves([]);
                break;

            case ["position", "startpos", "moves", .. var moves]:
                board = Notation.GetStartingPosition();

                ApplyMoves(moves);
                break;

            case ["go", "movetime", string time] when int.TryParse(time, out var duration):
                HandleGo(duration);
                break;

            case ["go", "infinite"]:
                HandleGo(int.MaxValue);
                break;

            case ["show"]:
                //engine.Board.ShowBoard();
                break;

            case ["zobrist"]:
                Console.WriteLine(board.ZobristKey);
                break;

            case ["quit"]:
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("[DEBUG] Unknown command!");
                break;
        }
    }

    public void ApplyMoves(string[] moves)
    {
        engine.RepetitionDetector.Add(board.ZobristKey, false);

        foreach (var moveNotation in moves)
        {
            var from = Notation.GetSquare(moveNotation);//convert string notation coordinate

            var move = Notation.GetMoveUci(board, moveNotation);

            var next = new BoardState();

            if (!next.Play(board, move))
            {
                Console.WriteLine("illegal move : " + moveNotation);
                return;
            }

            var cancel = move.CapturedPieceType() is Piece.None || move.MovingPieceType() is Piece.Pawn;

            engine.RepetitionDetector.Add(next.ZobristKey, cancel);

            board = next;
        }
    }

    private void HandleGo(int time)
    {
        var results = engine.Run(board, time);

        var bestLine = new List<Move>();

        foreach (var result in results)
        {
            WriteOutput(result);
            bestLine = result.BestLine;
        }

        Console.WriteLine("bestmove " + bestLine.First());
    }

    public static void WriteOutput(EngineResult result)
    {
        if (result.BestLine.Any())
        {
            var score = result.MateIn != 0 ? $"mate {result.MateIn}" : $"cp {result.Score}";

            Console.Write($"info depth {result.Ply} score {score} nodes {result.NodesCount} time {result.ElapsedTime} pv ");

            foreach (var move in result.BestLine ?? [])
            {
                Console.Write(move + " ");
            }

            Console.WriteLine();
        }
    }

}
