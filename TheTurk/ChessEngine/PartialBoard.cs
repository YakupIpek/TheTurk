namespace ChessEngine
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
            public State(Coordinate enPassant, Castle whiteCastle, Castle blackCastle, int fiftyMovesRule)
            {
                this.enPassantSquare = enPassant;
                this.whiteCastle = whiteCastle;
                this.blackCastle = blackCastle;
                this.fiftyMovesRule = fiftyMovesRule;
            }

            public Coordinate enPassantSquare { get; private set; }
            public Castle whiteCastle { get; private set; }
            public Castle blackCastle { get; private set; }
            public int fiftyMovesRule { get; private set; }
        }

        #endregion
    }
}
