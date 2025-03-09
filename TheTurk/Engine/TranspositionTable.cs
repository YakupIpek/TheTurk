using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using TheTurk.Moves;

namespace TheTurk.Engine;

// Entry type flags for transposition table
public enum HashEntryType
{
    Exact = 0,  // Exact score
    Alpha = 1,  // Upper bound (fail low)
    Beta = 2    // Lower bound (fail high)
}

// Transposition table entry
public class TEntry
{
    public ulong Hash { get; set; }        // Position zobrist hash
    public int Depth { get; set; }         // Search depth
    public int Score { get; set; }         // Evaluation score
    public HashEntryType Flag { get; set; } // Entry type
    public int Age { get; set; }           // Age (which search iteration it was added)
    public Move BestMove { get; set; }     // Best move from this position

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
    public void Store(ulong hash, int depth, int score, HashEntryType flag, Move bestMove)
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
            entry.Score = score;
            entry.Flag = flag;
            entry.BestMove = bestMove;
            entry.Age = currentAge;

            table[index] = entry;
        }
    }

    // Probe the table for an entry
    public bool TryGetBestMove(ulong hash, int depth, int alpha, int beta, out int score, out Move bestMove)
    {
        var index = GetIndex(hash);
        var entry = table[index];

        bestMove = null;
        score = 0;

        if(entry is null)
            return false;

        // No match found for this hash
        if (entry.Hash != hash)
            return false;

        // Always use the best move, even if depth is insufficient
        bestMove = entry.BestMove;

        // Don't use score if depth is insufficient
        if (entry.Depth < depth)
            return false;

        score = entry.Score;

        // EXACT: Direct hit, use score
        if (entry.Flag == HashEntryType.Exact)
            return true;

        // ALPHA: Upper bound (no beta cutoff occurred)
        // Can use only if entry.score <= alpha for alpha cutoff
        if (entry.Flag == HashEntryType.Alpha && score <= alpha)
        {
            score = alpha;
            return true;
        }

        // BETA: Lower bound (beta cutoff occurred)
        // Can use only if entry.score >= beta for beta cutoff
        if (entry.Flag == HashEntryType.Beta && score >= beta)
        {
            score = beta;
            return true;
        }

        return false;
    }

    // Clear the table
    public void Clear()
    {
        for (var i = 0ul; i < size; i++)
        {
            table[i].Hash = 0;
        }
        currentAge = 0;
    }
}