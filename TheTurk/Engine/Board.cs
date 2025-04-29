//namespace TheTurk.Engine
//{
//    public partial class Board
//    {
//        #region Fields and Properties

//        public const int CheckMateValue = 1_000_000,
//                         StaleMateValue = 0,
//                         Draw = 0;

//        private const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

//        public ThreeFoldRepetition ThreeFoldRepetetion;
//        public Zobrist Zobrist;
//        public ulong ZobristKey => Zobrist.ZobristKey;

//        private Piece[] squares;

//        public int FiftyMovesRule { get; private set; }
//        public int TotalMoves { get; private set; }
//        public Color Side { get; private set; }
//        public King WhiteKing { get; private set; }
//        public King BlackKing { get; private set; }
//        public Coordinate EnPassantSquare { get; private set; }
//        public Castle WhiteCastle { get; private set; }
//        public Castle BlackCastle { get; private set; }

//        public bool DisableThreeFoldRepetition { get; set; }

//        public Board(string fen = InitialFen)
//        {
//            SetUpBoard(fen);
//        }


//        #endregion
//        /// <summary>
//        /// Return piece on specified square
//        /// </summary>
//        /// <param name="square">Square</param>
//        /// <returns>Piece on that square</returns>
//        public Piece this[Coordinate square]
//        {
//            get => squares[square.Index];
//            set => squares[square.Index] = value;
//        }

//        public BoardState MakeMove(Move move)
//        {
//            var state = new BoardState(this);

//            var movingPiece = move.Piece;

//            TotalMoves++;

//            if (movingPiece is Pawn && Math.Abs(movingPiece.From.Rank - ((Ordinary)move).To.Rank) == 2)
//            {
//                if (movingPiece.Color == Color.White)
//                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.North);
//                else
//                    EnPassantSquare = movingPiece.From.To(Coordinate.Directions.South);
//            }
//            else
//                EnPassantSquare = new Coordinate(0, 0);

//            var capturedPiece = (move as Ordinary)?.CapturedPiece;

//            if (movingPiece is Pawn || capturedPiece is not null)
//                FiftyMovesRule = 0;
//            else
//                FiftyMovesRule++;

//            move.MakeMove(this);

//            if (movingPiece is King)
//            {
//                if (movingPiece.Color == Color.White)
//                {
//                    WhiteCastle = Castle.NoneCastle;
//                }
//                else
//                {
//                    BlackCastle = Castle.NoneCastle;
//                }
//            }
//            Piece piece;
//            if (WhiteCastle != Castle.NoneCastle)
//            {
//                piece = Coordinate.a1.GetPiece(this);
//                if (!(piece is Rook && piece.Color == Color.White))
//                {
//                    if (WhiteCastle == Castle.BothCastle)
//                    {
//                        WhiteCastle = Castle.ShortCastle;
//                    }
//                    else if (WhiteCastle == Castle.LongCastle)
//                    {
//                        WhiteCastle = Castle.NoneCastle;
//                    }

//                }
//                piece = Coordinate.h1.GetPiece(this);
//                if (!(piece is Rook && piece.Color == Color.White))
//                {
//                    if (WhiteCastle == Castle.BothCastle)
//                    {
//                        WhiteCastle = Castle.LongCastle;
//                    }
//                    else if (WhiteCastle == Castle.ShortCastle)
//                    {
//                        WhiteCastle = Castle.NoneCastle;
//                    }

//                }

//            }

//            if (BlackCastle != Castle.NoneCastle)
//            {
//                piece = Coordinate.a8.GetPiece(this);
//                if (!(piece is Rook && piece.Color == Color.Black))
//                {
//                    if (BlackCastle == Castle.BothCastle)
//                    {
//                        BlackCastle = Castle.ShortCastle;
//                    }
//                    else if (BlackCastle == Castle.LongCastle)
//                    {
//                        BlackCastle = Castle.NoneCastle;
//                    }

//                }
//                piece = Coordinate.h8.GetPiece(this);
//                if (!(piece is Rook && piece.Color == Color.Black))
//                {
//                    if (BlackCastle == Castle.BothCastle)
//                    {
//                        BlackCastle = Castle.LongCastle;
//                    }
//                    else if (BlackCastle == Castle.ShortCastle)
//                    {
//                        BlackCastle = Castle.NoneCastle;
//                    }
//                }
//            }

//            ToggleSide();

//            Zobrist.ZobristUpdate(move, state);

//            if (!DisableThreeFoldRepetition)
//            {
//                var cancelThreeFold = move is LongCastle or ShortCastle || movingPiece is Pawn || capturedPiece is not null;

//                ThreeFoldRepetetion.Add(Zobrist.ZobristKey, cancelThreeFold);
//            }

//            return state;
//        }

//        public BoardState MakeNullMove()
//        {
//            var state = new BoardState { ZobristKey = Zobrist.ZobristKey, EnPassantSquare = EnPassantSquare };

//            EnPassantSquare = new Coordinate(0, 0);
//            ToggleSide();

//            Zobrist.ZobristUpdateForNullMove();

//            return state;
//        }

//        public void UndoNullMove(BoardState state)
//        {
//            EnPassantSquare = state.EnPassantSquare;
//            Zobrist.ZobristKey = state.ZobristKey;

//            ToggleSide();
//        }

//        public void UndoMove(Move move, BoardState state)
//        {
//            var zobristKey = Zobrist.ZobristKey;
//            EnPassantSquare = state.EnPassantSquare;
//            WhiteCastle = state.WhiteCastle;
//            BlackCastle = state.BlackCastle;
//            FiftyMovesRule = state.FiftyMovesRule;
//            Zobrist.ZobristKey = state.ZobristKey;

//            move.UndoMove(this);

//            if (!DisableThreeFoldRepetition)
//            {
//                ThreeFoldRepetetion.Remove();
//            }

//            TotalMoves--;

//            ToggleSide();
//        }

//        public IEnumerable<Move> GenerateMoves()
//        {
//            var side = Side;

//            foreach (var piece in squares)
//            {
//                if (piece?.Color == Side)
//                {
//                    foreach (var move in piece.GenerateMoves(this))
//                    {
//                        var state = MakeMove(move);
//                        var inCheck = InCheck(side);
//                        UndoMove(move, state);

//                        if (inCheck)
//                        {
//                            continue;
//                        }

//                        yield return move;
//                    }
//                }
//            }
//        }


//        public bool InCheck() => InCheck(Side);

//        /// <summary>
//        /// Check if the specified side is in check
//        /// </summary>
//        /// <param name="sideIncheck">Side to check</param>
//        /// <returns>True if in check, otherwise false</returns>
//        private bool InCheck(Color sideIncheck)
//        {
//            var king = sideIncheck == Color.White ? WhiteKing : BlackKing;
//            return king.From.IsAttackedSquare(this, king.OppenentColor);
//        }

//        public int GetCheckMateOrStaleMateScore(int ply)
//        {
//            var mate = CheckMateValue - ply;
//            return InCheck() ? -mate : StaleMateValue;
//        }

//        public void SetUpBoard(string fen = InitialFen)
//        {
//            SetFen(fen);
//            Zobrist = new Zobrist(this);
//            ThreeFoldRepetetion = new ThreeFoldRepetition();
//            ThreeFoldRepetetion.Add(Zobrist.ZobristKey, false);
//        }

//        public void ShowBoard()
//        {

//            var symbols = new Dictionary<char, char>
//            {
//                {'K', '♔'}, // beyaz kral
//                {'Q', '♕'}, // beyaz vezir
//                {'R', '♖'}, // beyaz kale
//                {'B', '♗'}, // beyaz fil
//                {'N', '♘'}, // beyaz at
//                {'P', '♙'}, // beyaz piyon
//                {'k', '♚'}, // siyah kral
//                {'q', '♛'}, // siyah vezir
//                {'r', '♜'}, // siyah kale
//                {'b', '♝'}, // siyah fil
//                {'n', '♞'}, // siyah at
//                {'p', '♙'}  // siyah piyon
//            };

//            Console.WriteLine("+-----------------+");

//            for (int rank = 8; rank >= 1; rank--)
//            {
//                Console.Write("|");
//                for (int file = 1; file <= 8; file++)
//                {
//                    var square = new Coordinate(rank, file);
//                    var piece = this[square];
//                    if (piece != null)
//                    {
//                        var letter = piece.NotationLetter == ' ' ? 'P' : piece.NotationLetter;
//                        (letter, var color) = piece.Color == Color.White ? (letter, ConsoleColor.White) : (char.ToLower(letter), ConsoleColor.DarkGray);

//                        Console.ForegroundColor = color;
//                        Console.Write(" " + symbols[letter]);
//                        Console.ResetColor();
//                    }
//                    else
//                    {
//                        Console.Write(" -");
//                    }
//                }
//                Console.WriteLine(" |" + rank);
//            }

//            Console.WriteLine("+-----------------+");
//            Console.WriteLine("  a b c d e f g h");

//            Console.WriteLine();

//            Console.WriteLine($"Fen : {GetFen()}");
//            Console.WriteLine();
//            Console.WriteLine();


//        }
//        public void ToggleSide()
//        {
//            Side = Side == Color.White ? Color.Black : Color.White;
//        }

//        public IEnumerable<Piece> GetPieces()
//        {
//            return squares.Cast<Piece>().Where(p => p != null);
//        }

//        public int Evaluate()
//        {
//            return Side.AsInt() * (GetPieces().Sum(p => p.Evaluation())/* - depth * 5*/);
//        }

//        public static (bool IsCheckmate, int MateIn, int Score) GetCheckmateInfo(int score)
//        {
//            var sign = Math.Sign(score);

//            score = sign * score; //abs()

//            var isMate = score + 2000 > CheckMateValue;

//            if (!isMate)
//                return (false, 0, 0);

//            var mateIn = CheckMateValue -  score;

//            return (true, sign * mateIn, sign * score);
//        }
//    }
//}
