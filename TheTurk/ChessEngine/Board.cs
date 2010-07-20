using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;
namespace ChessEngine
{
    public partial class Board
    {
        Piece[,] board;
        public Coordinate enPassantSquare { get; private set; }
        Castle whiteCastle, blackCastle;
        Piece.Color Side;
        int fiftyMovesRule;
        int totalMoves;

        public string Fen
        {
            set
            {
                var splitted = value.Split(' ');
                var ranks = splitted[0].Split('/').Reverse().ToArray();
                var allLetters = from rank in ranks
                                 from letter in rank
                                 select letter;

                var castles = splitted[2];

                if (castles.Contains("KQ")) whiteCastle = Castle.BothCastle;
                else if (castles.Contains("K")) whiteCastle = Castle.ShortCastle;
                else if (castles.Contains("Q")) whiteCastle = Castle.LongCastle;

                if (castles.Contains("kq")) blackCastle = Castle.BothCastle;
                else if (castles.Contains("k")) blackCastle = Castle.ShortCastle;
                else if (castles.Contains("q")) blackCastle = Castle.LongCastle;

                enPassantSquare = splitted[3] == "-" ? new Coordinate(0, 0) : Coordinate.ToCoordinate(splitted[3]);

                if (splitted.Length >= 4)
                    fiftyMovesRule = Convert.ToInt32(splitted[4]);
                else
                    fiftyMovesRule = 0;
                if (splitted.Length >= 5)
                    totalMoves = Convert.ToInt32(splitted[5]);
                else
                    totalMoves = 1;



                Coordinate square = Coordinate.a1;
                foreach (var letter in allLetters)
                {
                    Piece.Color color = char.IsUpper(letter) ? Piece.Color.White : Piece.Color.Black;
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
                        case King.letter: this[square] = new King(square, color);
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
        public Board()
        {
            board = new Piece[8, 8];
            whiteCastle = Castle.NoneCastle;
            blackCastle = Castle.NoneCastle;
            Side = Piece.Color.White;
            enPassantSquare = new Coordinate(0, 0);//means deactive

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
        public void SetUpBoard()
        {
            Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        }
        public void ShowBoard()
        {
            Coordinate square = new Coordinate(8, 1);
            for (int rank = 0; rank < 8; rank++)
            {

                for (int file = 1; file <= 8; file++)
                {
                    if (this[square] != null)
                    {
                        //Console.WriteLine(square);
                        Console.Write(this[square].notationLetter == ' ' ? 'p' : this[square].notationLetter);
                    }
                    else
                    {
                        Console.Write("-");
                    }

                    square.file++;
                }
                Console.WriteLine();
                square.rank--;
                square.file = 1;
            }
        }

    }
}
