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
    public List<Move> BestLine { get; init; }

    public EngineResult(int iterationPly, int score, long rawElapsedTime, int nodesCount, List<Move> bestLine)
    {
        var (isMate, mateIn, _) = Board.GetCheckmateInfo(score);

        Ply = iterationPly;
        Score = score;
        MateIn = isMate ? (int)Math.Ceiling(mateIn / 2.0) : 0;
        ElapsedTime = rawElapsedTime;
        NodesCount = nodesCount;
        BestLine = bestLine;
    }
}