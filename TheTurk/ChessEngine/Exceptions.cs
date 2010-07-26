using System;

namespace ChessEngine.ChessEngine
{

    public class OutOfBoardException : Exception
    {
        public OutOfBoardException():base("Square was out of board")
        {
            
        }
    }
}
