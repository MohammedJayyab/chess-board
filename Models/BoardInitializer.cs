namespace ChessGame.Models;

public class BoardInitializer
{
    private readonly ChessPiece?[,] board;

    public BoardInitializer(ChessPiece?[,] board)
    {
        this.board = board;
    }

    public void Initialize()
    {
        for (int i = 0; i < 8; i++)
        {
            board[6, i] = new ChessPiece(PieceType.Pawn, PieceColor.Black);
            board[1, i] = new ChessPiece(PieceType.Pawn, PieceColor.White);
        }

        PlacePieces(0, PieceColor.White);
        PlacePieces(7, PieceColor.Black);
    }

    private void PlacePieces(int row, PieceColor color)
    {
        board[row, 0] = new ChessPiece(PieceType.Rook, color);
        board[row, 1] = new ChessPiece(PieceType.Knight, color);
        board[row, 2] = new ChessPiece(PieceType.Bishop, color);
        board[row, 3] = new ChessPiece(PieceType.Queen, color);
        board[row, 4] = new ChessPiece(PieceType.King, color);
        board[row, 5] = new ChessPiece(PieceType.Bishop, color);
        board[row, 6] = new ChessPiece(PieceType.Knight, color);
        board[row, 7] = new ChessPiece(PieceType.Rook, color);
    }
} 