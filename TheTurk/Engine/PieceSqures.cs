using System.Buffers.Binary;
using System.Numerics;
using TheTurk.Bitboards;

namespace TheTurk.Engine;

public static class PieceSqures
{
    // Each array has 64 values, one per square (A1 to H8)

    public static readonly int[] PawnMidgame =
    [
         0,   0,   0,   0,   0,   0,   0,   0,
        50,  50,  50,  50,  50,  50,  50,  50,
        10,  10,  20,  30,  30,  20,  10,  10,
         5,   5,  10,  25,  25,  10,   5,   5,
         0,   0,   0,  20,  20,   0,   0,   0,
         5,  -5, -10,   0,   0, -10,  -5,   5,
         5,  10,  10, -20, -20,  10,  10,   5,
         0,   0,   0,   0,   0,   0,   0,   0
    ];

    public static readonly int[] KnightMidgame =
    [
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -30,   5,  10,  15,  15,  10,   5, -30,
        -30,   0,  15,  20,  20,  15,   0, -30,
        -30,   5,  15,  20,  20,  15,   5, -30,
        -30,   0,  10,  15,  15,  10,   0, -30,
        -40, -20,   0,   0,   0,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50
    ];

    public static readonly int[] BishopMidgame =
    [
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10,   5,   0,   0,   0,   0,   5, -10,
        -10,  10,  10,  10,  10,  10,  10, -10,
        -10,   0,  10,  10,  10,  10,   0, -10,
        -10,   5,   5,  10,  10,   5,   5, -10,
        -10,   0,   5,  10,  10,   5,   0, -10,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -20, -10, -10, -10, -10, -10, -10, -20
    ];

    public static readonly int[] RookMidgame =
    [
         0,   0,   0,   0,   0,   0,   0,   0,
         5,  10,  10,  10,  10,  10,  10,   5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
         0,   0,   0,   5,   5,   0,   0,   0
    ];

    public static readonly int[] QueenMidgame =
    [
        -20, -10, -10,  -5,  -5, -10, -10, -20,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -10,   5,   5,   5,   5,   5,   0, -10,
         -5,   0,   5,   5,   5,   5,   0,  -5,
          0,   0,   5,   5,   5,   5,   0,  -5,
        -10,   5,   5,   5,   5,   5,   0, -10,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -20, -10, -10,  -5,  -5, -10, -10, -20
    ];

    public static readonly int[] KingMidgame =
    [
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
         20,  20,   0,   0,   0,   0,  20,  20,
         20,  30,  10,   0,   0,  10,  30,  20
    ];

    public static int Evaluate(BoardState board)
    {

        var score = 0;

        score += Evaluate(board.White & board.Pawns, true, PawnMidgame);
        score -= Evaluate(board.Black & board.Pawns, false, PawnMidgame);

        score += Evaluate(board.White & board.Knights, true, KnightMidgame);
        score -= Evaluate(board.Black & board.Knights, false, KnightMidgame);

        score += Evaluate(board.White & board.Bishops, true, BishopMidgame);
        score -= Evaluate(board.Black & board.Bishops, false, BishopMidgame);

        score += Evaluate(board.White & board.Rooks, true, RookMidgame);
        score -= Evaluate(board.Black & board.Rooks, false, RookMidgame);

        score += Evaluate(board.White & board.Queens, true, QueenMidgame);
        score -= Evaluate(board.Black & board.Queens, false, QueenMidgame);

        score += Evaluate(board.White & board.Kings, true, KingMidgame);
        score -= Evaluate(board.Black & board.Kings, false, KingMidgame);

        return score;
    }


    public static int Evaluate(ulong bitboard, bool isWhite, int[] table)
    {
        int score = 0;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard);

            
            int index = isWhite ? square : square ^ 56;

            score += table[index];

            // LSB sıfırlanır
            bitboard &= bitboard - 1;
        }

        return score;
    }
}
