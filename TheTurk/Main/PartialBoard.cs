namespace ChessEngine.Main
{
    public partial class Board
    {
        #region Castle enum

        public enum Castle
        {
            ShortCastle, LongCastle, BothCastle, NoneCastle
        }

        #endregion

        #region Nested type: State

        class State
        {
            public Coordinate EnPassantSquare { get; private set; }
            public Castle WhiteCastle { get; private set; }
            public Castle BlackCastle { get; private set; }
            public int FiftyMovesRule { get; private set; }
            public long ZobristKey { get; private set; }
            public State(Coordinate enPassant, Castle whiteCastle, Castle blackCastle, int fiftyMovesRule,long zobristKey)
            {
                EnPassantSquare = enPassant;
                WhiteCastle = whiteCastle;
                BlackCastle = blackCastle;
                FiftyMovesRule = fiftyMovesRule;
                ZobristKey = zobristKey;
            }
        }

        #endregion
    }
}
