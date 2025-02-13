namespace ChessGame.Models;

public class CheckDetection
{
    private const int BOARD_SIZE = 8;
    private readonly ChessPiece?[,] board;
    private MoveValidation? moveValidation;  // Allow it to be set later

    public CheckDetection(ChessPiece?[,] board)
    {
        this.board = board;
    }

    public void SetMoveValidation(MoveValidation moveValidation)
    {
        this.moveValidation = moveValidation;
    }

    public bool IsInCheck(PieceColor color)
    {
        Point? kingPos = FindKing(color);
        return kingPos.HasValue && IsSquareUnderAttack(kingPos.Value.Y, kingPos.Value.X, color);
    }

    private Point? FindKing(PieceColor color)
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var piece = board[row, col];
                if (piece?.Type == PieceType.King && piece.Color == color)
                {
                    return new Point(col, row);
                }
            }
        }
        return null;
    }

    private bool IsSquareUnderAttack(int row, int col, PieceColor defendingColor)
    {
        if (moveValidation == null) return false;  // Early return if not initialized
        
        for (int fromRow = 0; fromRow < BOARD_SIZE; fromRow++)
        {
            for (int fromCol = 0; fromCol < BOARD_SIZE; fromCol++)
            {
                var piece = board[fromRow, fromCol];
                if (piece != null && piece.Color != defendingColor)
                {
                    if (moveValidation.IsValidMove(fromRow, fromCol, row, col, piece.Color))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Move all check-related methods here
} 