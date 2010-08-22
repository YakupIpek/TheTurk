using System;
using ChessEngine.Moves;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    sealed class Zobrist
    {
        private Board board;
        private long zobristKey;
        public long ZobristKey { get { return zobristKey; } set { zobristKey = value; } }
        private static Random random;
        /// <summary>
        /// pieces[side,piece,rank,file]
        /// </summary>
        private long[, , ,] pieces;
        private long[] whiteCastle;
        private long[] blackCastle;
        private long[,] enPassant;//[rank,file]
        private long side;
        static Zobrist()
        {
            random = new Random(DateTime.Now.Millisecond);
        }
        public Zobrist(Board board)
        {
            this.board = board;
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
            foreach (Piece piece in board)
            {
                zobristKey ^= pieces[side, piece.ToInt, piece.From.Rank, piece.From.File];
            }
            zobristKey ^= whiteCastle[(int)board.WhiteCastle];
            zobristKey ^= blackCastle[(int)board.BlackCastle];
            zobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            zobristKey ^= this.side;
        }
        public void ZobristUpdate(Move move)
        {
            zobristKey ^= whiteCastle[(int)board.WhiteCastle];
            zobristKey ^= blackCastle[(int)board.BlackCastle];

            if (board.EnPassantSquare.Rank != 0 && board.EnPassantSquare.File != 0)
            {
                zobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            }

            side = move.Piece.Color == Color.White ? 0 : 1;
            zobristKey ^= pieces[side, move.Piece.ToInt, move.From.Rank, move.From.File];//Remove piece on the square
            zobristKey ^= pieces[side, move.To.GetPiece(board).ToInt, move.To.Rank, move.To.File];//Carry piece to target
            zobristKey ^= side;

            Piece capturedPiece;
            if (move is Ordinary && (capturedPiece = (move as Ordinary).CapturedPiece) != null)
            {
                zobristKey = pieces[side,capturedPiece.ToInt,  capturedPiece.From.Rank, capturedPiece.From.File];
            }
            else if (move is ShortCastle)
            {
                var rookFrom = move.Piece.Color == Color.White ? Coordinate.h1 : Coordinate.h8;
                var rookTo = move.Piece.Color == Color.White ? Coordinate.f1 : Coordinate.f8;
                zobristKey ^= pieces[side, Rook.rook, rookFrom.Rank, rookFrom.File];
                zobristKey ^= pieces[side, Rook.rook, rookTo.Rank, rookTo.File];
            }
            else if (move is LongCastle)
            {
                var rookFrom = move.Piece.Color == Color.White ? Coordinate.a1 : Coordinate.a8;
                var rookTo = move.Piece.Color == Color.White ? Coordinate.d1 : Coordinate.d8;
                zobristKey ^= pieces[side, Rook.rook, rookFrom.Rank, rookFrom.File];
                zobristKey ^= pieces[side, Rook.rook, rookTo.Rank, rookTo.File];
            }
        }
    }
}
