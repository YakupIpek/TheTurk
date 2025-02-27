namespace TheTurk.Engine;


#region Castle enum

public enum Castle
{
    ShortCastle, LongCastle, BothCastle, NoneCastle
}

#endregion

#region Nested type: State

public class BoardState
{
    public Coordinate EnPassantSquare { get; init; }
    public Castle WhiteCastle { get; init; }
    public Castle BlackCastle { get; init; }
    public int FiftyMovesRule { get; init; }
    public ulong ZobristKey { get; init; }

    public BoardState()
    {
        
    }
    public BoardState(Board board)
    {
        EnPassantSquare = board.EnPassantSquare;
        WhiteCastle = board.WhiteCastle;
        BlackCastle = board.BlackCastle;
        FiftyMovesRule = board.FiftyMovesRule;
        ZobristKey = board.Zobrist.ZobristKey;
    }
}

#endregion

