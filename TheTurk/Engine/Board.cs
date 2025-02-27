using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheTurk.Moves;
using TheTurk.Pieces;

namespace TheTurk.Engine
{
    public class Board
    {
        #region Fields and Properties

        public const int CheckMateValue = short.MaxValue,
                         StaleMateValue = 0,
                         Draw = 0;

        public ThreeFoldRepetition threeFoldRepetetion;
        public Zobrist Zobrist;
        private Piece[] pieces;
        public int FiftyMovesRule { get; private set; }
        private int totalMoves;

        private Lazy<bool> LazyIsInCheck = new(() => false, false);
        public bool IsInCheck => LazyIsInCheck.Value;
        public Color Side { get; private set; }
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public Coordinate EnPassantSquare { get; private set; }
        public Castle WhiteCastle { get; private set; }
        public Castle BlackCastle { get; private set; }
        public int StaticEvaluation { get; private set; }

        public Board()
        {
            pieces = new Piece[64];
            WhiteCastle = Castle.NoneCastle;
            BlackCastle = Castle.NoneCastle;
            Side = Color.White;
            EnPassantSquare = new Coordinate(0, 0);//means deactive
            SetUpBoard();
        }

        #endregion
        /// <summary>
        /// Return piece on specified square
        /// </summary>
        /// <param name="square">Square</param>
        /// <returns>Piece on that square</returns>
        public Piece this[Coordinate square]
        {
            get => pieces[square.Index];
            set => pieces[square.Index] = value;
        }


        public BoardState MakeMove(Move move)
        {
            LazyIsInCheck = new(InCheck, false);

            var state = new BoardState(this);

            var movingPiece = move.Piece;

            var capturedPiece = (move as Ordinary)?.CapturedPiece;

            StaticEvaluation -= capturedPiece?.Evaluation() ?? 0;

            if (Side == Color.Black) totalMoves++;

            if (movingPiece is Pawn && Math.Abs((movingPiece.From.Rank - ((Ordinary)move).To.Rank)) == 2)
            {
                if (movingPiece.Color == Color.White)
                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.North);
                else
                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.South);
            }
            else
                EnPassantSquare = new Coordinate(0, 0);

            if (movingPiece is Pawn || capturedPiece is not null)
                FiftyMovesRule = 0;
            else
                FiftyMovesRule++;


            move.MakeMove(this);

            if (movingPiece is King)
            {
                if (movingPiece.Color == Color.White)
                {
                    WhiteCastle = Castle.NoneCastle;
                }
                else
                {
                    BlackCastle = Castle.NoneCastle;
                }

            }
            Piece piece;
            if (WhiteCastle != Castle.NoneCastle)
            {
                piece = Coordinate.a1.GetPiece(this);
                if (!(piece is Rook && piece.Color == Color.White))
                {
                    if (WhiteCastle == Castle.BothCastle)
                    {
                        WhiteCastle = Castle.ShortCastle;
                    }
                    else if (WhiteCastle == Castle.LongCastle)
                    {
                        WhiteCastle = Castle.NoneCastle;
                    }

                }
                piece = Coordinate.h1.GetPiece(this);
                if (!(piece is Rook && piece.Color == Color.White))
                {
                    if (WhiteCastle == Castle.BothCastle)
                    {
                        WhiteCastle = Castle.LongCastle;
                    }
                    else if (WhiteCastle == Castle.ShortCastle)
                    {
                        WhiteCastle = Castle.NoneCastle;
                    }

                }

            }

            if (BlackCastle != Castle.NoneCastle)
            {
                piece = Coordinate.a8.GetPiece(this);
                if (!(piece is Rook && piece.Color == Color.Black))
                {
                    if (BlackCastle == Castle.BothCastle)
                    {
                        BlackCastle = Castle.ShortCastle;
                    }
                    else if (BlackCastle == Castle.LongCastle)
                    {
                        BlackCastle = Castle.NoneCastle;
                    }

                }
                piece = Coordinate.h8.GetPiece(this);
                if (!(piece is Rook && piece.Color == Color.Black))
                {
                    if (BlackCastle == Castle.BothCastle)
                    {
                        BlackCastle = Castle.LongCastle;
                    }
                    else if (BlackCastle == Castle.ShortCastle)
                    {
                        BlackCastle = Castle.NoneCastle;
                    }
                }
            }

            ToggleSide();
            Zobrist.ZobristUpdate(move);
            threeFoldRepetetion.Add(Zobrist.ZobristKey);

            return state;
        }

        public BoardState MakeNullMove()
        {
            LazyIsInCheck = new(InCheck, false);

            var state = new BoardState { ZobristKey = Zobrist.ZobristKey, EnPassantSquare = EnPassantSquare };

            EnPassantSquare = new Coordinate(0, 0);
            ToggleSide();

            Zobrist.ZobristUpdateForNullMove();

            return state;
        }

        public void UndoNullMove(BoardState state)
        {
            EnPassantSquare = state.EnPassantSquare;
            Zobrist.ZobristKey = state.ZobristKey;

            ToggleSide();
        }

        public void UndoMove(Move move, BoardState state)
        {
            var zobristKey = Zobrist.ZobristKey;
            EnPassantSquare = state.EnPassantSquare;
            WhiteCastle = state.WhiteCastle;
            BlackCastle = state.BlackCastle;
            FiftyMovesRule = state.FiftyMovesRule;
            Zobrist.ZobristKey = state.ZobristKey;

            move.UndoMove(this);

            StaticEvaluation += (move as Ordinary)?.CapturedPiece?.Evaluation() ?? 0;

            threeFoldRepetetion.Remove(zobristKey);

            if (Side == Color.Black)
                totalMoves--;

            ToggleSide();

        }

        public IEnumerable<Move> GenerateMoves()
        {
            var king = Side == Color.White ? WhiteKing : BlackKing;

            foreach (var piece in pieces)
            {
                if (piece != null && piece.Color == Side)
                {
                    foreach (var move in piece.GenerateMoves(this))
                    {
                        var state = MakeMove(move);
                        var result = king.From.IsAttackedSquare(this, king.OppenentColor);
                        UndoMove(move, state);

                        if (result)
                        {
                            continue;
                        }

                        yield return move;
                    }
                }
            }
        }

        private bool InCheck()
        {
            var king = Side == Color.White ? WhiteKing : BlackKing;
            return king.From.IsAttackedSquare(this, king.OppenentColor);

        }

        public int GetCheckMateOrStaleMateScore(int depth)
        {
            var king = Side == Color.White ? WhiteKing : BlackKing;
            var attacked = king.From.IsAttackedSquare(this, king.OppenentColor);
            return attacked ? CheckMateValue + depth : StaleMateValue;
        }

        public void SetUpBoard()
        {
            SetFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

            Zobrist = new Zobrist(this);
            threeFoldRepetetion = new ThreeFoldRepetition();
            threeFoldRepetetion.Add(Zobrist.ZobristKey);
        }

        public void SetUpBoard(string fen)
        {
            SetFen(fen);

            Zobrist = new Zobrist(this);
            threeFoldRepetetion = new ThreeFoldRepetition();
            threeFoldRepetetion.Add(Zobrist.ZobristKey);
            StaticEvaluation = Evaluate();
        }

        private void SetFen(string value)
        {

            {
                pieces = new Piece[64];
                var splitted = value.Trim().Split(' ');
                var ranks = splitted[0].Split('/').Reverse().ToArray();
                var allLetters = from rank in ranks
                                 from letter in rank
                                 select letter;
                Side = splitted[1] == "w" ? Color.White : Color.Black;

                var castles = splitted[2];

                if (castles.Contains("KQ")) WhiteCastle = Castle.BothCastle;
                else if (castles.Contains("K")) WhiteCastle = Castle.ShortCastle;
                else if (castles.Contains("Q")) WhiteCastle = Castle.LongCastle;
                else WhiteCastle = Castle.NoneCastle;

                if (castles.Contains("kq")) BlackCastle = Castle.BothCastle;
                else if (castles.Contains("k")) BlackCastle = Castle.ShortCastle;
                else if (castles.Contains("q")) BlackCastle = Castle.LongCastle;
                else BlackCastle = Castle.NoneCastle;

                EnPassantSquare = splitted[3] == "-" ? new Coordinate(0, 0) : Coordinate.NotationToSquare(splitted[3]);

                if (splitted.Length > 4)
                    FiftyMovesRule = Convert.ToInt32(splitted[4]);
                else
                    FiftyMovesRule = 0;
                if (splitted.Length > 5)
                    totalMoves = Convert.ToInt32(splitted[5]);
                else
                    totalMoves = 1;

                Coordinate square = Coordinate.a1;
                foreach (var letter in allLetters)
                {
                    Color color = char.IsUpper(letter) ? Color.White : Color.Black;
                    switch (char.ToUpper(letter))
                    {
                        case Queen.Letter:
                            this[square] = new Queen(square, color);
                            break;
                        case Rook.Letter:
                            this[square] = new Rook(square, color);
                            break;
                        case Bishop.Letter:
                            this[square] = new Bishop(square, color);
                            break;
                        case Knight.Letter:
                            this[square] = new Knight(square, color);
                            break;
                        case 'P':
                            this[square] = new Pawn(square, color);
                            break;
                        case King.Letter:
                            {
                                var king = new King(square, color);
                                this[square] = king;
                                if (color == Color.White) WhiteKing = king;
                                else BlackKing = king;


                            }
                            break;
                        default:
                            {
                                for (int i = 1; i < int.Parse(letter.ToString()); i++)
                                {
                                    square = square.To(Coordinate.Directions.East);

                                }

                            }
                            break;
                    }
                    if (square.File == 8)
                    {
                        if (square.Rank == 8 && square.File == 8) break;
                        square = new Coordinate(square.Rank + 1, 1);
                        continue;
                    }
                    square = square.To(Coordinate.Directions.East);
                }
            }
        }

        public void ShowBoard()
        {
            Coordinate square = new Coordinate(8, 1);
            Console.WriteLine("----------------");
            for (int rank = 0; rank < 8; rank++)
            {

                for (int file = 1; file <= 8; file++)
                {
                    if (this[square] != null)
                    {
                        Console.Write(" ");
                        var letter = this[square].NotationLetter == ' ' ? 'P' : this[square].NotationLetter;
                        letter = square.GetPiece(this).Color == Color.White ? letter : char.ToLower(letter);
                        Console.Write(letter);
                    }
                    else
                    {
                        Console.Write(" -");
                    }
                    square = new Coordinate(square.Rank, square.File + 1);
                }
                Console.WriteLine();
                square = new Coordinate(square.Rank - 1, 1);
            }
            Console.WriteLine("----------------");
        }

        public void ToggleSide()
        {
            Side = Side == Color.White ? Color.Black : Color.White;
        }

        public IEnumerable<Piece> GetPieces()
        {
            return pieces.Cast<Piece>().Where(p => p != null);
        }

        private int Evaluate()
        {
            return GetPieces().Sum(p => p.Evaluation());
        }
    }
}
