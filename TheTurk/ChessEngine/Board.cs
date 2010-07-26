using System;
using System.Collections.Generic;
using System.Linq;
using ChessEngine.Moves;
using ChessEngine.Pieces;

namespace ChessEngine
{
    public partial class Board
    {
        Stack<State> BoardStateHistory;
        Color Side;
        Piece[,] board;
        private int fiftyMovesRule, totalMoves;

        public Board()
        {
            board = new Piece[8, 8];
            WhiteCastle = Castle.NoneCastle;
            BlackCastle = Castle.NoneCastle;
            Side = Color.White;
            enPassantSquare = new Coordinate(0, 0);//means deactive
            BoardStateHistory = new Stack<State>();
        }

        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public Coordinate enPassantSquare { get; private set; }
        public Castle WhiteCastle { get; private set; }
        public Castle BlackCastle { get; private set; }
        public string Fen
        {
            set
            {
                board = new Piece[8, 8];
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

                enPassantSquare = splitted[3] == "-" ? new Coordinate(0, 0) : Coordinate.NotationToSquare(splitted[3]);

                if (splitted.Length > 4)
                    fiftyMovesRule = Convert.ToInt32(splitted[4]);
                else
                    fiftyMovesRule = 0;
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
                        case Queen.letter: this[square] = new Queen(square, color);
                            break;
                        case Rook.letter: this[square] = new Rook(square, color);
                            break;
                        case Bishop.letter: this[square] = new Bishop(square, color);
                            break;
                        case Knight.letter: this[square] = new Knight(square, color);
                            break;
                        case 'P': this[square] = new Pawn(square, color);
                            break;
                        case King.letter:
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
                    if (square.file == 8)
                    {
                        if (square.rank == 8 && square.file == 8) break;
                        square.file = 1;
                        square.rank += 1;
                        continue;
                    }
                    square = square.To(Coordinate.Directions.East);
                }

            }
        }

        /// <summary>
        /// Return piece on specified square
        /// </summary>
        /// <param name="square">Square</param>
        /// <returns>Piece on that square</returns>
        public Piece this[Coordinate square]
        {
            get { return board[square.rank - 1, square.file - 1]; }
            set { board[square.rank - 1, square.file - 1] = value; }
        }

        public void MakeMove(Move move)
        {
            Piece piece = move.piece;

            BoardStateHistory.Push(new State(enPassantSquare, WhiteCastle, BlackCastle, fiftyMovesRule));

            if (Side == Color.Black) totalMoves++;

            if (piece is Pawn && Math.Abs((piece.From.rank - ((Ordinary)move).To.rank)) == 2)
            {
                if (piece.Color == Color.White)
                    enPassantSquare = piece.From.To(Coordinate.Directions.North);
                else
                    enPassantSquare = piece.From.To(Coordinate.Directions.South);
            }
            else
                enPassantSquare = new Coordinate(-1, -1);


            if (piece is Pawn || (move is Ordinary) && (move as Ordinary).CapturedPiece != null)
                fiftyMovesRule = 0;
            else
                fiftyMovesRule++;


            move.MakeMove(this);


            if (piece is King)
            {
                if (piece.Color == Color.White)
                {
                    WhiteCastle = Castle.NoneCastle;
                }
                else
                {
                    BlackCastle = Castle.NoneCastle;
                }

            }
            else if (piece is Rook)
            {
                if (Side == Color.White && WhiteCastle != Castle.NoneCastle)
                {
                    if (Coordinate.a1.GetPiece(this) == null)
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
                    else if (Coordinate.h1.GetPiece(this) == null)
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
                else if (Side == Color.Black && BlackCastle != Castle.NoneCastle)
                {
                    if (Coordinate.a8.GetPiece(this) == null)
                    {
                        if (BlackCastle == Castle.BothCastle)
                        {
                            BlackCastle = Castle.ShortCastle;
                        }
                        else if (BlackCastle == Castle.ShortCastle)
                        {
                            BlackCastle = Castle.NoneCastle;
                        }

                    }
                    else
                    {
                        if (Coordinate.h8.GetPiece(this) == null)
                        {
                            if (BlackCastle == Castle.BothCastle)
                            {
                                BlackCastle = Castle.ShortCastle;
                            }
                            else if (BlackCastle == Castle.ShortCastle)
                            {
                                BlackCastle = Castle.NoneCastle;
                            }
                        }
                    }
                }

            }
            ToggleSide();

        }
        public void TakeBackMove(Move move)
        {
            var state = BoardStateHistory.Pop();
            enPassantSquare = state.enPassantSquare;
            WhiteCastle = state.whiteCastle;
            BlackCastle = state.blackCastle;
            fiftyMovesRule = state.fiftyMovesRule;
            move.UnMakeMove(this);
            if (Side == Color.Black)
                totalMoves--;

            ToggleSide();

        }

        public void SetUpBoard()
        {
            Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
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

                    square.file++;
                }
                Console.WriteLine();
                square.rank--;
                square.file = 1;
            }
            Console.WriteLine("----------------");
        }
        public List<Move> GenerateMoves()
        {
            var moves = new List<Move>();
            King king = Side == Color.White ? WhiteKing : BlackKing;
            foreach (var piece in board)
            {
                if (piece != null && piece.Color == Side)
                {
                    var query = piece.GenerateMoves(this).Where(x =>
                        {
                            MakeMove(x);
                            //Console.Clear();
                            //ShowBoard();
                            //Thread.Sleep(1000);




                            var result = king.From.IsAttackedSquare(this,king.OppenentColor);
                            TakeBackMove(x);
                            //Console.Clear();
                            //ShowBoard();
                            //Thread.Sleep(1000);
                            return !result;
                        }).ToList();
                    moves.AddRange(query);

                }

            }
            return moves;
        }
        void ToggleSide()
        {
            Side = Side == Color.White ? Color.Black : Color.White;
        }

    }
}
