using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChessEngine.Moves;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    public partial class Board : IEnumerable
    {
        Stack<State> BoardStateHistory;

        Piece[,] board;
        private int fiftyMovesRule, totalMoves;

        public Board()
        {
            board = new Piece[8, 8];
            WhiteCastle = Castle.NoneCastle;
            BlackCastle = Castle.NoneCastle;
            Side = Color.White;
            EnPassantSquare = new Coordinate(0, 0);//means deactive
            BoardStateHistory = new Stack<State>();
            SetUpBoard();
        }
        public Color Side { get; private set; }
        public King WhiteKing { get; private set; }
        public King BlackKing { get; private set; }
        public Coordinate EnPassantSquare { get; private set; }
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

                EnPassantSquare = splitted[3] == "-" ? new Coordinate(0, 0) : Coordinate.NotationToSquare(splitted[3]);

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
                        case Queen.Letter: this[square] = new Queen(square, color);
                            break;
                        case Rook.Letter: this[square] = new Rook(square, color);
                            break;
                        case Bishop.Letter: this[square] = new Bishop(square, color);
                            break;
                        case Knight.Letter: this[square] = new Knight(square, color);
                            break;
                        case 'P': this[square] = new Pawn(square, color);
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
            Piece movingPiece = move.piece;

            BoardStateHistory.Push(new State(EnPassantSquare, WhiteCastle, BlackCastle, fiftyMovesRule));

            if (Side == Color.Black) totalMoves++;

            if (movingPiece is Pawn && Math.Abs((movingPiece.From.rank - ((Ordinary)move).To.rank)) == 2)
            {
                if (movingPiece.Color == Color.White)
                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.North);
                else
                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.South);
            }
            else
                EnPassantSquare = new Coordinate(-1, -1);


            if (movingPiece is Pawn || (move is Ordinary) && (move as Ordinary).CapturedPiece != null)
                fiftyMovesRule = 0;
            else
                fiftyMovesRule++;


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

        }
        public void TakeBackMove(Move move)
        {
            var state = BoardStateHistory.Pop();
            EnPassantSquare = state.enPassantSquare;
            WhiteCastle = state.whiteCastle;
            BlackCastle = state.blackCastle;
            fiftyMovesRule = state.fiftyMovesRule;
            move.UnMakeMove(this);
            if (Side == Color.Black)
                totalMoves--;

            ToggleSide();

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




                            var result = king.From.IsAttackedSquare(this, king.OppenentColor);
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

        public bool IsInCheck()
        {
            var king = Side == Color.White ? WhiteKing : BlackKing;
            return king.From.IsAttackedSquare(this, king.OppenentColor);

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
        void ToggleSide()
        {
            Side = Side == Color.White ? Color.Black : Color.White;
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var piece = board[i, j]; 
                    if (piece!=null)
                    {
                        yield return piece ;
                    }
                    
                }
            }
            
        }
    }
}
