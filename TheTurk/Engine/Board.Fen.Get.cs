//using TheTurk.Pieces;

//namespace TheTurk.Engine;

//public partial class Board
//{
//    /// <summary>
//    /// Tahtanın mevcut durumunu FEN (Forsyth-Edwards Notation) formatında döndürür
//    /// </summary>
//    /// <returns>FEN formatında string</returns>
//    public string GetFen()
//    {
//        // 1. Tahta pozisyonu
//        var boardPosition = GetBoardPositionFen();

//        // 2. Aktif renk
//        var activeColor = Side == Color.White ? "w" : "b";

//        // 3. Rok hakları
//        var castlingRights = GetCastlingRightsFen();

//        // 4. En passant hedef karesi
//        var enPassant = IsValidEnPassantSquare(EnPassantSquare)
//            ? EnPassantSquare.ToString()
//            : "-";

//        // 5. Yarım hamle sayacı (50 hamle kuralı)
//        var halfMoveClock = FiftyMovesRule.ToString();

//        // 6. Toplam hamle sayısı
//        var fullMoveNumber = TotalMoves.ToString();

//        // Tüm parçaları birleştir
//        return string.Join(" ", boardPosition, activeColor, castlingRights, enPassant, halfMoveClock, fullMoveNumber);
//    }

//    private string GetBoardPositionFen()
//    {
//        var ranks = new string[8];

//        for (var rank = 8; rank >= 1; rank--)
//        {
//            var rankStr = "";
//            var emptyCount = 0;

//            for (var file = 1; file <= 8; file++)
//            {
//                var square = new Coordinate(rank, file);
//                var piece = this[square];

//                if (piece == null)
//                {
//                    emptyCount++;
//                    continue;
//                }

//                if (emptyCount > 0)
//                {
//                    rankStr += emptyCount.ToString();
//                    emptyCount = 0;
//                }

//                var pieceChar = GetPieceChar(piece);
//                rankStr += pieceChar;
//            }

//            if (emptyCount > 0)
//            {
//                rankStr += emptyCount.ToString();
//            }

//            ranks[8 - rank] = rankStr;
//        }

//        return string.Join("/", ranks);
//    }

//    private string GetCastlingRightsFen()
//    {
//        var castlingRights = "";

//        // Beyaz rok hakları
//        if (WhiteCastle.HasFlag(Castle.ShortCastle))
//        {
//            castlingRights += "K";
//        }

//        if (WhiteCastle.HasFlag(Castle.LongCastle))
//        {
//            castlingRights += "Q";
//        }

//        // Siyah rok hakları
//        if (BlackCastle.HasFlag(Castle.ShortCastle))
//        {
//            castlingRights += "k";
//        }

//        if (BlackCastle.HasFlag(Castle.LongCastle))
//        {
//            castlingRights += "q";
//        }

//        return castlingRights.Length > 0 ? castlingRights : "-";
//    }

//    private static char GetPieceChar(Piece piece)
//    {
//        var c = piece is Pawn ? 'P' : piece.NotationLetter;

//        return piece.Color == Color.Black ? char.ToLower(c) : c;
//    }

//    private static bool IsValidEnPassantSquare(Coordinate square)
//    {
//        return square.Rank != 0 && square.File != 0;
//    }
//}
