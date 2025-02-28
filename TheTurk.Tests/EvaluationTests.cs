using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTurk.Engine;

namespace TheTurk.Tests;

[TestClass]
public class EvaluationTests
{

    [TestMethod]
    public void StaticEvaluation()
    {
        var board = new Board();

        board.SetUpBoard();

        foreach (var piece in board.GenerateMoves())
        {

        }
    }
}
