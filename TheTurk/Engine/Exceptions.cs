using System;

namespace ChessEngine.Engine
{

    public class OutOfBoardException : Exception
    {
        public OutOfBoardException():base("Square was out of board")
        {
            
        }
    }
}
