using System;

namespace ChessEngine.Main
{

    public class OutOfBoardException : Exception
    {
        public OutOfBoardException():base("Square was out of board")
        {
            
        }
    }
}
