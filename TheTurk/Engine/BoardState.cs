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
    public Coordinate EnPassantSquare { get; private set; }
    public Castle WhiteCastle { get; private set; }
    public Castle BlackCastle { get; private set; }
    public int FiftyMovesRule { get; private set; }
    public ulong ZobristKey { get; private set; }
    public BoardState(Coordinate enPassant, Castle whiteCastle, Castle blackCastle, int fiftyMovesRule, ulong zobristKey)
    {
        EnPassantSquare = enPassant;
        WhiteCastle = whiteCastle;
        BlackCastle = blackCastle;
        FiftyMovesRule = fiftyMovesRule;
        ZobristKey = zobristKey;
    }
}

#endregion

