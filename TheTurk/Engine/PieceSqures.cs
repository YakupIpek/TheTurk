using System.Buffers.Binary;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using TheTurk.Bitboards;

namespace TheTurk.Engine;

public static class Evaluation
{
    // Each array has 64 values, one per square (A1 to H8)

    public static readonly int[] PawnMidgame =
    [
         0,  0,  0,  0,  0,  0,  0,  0,
        -6, -4,  1,  1,  1,  1, -4, -6,
        -6, -4,  1,  2,  2,  1, -4, -6,
        -6, -4,  2,  8,  8,  2, -4, -6,
        -6, -4,  5, 10, 10,  5, -4, -6,
        -4, -4,  1,  5,  5,  1, -4, -4,
        -6, -4,  1,-24,-24,  1, -4, -6,
         0,  0,  0,  0,  0,  0,  0,  0
    ];

    public static readonly int[] KnightMidgame =
    [
        -8, -8, -8, -8, -8, -8, -8, -8,
        -8,  0,  0,  0,  0,  0,  0, -8,
        -8,  0,  4,  4,  4,  4,  0, -8,
        -8,  0,  4,  8,  8,  4,  0, -8,
        -8,  0,  4,  8,  8,  4,  0, -8,
        -8,  0,  4,  4,  4,  4,  0, -8,
        -8,  0,  1,  2,  2,  1,  0, -8,
        -8,-12, -8, -8, -8, -8,-12, -8
    ];

    public static readonly int[] BishopMidgame =
    [
        -4, -4, -4, -4, -4, -4, -4, -4,
        -4,  0,  0,  0,  0,  0,  0, -4,
        -4,  0,  2,  4,  4,  2,  0, -4,
        -4,  0,  4,  6,  6,  4,  0, -4,
        -4,  0,  4,  6,  6,  4,  0, -4,
        -4,  1,  2,  4,  4,  2,  1, -4,
        -4,  2,  1,  1,  1,  1,  2, -4,
        -4, -4,-12, -4, -4,-12, -4, -4
    ];

    public static readonly int[] RookMidgame =
    [
         5,  5,  5,  5,  5,  5,  5,  5,
        20, 20, 20, 20, 20, 20, 20, 20,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
         0,  0,  0,  2,  2,  0,  0,  0
    ];

    public static readonly int[] QueenMidgame =
    [
         0,  0,  0,  0,  0,  0,  0,  0,
         0,  0,  1,  1,  1,  1,  0,  0,
         0,  0,  1,  2,  2,  1,  0,  0,
         0,  0,  2,  3,  3,  2,  0,  0,
         0,  0,  2,  3,  3,  2,  0,  0,
         0,  0,  1,  2,  2,  1,  0,  0,
         0,  0,  1,  1,  1,  1,  0,  0,
        -5, -5, -5, -5, -5, -5, -5, -5
    ];

    public static readonly int[] KingMidgame =
    [
       -40, -40, -40, -40, -40, -40, -40, -40,
       -40, -40, -40, -40, -40, -40, -40, -40,
       -40, -40, -40, -40, -40, -40, -40, -40,
       -40, -40, -40, -40, -40, -40, -40, -40,
       -40, -40, -40, -40, -40, -40, -40, -40,
       -40, -40, -40, -40, -40, -40, -40, -40,
       -15, -15, -20, -20, -20, -20, -15, -15,
         0,  20,  30, -30,  0, -20,  30,  20
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static int GetPieceSquareScore(BoardState board)
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

    public static int Evaluate(BoardState board)
    {
        var score = 0;

        score += Move.GetPieceValue(Piece.Pawn) * (BitOperations.PopCount(board.Pawns & board.White) - BitOperations.PopCount(board.Pawns & board.Black));
        score += Move.GetPieceValue(Piece.Knight) * (BitOperations.PopCount(board.Knights & board.  White) - BitOperations.PopCount(board.Knights & board.Black));
        score += Move.GetPieceValue(Piece.Bishop) * (BitOperations.PopCount(board.Bishops & board.White) - BitOperations.PopCount(board.Bishops & board.Black));
        score += Move.GetPieceValue(Piece.Rook) * (BitOperations.PopCount(board.Rooks & board.White) - BitOperations.PopCount(board.Rooks & board.Black));
        score += Move.GetPieceValue(Piece.Queen) * (BitOperations.PopCount(board.Queens & board.White) - BitOperations.PopCount(board.Queens & board.Black));

        score += GetPieceSquareScore(board);

        return score;
    }


    public static int GetScore(ulong bitboard, bool isWhite, int[] table)
    {
        int score = 0;

        while (bitboard != 0)
        {
            int square = BitOperations.TrailingZeroCount(bitboard);

            int index = isWhite ? square ^ 56 : square;

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
