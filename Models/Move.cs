namespace ChessGame.Models;

public class Move
{
    public Point From { get; }
    public Point To { get; }
    public ChessPiece Piece { get; }
    public bool IsCapture { get; }
    public bool IsCheck { get; }
    public bool IsCheckmate { get; }
    public PieceType? PromotionPiece { get; }

    public Move(Point from, Point to, ChessPiece piece, bool isCapture, bool isCheck, bool isCheckmate, PieceType? promotionPiece)
    {
        From = from;
        To = to;
        Piece = piece;
        IsCapture = isCapture;
        IsCheck = isCheck;
        IsCheckmate = isCheckmate;
        PromotionPiece = promotionPiece;
    }
} 