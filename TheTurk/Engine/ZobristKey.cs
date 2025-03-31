using System;
using System.Threading.Tasks.Sources;
using TheTurk.Moves;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    public class Zobrist
    {
        private Board board;

        public ulong ZobristKey { get; set; }

        private Random random;
        /// <summary>
        /// pieces[side,piece,rank,file]
        /// </summary>
        private ulong[,,,] pieces;
        private ulong[] whiteCastle;
        private ulong[] blackCastle;
        private ulong[,] enPassant;//[rank,file]
        private ulong side;

        public Zobrist(Board board)
        {
            this.board = board;
            random = new Random(638750994);
            pieces = new ulong[2, 6, 9, 9];
            whiteCastle = new ulong[4];
            blackCastle = new ulong[4];
            enPassant = new ulong[9, 9];
            Initialize();
            AssignFirstZobristKey();
        }

        private ulong GetNext()
        {
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
        private void Initialize()
        {
            this.side = GetNext();

            for (int i = 0; i < 4; i++)
            {
                whiteCastle[i] = GetNext();
                blackCastle[i] = GetNext();
            }

            for (int rank = 1; rank < 9; rank++)
            {
                for (int file = 1; file < 9; file++)
                {
                    for (int piece = 0; piece < 6; piece++)
                    {
                        for (int side = 0; side < 2; side++)
                        {
                            pieces[side, piece, rank, file] = GetNext();
                        }
                    }
                    enPassant[rank, file] = GetNext();
                }
            }
            enPassant[0, 0] = GetNext();
        }
        private void AssignFirstZobristKey()
        {
            var side = (int)board.Side == 1 ? 0 : 1;
            foreach (var piece in board.GetPieces())
            {
                ZobristKey ^= pieces[side, piece.Number, piece.From.Rank, piece.From.File];
            }
            ZobristKey ^= whiteCastle[(int)board.WhiteCastle];
            ZobristKey ^= blackCastle[(int)board.BlackCastle];

            ZobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            ZobristKey ^= this.side;
        }

        public void ZobristUpdateForNullMove()
        {
            ZobristKey ^= this.side;
        }

        public void ZobristUpdate(Move move, BoardState state)
        {
            var (movingSide, oppositeSide) = move.Piece.Color == Color.White ? (0, 1) : (1, 0);

            if (move is Promote promotionMove)
            {
                Update(movingSide, move.Piece.Number, move.From);

                // Add the promoted piece to the destination square.
                Update(movingSide, promotionMove.PromotedPiece.Number, move.To);
            }
            else
            {
                Update(movingSide, move.Piece.Number, move.From, move.To);
            }

            // Remove captured piece from the destination square.
            if (move is Ordinary ordinaryMove && ordinaryMove.CapturedPiece is not null)
            {
                Update(oppositeSide, ordinaryMove.CapturedPiece.Number, ordinaryMove.CapturedPiece.From);
            }
            else if (move is ShortCastle)
            {
                var (h1, f1, h8, f8) = (Coordinate.h1, Coordinate.f1, Coordinate.h8, Coordinate.f8);

                // Update rook's position for short castle.
                var (from, to) = move.Piece.Color == Color.White ? (h1, f1) : (h8, f8);

                // Remove rook from its original square.
                Update(movingSide, Rook.Id, from, to);
            }
            else if (move is LongCastle)
            {
                var (a1, d1, a8, d8) = (Coordinate.a1, Coordinate.d1, Coordinate.a8, Coordinate.d8);

                // Update rook's position for long castle.
                var (from, to) = move.Piece.Color == Color.White ? (a1, d1) : (a8, d8);

                // Remove rook from its original square.
                Update(movingSide, Rook.Id, from, to);
            }

            
            ZobristKey ^= this.side;

            // Add updated castling rights and en passant square to the hash.
            // (Assumes that the board state has already been updated with the move.)

            if (board.WhiteCastle != state.WhiteCastle)
                ZobristKey ^= whiteCastle[(int)board.WhiteCastle];

            if (board.BlackCastle != state.BlackCastle)
                ZobristKey ^= blackCastle[(int)board.BlackCastle];

            if (board.EnPassantSquare.Rank != 0)
            {
                ZobristKey ^= enPassant[board.EnPassantSquare.Rank, board.EnPassantSquare.File];
            }
        }

        private void Update(int side, int piece, Coordinate square)
        {
            ZobristKey ^= pieces[side, piece, square.Rank, square.File];
        }

        private void Update(int side, int piece, Coordinate from, Coordinate to)
        {
            Update(side, piece, from);
            Update(side, piece, to);
        }
    }
}
