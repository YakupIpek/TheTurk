using System;

namespace TheTurk.Engine
{

    public class OutOfBoardException : Exception
    {
        public OutOfBoardException():base("Square was out of board")
        {
            
        }
    }
}
