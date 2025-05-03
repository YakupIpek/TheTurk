using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using TheTurk.Bitboards;

namespace TheTurk.Engine;

public static class Evaluation
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static int GetScore(BoardState board)
    {
        var score = 0;

        score += GetScore(board.White & board.Pawns, true, PawnMidgame);
        score -= GetScore(board.Black & board.Pawns, false, PawnMidgame);

        score += GetScore(board.White & board.Knights, true, KnightMidgame);
        score -= GetScore(board.Black & board.Knights, false, KnightMidgame);

        score += GetScore(board.White & board.Bishops, true, BishopMidgame);
        score -= GetScore(board.Black & board.Bishops, false, BishopMidgame);

        score += GetScore(board.White & board.Rooks, true, RookMidgame);
        score -= GetScore(board.Black & board.Rooks, false, RookMidgame);

        score += GetScore(board.White & board.Queens, true, QueenMidgame);
        score -= GetScore(board.Black & board.Queens, false, QueenMidgame);

        score += GetScore(board.White & board.Kings, true, KingMidgame);
        score -= GetScore(board.Black & board.Kings, false, KingMidgame);

        return score;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static int GetScore(ulong bitboard, bool isWhite, int[] table)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInsufficientMatingMaterial(BoardState board)
    {
        //https://support.chess.com/article/128-what-does-insufficient-mating-material-mean
        if (Bitboard.PopCount(board.Black | board.White) > 4)
            return false;

        ulong black = board.Black & ~board.Kings;
        ulong white = board.White & ~board.Kings;

        //lone king vs two knights
        if (white == 0 && (board.Knights & black) == black && Bitboard.PopCount(black) == 2)
            return true;

        if (black == 0 && (board.Knights & white) == white && Bitboard.PopCount(white) == 2)
            return true;

        //if both sides have any one of the following, and there are no pawns on the board: 
        //    * lone king
        //    * king and bishop
        //    * king and knight
        ulong norb = board.Knights | board.Bishops;
        return (black == 0 || ((norb & black) == black && Bitboard.PopCount(black) == 1)) && //Black is K or K(N|B)
               (white == 0 || ((norb & white) == white && Bitboard.PopCount(white) == 1));   //White is K or K(N|B)
    }
}
