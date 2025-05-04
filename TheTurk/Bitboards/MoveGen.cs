using System.Reflection;
using System.Runtime.CompilerServices;

namespace TheTurk.Bitboards;

public struct MoveGen
{
    private readonly BoardState board;

    public MoveGen(BoardState board)
    {
        this.board=board;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Move Add(Piece flags, int from, int to)
    {
        return new Move(flags, from, to, Piece.None);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> AddAll(Piece piece, int square, ulong targets)
    {
        for (; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return Add(piece, square, Bitboard.LSB(targets));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Move PawnMove(Piece flags, ulong moveTargets, int offset)
    {
        int to = Bitboard.LSB(moveTargets);
        int from = to + offset;
        return Add(flags, from, to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> AddAllCaptures(Piece piece, int square, ulong targets)
    {
        for (; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return AddCapture(piece, square, Bitboard.LSB(targets));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Move AddCapture(Piece flags, int from, int to)
    {
        return new Move(flags, from, to, board.GetPiece(to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Move PawnCapture(Piece flags, ulong moveTargets, int offset)
    {
        int to = Bitboard.LSB(moveTargets);
        int from = to + offset;
        return AddCapture(flags, from, to);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> PawnCapturePromotions(Piece flags, ulong moveTargets, int offset)
    {
        int to = Bitboard.LSB(moveTargets);
        int from = to + offset;
        Piece target = board.GetPiece(to);
        yield return new Move(flags | Piece.QueenPromotion, from, to, target);
        yield return new Move(flags | Piece.RookPromotion, from, to, target);
        yield return new Move(flags | Piece.BishopPromotion, from, to, target);
        yield return new Move(flags | Piece.KnightPromotion, from, to, target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> GenerateMoves()
    {
        if (board.SideToMove == Color.White)
        {
            return CollectWhiteCaptures().Concat(CollectWhiteQuiets());
        }
        else
        {
            return CollectBlackCaptures().Concat(CollectBlackQuiets());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> GenerateQuiscenceMoves()
    {
        if (board.SideToMove == Color.White)
        {
            return CollectWhiteCaptures().Concat(CollectWhitePawnQuiets(promotionsOnly:true));
        }
        else
        {
            return CollectBlackCaptures().Concat(CollectBlackPawnQuiets(promotionsOnly: true));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> CollectCaptures()
    {
        if (board.SideToMove == Color.White)
            return CollectWhiteCaptures();
        else
            return CollectBlackCaptures();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Move> CollectQuiets()
    {
        if (board.SideToMove == Color.White)
            return CollectWhiteQuiets();
        else
            return CollectBlackQuiets();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> CollectBlackCaptures()
    {
        ulong occupied = board.Black | board.White;

        // Kings  
        int square = Bitboard.LSB(board.Kings & board.Black);
        foreach (var move in AddAllCaptures(Piece.BlackKing, square, Bitboard.KingTargets[square] & board.White))
            yield return move;

        // Knights  
        for (ulong knights = board.Knights & board.Black; knights != 0; knights = Bitboard.ClearLSB(knights))
        {
            square = Bitboard.LSB(knights);
            foreach (var move in AddAllCaptures(Piece.BlackKnight, square, Bitboard.KnightTargets[square] & board.White))
                yield return move;
        }

        // Bishops  
        for (ulong bishops = board.Bishops & board.Black; bishops != 0; bishops = Bitboard.ClearLSB(bishops))
        {
            square = Bitboard.LSB(bishops);
            foreach (var move in AddAllCaptures(Piece.BlackBishop, square, Bitboard.GetBishopTargets(occupied, square) & board.White))
                yield return move;
        }

        // Rooks  
        for (ulong rooks = board.Rooks & board.Black; rooks != 0; rooks = Bitboard.ClearLSB(rooks))
        {
            square = Bitboard.LSB(rooks);
            foreach (var move in AddAllCaptures(Piece.BlackRook, square, Bitboard.GetRookTargets(occupied, square) & board.White))
                yield return move;
        }

        // Queens  
        for (ulong queens = board.Queens & board.Black; queens != 0; queens = Bitboard.ClearLSB(queens))
        {
            square = Bitboard.LSB(queens);
            foreach (var move in AddAllCaptures(Piece.BlackQueen, square, Bitboard.GetQueenTargets(occupied, square) & board.White))
                yield return move;
        }

        // Pawns & Castling  
        ulong targets;
        ulong blackPawns = board.Pawns & board.Black;

        // Capture left  
        ulong captureLeft = ((blackPawns & 0xFEFEFEFEFEFEFEFEUL) >> 9) & board.White;
        for (targets = captureLeft & 0xFFFFFFFFFFFFFF00UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnCapture(Piece.BlackPawn, targets, +9);

        // Capture left to first rank and promote  
        for (targets = captureLeft & 0x00000000000000FFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            foreach (var move in PawnCapturePromotions(Piece.BlackPawn, targets, +9))
                yield return move;
        }

        // Capture right  
        ulong captureRight = ((blackPawns & 0x7F7F7F7F7F7F7F7FUL) >> 7) & board.White;
        for (targets = captureRight & 0xFFFFFFFFFFFFFF00UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnCapture(Piece.BlackPawn, targets, +7);

        // Capture right to first rank and promote  
        for (targets = captureRight & 0x00000000000000FFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            foreach (var move in PawnCapturePromotions(Piece.BlackPawn, targets, +7))
                yield return move;
        }

        // En-passant  
        captureLeft = ((blackPawns & 0x00000000FE000000UL) >> 9) & board.EnPassant;
        if (captureLeft != 0)
            yield return PawnMove(Piece.BlackPawn | Piece.EnPassant, captureLeft, +9);

        captureRight = ((blackPawns & 0x000000007F000000UL) >> 7) & board.EnPassant;
        if (captureRight != 0)
            yield return PawnMove(Piece.BlackPawn | Piece.EnPassant, captureRight, +7);

        // Move up and promote to Queen  
        for (targets = (blackPawns >> 8) & ~occupied & 0x00000000000000FFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.Black | Piece.QueenPromotion, targets, +8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> CollectBlackQuiets()
    {
        ulong occupied = board.Black | board.White;

        // Kings
        int square = Bitboard.LSB(board.Kings & board.Black);
        foreach (var move in AddAll(Piece.BlackKing, square, Bitboard.KingTargets[square] & ~occupied))
            yield return move;

        // Knights
        for (ulong knights = board.Knights & board.Black; knights != 0; knights = Bitboard.ClearLSB(knights))
        {
            square = Bitboard.LSB(knights);
            foreach (var move in AddAll(Piece.BlackKnight, square, Bitboard.KnightTargets[square] & ~occupied))
                yield return move;
        }

        // Bishops
        for (ulong bishops = board.Bishops & board.Black; bishops != 0; bishops = Bitboard.ClearLSB(bishops))
        {
            square = Bitboard.LSB(bishops);
            foreach (var move in AddAll(Piece.BlackBishop, square, Bitboard.GetBishopTargets(occupied, square) & ~occupied))
                yield return move;
        }

        // Rooks
        for (ulong rooks = board.Rooks & board.Black; rooks != 0; rooks = Bitboard.ClearLSB(rooks))
        {
            square = Bitboard.LSB(rooks);
            foreach (var move in AddAll(Piece.BlackRook, square, Bitboard.GetRookTargets(occupied, square) & ~occupied))
                yield return move;
        }

        // Queens
        for (ulong queens = board.Queens & board.Black; queens != 0; queens = Bitboard.ClearLSB(queens))
        {
            square = Bitboard.LSB(queens);
            foreach (var move in AddAll(Piece.BlackQueen, square, Bitboard.GetQueenTargets(occupied, square) & ~occupied))
                yield return move;
        }

        foreach (var move in CollectBlackPawnQuiets(false))
        {
            yield return move;
        }

        // Castling
        if (board.CanBlackCastleLong() && !board.IsAttackedByWhite(60) && !board.IsAttackedByWhite(59) /*&& !board.IsAttackedByWhite(58)*/)
            yield return Move.BlackCastlingLong;

        if (board.CanBlackCastleShort() && !board.IsAttackedByWhite(60) && !board.IsAttackedByWhite(61) /*&& !board.IsAttackedByWhite(62)*/)
            yield return Move.BlackCastlingShort;
    }

    private IEnumerable<Move> CollectBlackPawnQuiets(bool promotionsOnly)
    {
        var occupied = board.Black | board.White;


        // Pawns
        ulong targets;
        ulong blackPawns = board.Pawns & board.Black;
        ulong oneStep = (blackPawns >> 8) & ~occupied;

        for (targets = oneStep & 0x00000000000000FFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            yield return PawnMove(Piece.Black | Piece.RookPromotion, targets, +8);
            yield return PawnMove(Piece.Black | Piece.BishopPromotion, targets, +8);
            yield return PawnMove(Piece.Black | Piece.KnightPromotion, targets, +8);
        }

        if(promotionsOnly)
            yield break;

        // Move one square down
        for (targets = oneStep & 0xFFFFFFFFFFFFFF00UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.BlackPawn, targets, +8);

        // Move two squares down
        ulong twoStep = (oneStep >> 8) & ~occupied;
        for (targets = twoStep & 0x000000FF00000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.BlackPawn, targets, +16);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> CollectWhiteCaptures()
    {
        ulong occupied = board.Black | board.White;

        // Kings  
        int square = Bitboard.LSB(board.Kings & board.White);
        foreach (var move in AddAllCaptures(Piece.WhiteKing, square, Bitboard.KingTargets[square] & board.Black))
            yield return move;

        // Knights  
        for (ulong knights = board.Knights & board.White; knights != 0; knights = Bitboard.ClearLSB(knights))
        {
            square = Bitboard.LSB(knights);
            foreach (var move in AddAllCaptures(Piece.WhiteKnight, square, Bitboard.KnightTargets[square] & board.Black))
                yield return move;
        }

        // Bishops  
        for (ulong bishops = board.Bishops & board.White; bishops != 0; bishops = Bitboard.ClearLSB(bishops))
        {
            square = Bitboard.LSB(bishops);
            foreach (var move in AddAllCaptures(Piece.WhiteBishop, square, Bitboard.GetBishopTargets(occupied, square) & board.Black))
                yield return move;
        }

        // Rooks  
        for (ulong rooks = board.Rooks & board.White; rooks != 0; rooks = Bitboard.ClearLSB(rooks))
        {
            square = Bitboard.LSB(rooks);
            foreach (var move in AddAllCaptures(Piece.WhiteRook, square, Bitboard.GetRookTargets(occupied, square) & board.Black))
                yield return move;
        }

        // Queens  
        for (ulong queens = board.Queens & board.White; queens != 0; queens = Bitboard.ClearLSB(queens))
        {
            square = Bitboard.LSB(queens);
            foreach (var move in AddAllCaptures(Piece.WhiteQueen, square, Bitboard.GetQueenTargets(occupied, square) & board.Black))
                yield return move;
        }

        // Pawns  
        ulong targets;
        ulong whitePawns = board.Pawns & board.White;
        ulong oneStep = (whitePawns << 8) & ~occupied;
        
        // Capture left  
        ulong captureLeft = ((whitePawns & 0xFEFEFEFEFEFEFEFEUL) << 7) & board.Black;
        for (targets = captureLeft & 0x00FFFFFFFFFFFFFFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnCapture(Piece.WhitePawn, targets, -7);

        // Capture left to last rank and promote  
        for (targets = captureLeft & 0xFF00000000000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            foreach (var move in PawnCapturePromotions(Piece.WhitePawn, targets, -7))
                yield return move;
        }

        // Capture right  
        ulong captureRight = ((whitePawns & 0x7F7F7F7F7F7F7F7FUL) << 9) & board.Black;
        for (targets = captureRight & 0x00FFFFFFFFFFFFFFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnCapture(Piece.WhitePawn, targets, -9);

        // Capture right to last rank and promote  
        for (targets = captureRight & 0xFF00000000000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            foreach (var move in PawnCapturePromotions(Piece.WhitePawn, targets, -9))
                yield return move;
        }

        // En-passant  
        captureLeft = ((whitePawns & 0x000000FE00000000UL) << 7) & board.EnPassant;
        if (captureLeft != 0)
            yield return PawnMove(Piece.WhitePawn | Piece.EnPassant, captureLeft, -7);

        captureRight = ((whitePawns & 0x000007F00000000UL) << 9) & board.EnPassant;
        if (captureRight != 0)
            yield return PawnMove(Piece.WhitePawn | Piece.EnPassant, captureRight, -9);

        // Move up and promote Queen  
        for (targets = (whitePawns << 8) & ~occupied & 0xFF00000000000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.White | Piece.QueenPromotion, targets, -8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<Move> CollectWhiteQuiets()
    {
        ulong occupied = board.Black | board.White;

        // Kings  
        int square = Bitboard.LSB(board.Kings & board.White);
        foreach (var move in AddAll(Piece.WhiteKing, square, Bitboard.KingTargets[square] & ~occupied))
            yield return move;

        // Knights  
        for (ulong knights = board.Knights & board.White; knights != 0; knights = Bitboard.ClearLSB(knights))
        {
            square = Bitboard.LSB(knights);
            foreach (var move in AddAll(Piece.WhiteKnight, square, Bitboard.KnightTargets[square] & ~occupied))
                yield return move;
        }

        // Bishops  
        for (ulong bishops = board.Bishops & board.White; bishops != 0; bishops = Bitboard.ClearLSB(bishops))
        {
            square = Bitboard.LSB(bishops);
            foreach (var move in AddAll(Piece.WhiteBishop, square, Bitboard.GetBishopTargets(occupied, square) & ~occupied))
                yield return move;
        }

        // Rooks  
        for (ulong rooks = board.Rooks & board.White; rooks != 0; rooks = Bitboard.ClearLSB(rooks))
        {
            square = Bitboard.LSB(rooks);
            foreach (var move in AddAll(Piece.WhiteRook, square, Bitboard.GetRookTargets(occupied, square) & ~occupied))
                yield return move;
        }

        // Queens  
        for (ulong queens = board.Queens & board.White; queens != 0; queens = Bitboard.ClearLSB(queens))
        {
            square = Bitboard.LSB(queens);
            foreach (var move in AddAll(Piece.WhiteQueen, square, Bitboard.GetQueenTargets(occupied, square) & ~occupied))
                yield return move;
        }

        foreach (var move in CollectWhitePawnQuiets(false))
        {
            yield return move;
        }

        // Castling  
        if (board.CanWhiteCastleLong() && !board.IsAttackedByBlack(4) && !board.IsAttackedByBlack(3))
            yield return Move.WhiteCastlingLong;

        if (board.CanWhiteCastleShort() && !board.IsAttackedByBlack(4) && !board.IsAttackedByBlack(5))
            yield return Move.WhiteCastlingShort;
    }

    private IEnumerable<Move> CollectWhitePawnQuiets(bool promotionsOnly)
    {
        var occupied = board.Black | board.White;

        // Pawns  
        ulong targets;
        ulong whitePawns = board.Pawns & board.White;
        ulong oneStep = (whitePawns << 8) & ~occupied;

        for (targets = oneStep & 0xFF00000000000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
        {
            yield return PawnMove(Piece.White | Piece.RookPromotion, targets, -8);
            yield return PawnMove(Piece.White | Piece.BishopPromotion, targets, -8);
            yield return PawnMove(Piece.White | Piece.KnightPromotion, targets, -8);
        }

        if (promotionsOnly)
            yield break;

        // Move one square up  
        for (targets = oneStep & 0x00FFFFFFFFFFFFFFUL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.WhitePawn, targets, -8);

        // Move two squares up  
        ulong twoStep = (oneStep << 8) & ~occupied;
        for (targets = twoStep & 0x00000000FF000000UL; targets != 0; targets = Bitboard.ClearLSB(targets))
            yield return PawnMove(Piece.WhitePawn, targets, -16);
    }
}

