using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessEngine.Engine
{
    public interface IProtocol
    {
        void WriteOutput(Engine.Result result);
    }
}
