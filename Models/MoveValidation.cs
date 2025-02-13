namespace ChessGame.Models;

public class MoveValidation
{
    private const int BOARD_SIZE = 8;
    private readonly ChessPiece?[,] board;
    private readonly CheckDetection checkDetection;

    public MoveValidation(ChessPiece?[,] board, CheckDetection checkDetection)
    {
        this.board = board;
        this.checkDetection = checkDetection;
    }

    public bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol, PieceColor currentTurn)
    {
        if (!IsBasicMoveValid(fromRow, fromCol, toRow, toCol, currentTurn))
            return false;

        // Make temporary move and check if it leaves king in check
        var piece = board[fromRow, fromCol];
        var targetPiece = board[toRow, toCol];
        
        // Make move
        board[toRow, toCol] = piece;
        board[fromRow, fromCol] = null;
        
        // Check if move leaves own king in check
        bool isValid = !checkDetection.IsInCheck(currentTurn);
        
        // Undo move
        board[fromRow, fromCol] = piece;
        board[toRow, toCol] = targetPiece;
        
        return isValid;
    }

    private bool IsBasicMoveValid(int fromRow, int fromCol, int toRow, int toCol, PieceColor currentTurn)
    {
        if (!IsValidPosition(fromRow, fromCol) || !IsValidPosition(toRow, toCol))
            return false;

        var piece = board[fromRow, fromCol];
        if (piece == null || piece.Color != currentTurn)
            return false;

        // Can't move to a square occupied by own piece
        var targetPiece = board[toRow, toCol];
        if (targetPiece != null && targetPiece.Color == piece.Color)
            return false;

        return piece.Type switch
        {
            PieceType.Pawn => IsValidPawnMove(fromRow, fromCol, toRow, toCol, piece.Color),
            PieceType.Knight => IsValidKnightMove(fromRow, fromCol, toRow, toCol),
            PieceType.Bishop => IsValidBishopMove(fromRow, fromCol, toRow, toCol),
            PieceType.Rook => IsValidRookMove(fromRow, fromCol, toRow, toCol),
            PieceType.Queen => IsValidQueenMove(fromRow, fromCol, toRow, toCol),
            PieceType.King => IsValidKingMove(fromRow, fromCol, toRow, toCol),
            _ => false
        };
    }

    private bool IsValidPawnMove(int fromRow, int fromCol, int toRow, int toCol, PieceColor color)
    {
        int direction = color == PieceColor.White ? 1 : -1;
        int startRow = color == PieceColor.White ? 1 : 6;

        // Forward moves
        if (fromCol == toCol)
        {
            // Single square move
            if (toRow == fromRow + direction && board[toRow, toCol] == null)
                return true;

            // Double square move from starting position
            if (fromRow == startRow && 
                toRow == fromRow + (2 * direction) && 
                board[fromRow + direction, toCol] == null && 
                board[toRow, toCol] == null)
                return true;
        }

        // Captures
        if (Math.Abs(toCol - fromCol) == 1 && toRow == fromRow + direction)
        {
            return board[toRow, toCol] != null && board[toRow, toCol]?.Color != color;
        }

        return false;
    }

    private bool IsValidKnightMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(toRow - fromRow);
        int colDiff = Math.Abs(toCol - fromCol);
        return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
    }

    private bool IsValidBishopMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (Math.Abs(toRow - fromRow) != Math.Abs(toCol - fromCol))
            return false;

        return IsPathClear(fromRow, fromCol, toRow, toCol);
    }

    private bool IsValidRookMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (fromRow != toRow && fromCol != toCol)
            return false;

        return IsPathClear(fromRow, fromCol, toRow, toCol);
    }

    private bool IsValidQueenMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (fromRow == toRow || fromCol == toCol)
            return IsValidRookMove(fromRow, fromCol, toRow, toCol);
        
        return IsValidBishopMove(fromRow, fromCol, toRow, toCol);
    }

    private bool IsValidKingMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(toRow - fromRow);
        int colDiff = Math.Abs(toCol - fromCol);
        return rowDiff <= 1 && colDiff <= 1;
    }

    private bool IsPathClear(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowStep = fromRow == toRow ? 0 : (toRow - fromRow) / Math.Abs(toRow - fromRow);
        int colStep = fromCol == toCol ? 0 : (toCol - fromCol) / Math.Abs(toCol - fromCol);

        int currentRow = fromRow + rowStep;
        int currentCol = fromCol + colStep;

        while (currentRow != toRow || currentCol != toCol)
        {
            if (board[currentRow, currentCol] != null)
                return false;

            currentRow += rowStep;
            currentCol += colStep;
        }

        return true;
    }

    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
    }
} 