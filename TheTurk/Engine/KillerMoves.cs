using TheTurk.Bitboards;

namespace TheTurk.Engine;

class KillerMoves
{
    private const int Maxdepth = 40;
    private int[,,] killerMoves;///[depth,From,To] 
    private int[] bestMoveScores;
    public Move[] BestMoves;

    public KillerMoves()
    {
        killerMoves=new int[Maxdepth, 64, 64];

        BestMoves=new Move[Maxdepth];

        bestMoveScores= new int[Maxdepth];
    }
    public void Add(Move move, int depth)
    {
        int score = ++killerMoves[depth, move.FromSquare, move.ToSquare];

        if (score > bestMoveScores[depth])
        {
            bestMoveScores[depth] = score;
            BestMoves[depth] = move;
        }
    }

}
