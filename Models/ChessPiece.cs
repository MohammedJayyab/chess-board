namespace ChessGame.Models;

using System.Drawing;
using Svg;

public enum PieceType
{
    King,
    Queen,
    Rook,
    Bishop,
    Knight,
    Pawn
}

public enum PieceColor
{
    White,
    Black
}

public class ChessPiece
{
    public PieceType Type { get; private set; }
    public PieceColor Color { get; private set; }
    public Image? PieceImage { get; private set; }

    public ChessPiece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
        LoadImage();
    }

    private void LoadImage()
    {
        try
        {
            string pieceCode = Type.ToString().ToLower();
            string colorCode = Color == PieceColor.White ? "w" : "b";
            string imagePath = Path.Combine("Assets", "pieces-images", $"{pieceCode}-{colorCode}.svg");
            
            var svgDocument = SvgDocument.Open<SvgDocument>(imagePath);
            if (svgDocument != null)
            {
                PieceImage = svgDocument.Draw(64, 64);
            }
        }
        catch (Exception ex)
        {
            // Log or handle the error appropriately
            Console.WriteLine($"Failed to load image: {ex.Message}");
        }
    }

    public int Value => Type switch
    {
        PieceType.Pawn => 1,
        PieceType.Knight => 3,
        PieceType.Bishop => 3,
        PieceType.Rook => 5,
        PieceType.Queen => 9,
        PieceType.King => 0,
        _ => 0 // Default case for any unhandled values
    };
} 