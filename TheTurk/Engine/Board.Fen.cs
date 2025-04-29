//using TheTurk.Pieces;

//namespace TheTurk.Engine
//{
//    public partial class Board
//    {
//        private void SetFen(string fen)
//        {
//            squares = new Piece[64];
//            var fenParts = fen.Trim().Split(' ');

//            // Parse board position
//            ParseBoardPosition(fenParts[0]);

//            // Parse active color
//            Side = fenParts[1] == "w" ? Color.White : Color.Black;

//            // Parse castling rights
//            ParseCastlingRights(fenParts[2]);

//            // Parse en passant target square
//            EnPassantSquare = fenParts[3] == "-" ? new Coordinate(0, 0) : Coordinate.NotationToSquare(fenParts[3]);

//            // Parse halfmove clock (fifty moves rule)
//            FiftyMovesRule = fenParts.Length > 4 ? Convert.ToInt32(fenParts[4]) : 0;

//            // Parse fullmove number
//            TotalMoves = fenParts.Length > 5 ? Convert.ToInt32(fenParts[5]) : 1;
//        }

//        private void ParseBoardPosition(string boardFen)
//        {
//            var ranks = boardFen.Split('/').Reverse().ToArray();
//            Coordinate square = Coordinate.a1;

//            foreach (var rank in ranks)
//            {
//                foreach (var letter in rank)
//                {
//                    if (char.IsDigit(letter))
//                    {
//                        // Empty squares
//                        int emptyCount = int.Parse(letter.ToString());
//                        for (int i = 0; i < emptyCount; i++)
//                        {
//                            square = AdvanceSquare(square);
//                        }
//                    }
//                    else
//                    {
//                        // Piece
//                        Color color = char.IsUpper(letter) ? Color.White : Color.Black;
//                        char upperLetter = char.ToUpper(letter);

//                        PlacePiece(square, upperLetter, color);
//                        square = AdvanceSquare(square);
//                    }
//                }
//            }
//        }

//        private void ParseCastlingRights(string castles)
//        {
//            if (castles == "-")
//            {
//                WhiteCastle = BlackCastle = Castle.NoneCastle;

//                return;
//            }

//            var whiteCastles = new string(castles.TakeWhile(char.IsUpper).ToArray());
//            var blackCastles = new string(castles.SkipWhile(char.IsUpper).ToArray());

//            WhiteCastle = GetCastleRight(whiteCastles);
//            BlackCastle = GetCastleRight(blackCastles);

//            static Castle GetCastleRight(string castle)
//            {
//                return castle.ToLower() switch
//                {
//                    "k" => Castle.ShortCastle,
//                    "q" => Castle.LongCastle,
//                    "kq" => Castle.BothCastle,
//                    _ => Castle.NoneCastle
//                };
//            }
//        }
//        private void PlacePiece(Coordinate square, char pieceType, Color color)
//        {
//            this[square] = pieceType switch
//            {
//                Queen.Letter => new Queen(square, color),
//                Rook.Letter => new Rook(square, color),
//                Bishop.Letter => new Bishop(square, color),
//                Knight.Letter => new Knight(square, color),
//                'P' => new Pawn(square, color),
//                King.Letter => CreateKing(square, color),
//                _ => throw new ArgumentException($"Invalid piece type: {pieceType}")
//            };


//            King CreateKing(Coordinate pos, Color color)
//            {
//                var king = new King(pos, color);

//                if (color == Color.White)
//                    WhiteKing = king;
//                else
//                    BlackKing = king;
//                return king;
//            }
//        }

//        private Coordinate AdvanceSquare(Coordinate square)
//        {
//            if (square.File == 8)
//            {
//                return new Coordinate(square.Rank + 1, 1);
//            }

//            return square.To(Coordinate.Directions.East);
//        }
//    }
//}
