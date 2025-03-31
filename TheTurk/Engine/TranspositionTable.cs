using TheTurk.Moves;

namespace TheTurk.Engine;

// Entry type flags for transposition table
public enum HashEntryType
{
    Exact = 0,  // Exact score
    LowerBound = 1,  // Upper bound (fail low)
    UpperBound = 2    // Lower bound (fail high)
}

// Transposition table entry
public class TEntry
{
    public ulong Hash { get; set; }        // Position zobrist hash
    public int Depth { get; set; }         // Search depth
    public int Score { get; set; }         // Evaluation score
    public HashEntryType Flag { get; set; } // Entry type
    public int Age { get; set; }           // Age (which search iteration it was added)
    public Node<Move> BestMove { get; set; }     // Best move from this position

    public static int SizeOf()
    {
        var hashSize = sizeof(ulong);
        var depthSize = sizeof(int);
        var scoreSize = sizeof(int);
        var flagSize = sizeof(HashEntryType);
        var ageSize = sizeof(int);
        var moveSize = 3 * sizeof(int); // Move: From + To + Index
        return hashSize + depthSize + scoreSize + flagSize + ageSize + moveSize;
    }
}

public class TranspositionTable
{
    private readonly TEntry[] table;
    private readonly ulong size;
    private int currentAge;
    public int savedCount = 0;
    public TranspositionTable(int sizeMB)
    {
        // Convert size from MB to entry count
        var entrySize = TEntry.SizeOf();

        var entriesCount = (sizeMB * 1024 * 1024) / entrySize;

        // Round to nearest power of 2
        size = (ulong)Math.Pow(2, Math.Floor(Math.Log(entriesCount, 2)));

        table = new TEntry[size];

        currentAge = 0;
    }

    // Called when a new search begins
    public void IncrementAge()
    {
        currentAge++;
    }

    // Get table index from hash
    private int GetIndex(ulong hash)
    {
        return (int)(hash & (size - 1)); // Fast modulo for power of 2 sizes
    }

    // Store/update data in the table
    public void Store(ulong hash, int depth, int score, HashEntryType flag, Node<Move> bestMove)
    {
        var index = GetIndex(hash);
        var entry = table[index] ?? new();

        // Replacement strategy:
        // 1. If entry is empty, or
        // 2. If same hash but current search is deeper, or
        // 3. If entry is old (from previous searches)
        // then replace the entry
        if (entry.Hash == 0 ||
            entry.Hash == hash && depth >= entry.Depth /*|| currentAge - entry.Age > 2*/)
        {
            entry.Hash = hash;
            entry.Depth = depth;
            entry.Score = score; //AdjustScoreForDepth(score, depth);
            entry.Flag = flag;
            entry.BestMove = bestMove;
            entry.Age = currentAge;

            table[index] = entry;
        }
    }

    // Probe the table for an entry
    public (bool Valid, int Score, Node<Move>? BestMove) TryGetBestMove(ulong hash, int depth, ref int alpha, ref int beta)
    {
        var index = GetIndex(hash);
        var entry = table[index];

        if (entry is null)
            return (false, 0, null);

        // No match found for this hash
        if (entry.Hash != hash)
            return (false, 0, null);


        // Don't use score if depth is insufficient
        if (entry.Depth < depth)
            return (false, 0, entry.BestMove);

        var score = entry.Score; // AdjustScoreForDepth(entry.Score, depth);

        if (entry.Flag == HashEntryType.Exact)
        {
            return (true, score, entry.BestMove);
        }

        if (entry.Flag == HashEntryType.LowerBound)
        {
            alpha = Math.Max(alpha, score);
        }
        else if (entry.Flag == HashEntryType.UpperBound)
        {
            beta = Math.Min(beta, score);
        }

        return (alpha >= beta, score, entry.BestMove);
    }

    private int AdjustScoreForDepth(int score, int depth)
    {
        if (Math.Abs(score) + 1_000 >= Board.CheckMateValue)
        {
            return score > 0
                ? score - depth
                : score + depth;
        }

        return score;
    }
}