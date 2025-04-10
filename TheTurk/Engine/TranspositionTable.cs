using System.Numerics;
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
    public Move? BestMove { get; set; }     // Best move from this position

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
    public readonly TEntry[] table;
    private readonly ulong mask;
    private int currentAge;
    public TranspositionTable(int sizeMB)
    {
        // Convert size from MB to entry count
        var entrySize = TEntry.SizeOf();

        var entriesCount = (sizeMB * 1024 * 1024) / entrySize;

        // Round to nearest power of 2
        var size = (ulong)Math.Pow(2, Math.Floor(Math.Log(entriesCount, 2)));

        table = new TEntry[size];

        mask = size - 1; // Fast modulo for power of 2 sizes 2^n - 1
    }

    // Called when a new search begins
    public void IncrementAge()
    {
        currentAge++;
    }


    // Get table index from hash
    private int GetIndex(ulong hash) => (int)(hash & mask);

    // Store/update data in the table
    public void Store(ulong hash, int depth, int ply, int score, HashEntryType flag, Move? bestMove)
    {
        var index = GetIndex(hash);
        var entry = table[index] ?? new();

        if (Board.GetCheckmateInfo(score) is { IsCheckmate: true })
            score += Math.Sign(score) * ply;

        entry.Hash = hash;
        entry.Depth = depth;
        entry.Score = score;
        entry.Flag = flag;
        entry.BestMove = bestMove;
        entry.Age = currentAge;

        table[index] = entry;
    }

    public Move? TryGetBestMove(ulong hash, int depth)
    {
        var index = GetIndex(hash);
        var entry = table[index];

        if (hash == entry?.Hash && entry?.Depth >= depth)
            return entry.BestMove;

        return null;
    }
    public (bool Valid, int Score, Move? BestMove) TryGetBestMove(ulong hash, int depth, int ply, ref int alpha, ref int beta)
    {
        var index = GetIndex(hash);
        var entry = table[index];

        if (entry is null)
            return (false, 0, null);

        // No match found for this hash
        if (entry.Hash != hash)
            return (false, 0, null);

        if (currentAge > entry.Age + 1)
            return (false, 0, null);

        // Don't use score if depth is insufficient
        if (entry.Depth < depth)
            return (false, 0, entry.BestMove);

        var score = entry.Score;

        if (Board.GetCheckmateInfo(score) is { IsCheckmate: true })
            score -= Math.Sign(score) * ply;

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
}