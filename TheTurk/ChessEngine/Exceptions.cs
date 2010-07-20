using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessEngine.ChessEngine
{

    public class OutOfBoardException : Exception
    {

        public OutOfBoardException():base("Square was out of board")
        {
            
        }

    }
}
