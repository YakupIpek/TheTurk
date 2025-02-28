using System.Runtime.CompilerServices;
using TheTurk.Moves;

namespace TheTurk.Engine;

public class EngineResult
{
    public int Ply { get; init; }
    public int Score { get; init; }
    public int MateIn { get; }
    public long ElapsedTime { get; init; }
    public int NodesCount { get; init; }
    public Move[] BestLine { get; init; }

    public EngineResult(int ply,  int score, long rawElapsedTime, int nodesCount, Move[] bestLine)
    {
        var depth = bestLine.Length;

        Ply = ply;
        Score = score;
        MateIn = Math.Abs(score) + depth == Board.CheckMateValue ? (int)Math.Ceiling(depth / 2.0) * Math.Sign(score) : 0;
        ElapsedTime = rawElapsedTime;
        NodesCount = nodesCount;
        BestLine = bestLine;
    }
}