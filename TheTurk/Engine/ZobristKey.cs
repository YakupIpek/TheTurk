using System;
using TheTurk.Moves;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    sealed class Zobrist
    {
        private Board board;

        public long ZobristKey { get; set; }

        private Random random;
        /// <summary>
        /// pieces[side,piece,rank,file]
        /// </summary>
        private long[,,,] pieces;
        private long[] whiteCastle;
        private long[] blackCastle;
        private long[,] enPassant;//[rank,file]
        private long side;

        public Zobrist(Board board)
        {
            this.board = board;
            random = new Random(638750994);
            pieces = new long[2, 6, 9, 9];
            whiteCastle = new long[4];
            blackCastle = new long[4];
            enPassant = new long[9, 9];
            Initialize();
            AssignFirstZobristKey();
        }
        private void Initialize()
        {
            this.side = random.NextLong();

            for (int i = 0; i < 4; i++)
            {
                whiteCastle[i] = random.NextLong();
                blackCastle[i] = random.NextLong();
            }

            for (int rank = 1; rank < 9; rank++)
            {
                for (int file = 1; file < 9; file++)
                {
                    for (int piece = 0; piece < 6; piece++)
                    {
                        for (int side = 0; side < 2; side++)
                        {
                            pieces[side, piece, rank, file] = random.NextLong();
                        }
                    }
                    enPassant[rank, file] = random.NextLong();
                }
            }
            enPassant[0, 0] = random.NextLong();
        }
        private void AssignFirstZobristKey()
        {
            var side = (int)board.Side == 1 ? 0 : 1;
            foreach (var piece in board.GetPieces())
            {
                ZobristKey ^= pieces[side, piece.ToInt, piece.From.Rank, piece.From.File];
            }
            ZobristKey ^= whiteCastle[(int)board.WhiteCastle];
            ZobristKey ^= blackCastle[(int)board.BlackCastle];
            ZobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            ZobristKey ^= this.side;
        }
        public void ZobristUpdate(Move move)
        {
            ZobristKey ^= whiteCastle[(int)board.WhiteCastle];
            ZobristKey ^= blackCastle[(int)board.BlackCastle];

            if (board.EnPassantSquare.Rank != 0 && board.EnPassantSquare.File != 0)
            {
                ZobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            }

            side = move.Piece.Color == Color.White ? 0 : 1;
            ZobristKey ^= pieces[side, move.Piece.ToInt, move.From.Rank, move.From.File];//Remove piece on the square
            ZobristKey ^= pieces[side, move.To.GetPiece(board).ToInt, move.To.Rank, move.To.File];//Carry piece to target
            ZobristKey ^= side;

            Piece capturedPiece;
            if (move is Ordinary && (capturedPiece = (move as Ordinary).CapturedPiece) != null)
            {
                ZobristKey = pieces[side, capturedPiece.ToInt, capturedPiece.From.Rank, capturedPiece.From.File];
            }
            else if (move is ShortCastle)
            {
                var rookFrom = move.Piece.Color == Color.White ? Coordinate.h1 : Coordinate.h8;
                var rookTo = move.Piece.Color == Color.White ? Coordinate.f1 : Coordinate.f8;
                ZobristKey ^= pieces[side, Rook.rook, rookFrom.Rank, rookFrom.File];
                ZobristKey ^= pieces[side, Rook.rook, rookTo.Rank, rookTo.File];
            }
            else if (move is LongCastle)
            {
                var rookFrom = move.Piece.Color == Color.White ? Coordinate.a1 : Coordinate.a8;
                var rookTo = move.Piece.Color == Color.White ? Coordinate.d1 : Coordinate.d8;
                ZobristKey ^= pieces[side, Rook.rook, rookFrom.Rank, rookFrom.File];
                ZobristKey ^= pieces[side, Rook.rook, rookTo.Rank, rookTo.File];
            }
        }
    }
}
