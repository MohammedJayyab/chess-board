namespace ChessGame.Models;

using System.Drawing;
using System.Collections.Generic;

public class ChessBoard
{
    private readonly ChessPiece?[,] board;
    private const int BOARD_SIZE = 8;
    private PieceColor currentTurn;
    private bool[,] hasMoved; // Track if pieces have moved (for castling)
    private Point? lastPawnDoubleMove; // Track last pawn double move (for en passant)
    private bool isPromotion;
    private Point? lastMoveFrom;
    private Point? lastMoveTo;
    private bool isCheck;
    public event EventHandler<PawnPromotionEventArgs>? PawnPromotionRequired;
    public event EventHandler<MoveEventArgs>? MoveExecuted;

    public Point? LastMoveFrom => lastMoveFrom;
    public Point? LastMoveTo => lastMoveTo;
    public bool IsCheck => isCheck;
    public PieceColor CurrentTurn => currentTurn;

    public ChessBoard()
    {
        board = new ChessPiece[BOARD_SIZE, BOARD_SIZE];
        hasMoved = new bool[BOARD_SIZE, BOARD_SIZE];
        currentTurn = PieceColor.White; // White moves first
        lastPawnDoubleMove = null;
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        // Place pawns
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            board[6, i] = new ChessPiece(PieceType.Pawn, PieceColor.Black); // Black pawns on rank 7
            board[1, i] = new ChessPiece(PieceType.Pawn, PieceColor.White); // White pawns on rank 2
        }

        // Place White pieces at bottom (rank 1)
        PlacePieces(0, PieceColor.White);
        
        // Place Black pieces at top (rank 8)
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

    public ChessPiece? GetPiece(int row, int col)
    {
        if (IsValidPosition(row, col))
            return board[row, col];
        return null;
    }

    public bool MovePiece(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (!IsValidPosition(fromRow, fromCol) || !IsValidPosition(toRow, toCol))
            return false;

        var piece = board[fromRow, fromCol];
        if (piece == null || piece.Color != currentTurn)
            return false;

        if (!IsValidMove(fromRow, fromCol, toRow, toCol))
            return false;

        // Check if there's an enemy piece to capture BEFORE making the move
        var targetPiece = board[toRow, toCol];
        bool isCapture = targetPiece != null && targetPiece.Color != piece.Color;

        // Make the move
        board[toRow, toCol] = piece;
        board[fromRow, fromCol] = null;

        // Record the move
        lastMoveFrom = new Point(fromCol, fromRow);
        lastMoveTo = new Point(toCol, toRow);

        // Pass the capture information
        MoveExecuted?.Invoke(this, new MoveEventArgs(fromRow, fromCol, toRow, toCol, piece, isCapture));

        SwitchTurns();
        return true;
    }

    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
    }

    public bool IsValidMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        var piece = GetPiece(fromRow, fromCol);
        if (piece == null) return false;

        // Can't move to a square occupied by own piece
        var targetPiece = GetPiece(toRow, toCol);
        if (targetPiece != null && targetPiece.Color == piece.Color)
            return false;

        // Check basic piece movement rules
        bool isValidPieceMove = piece.Type switch
        {
            PieceType.Pawn => IsValidPawnMove(fromRow, fromCol, toRow, toCol, piece.Color),
            PieceType.Knight => IsValidKnightMove(fromRow, fromCol, toRow, toCol),
            PieceType.Bishop => IsValidBishopMove(fromRow, fromCol, toRow, toCol),
            PieceType.Rook => IsValidRookMove(fromRow, fromCol, toRow, toCol),
            PieceType.Queen => IsValidQueenMove(fromRow, fromCol, toRow, toCol),
            PieceType.King => IsValidKingMove(fromRow, fromCol, toRow, toCol),
            _ => false
        };

        if (!isValidPieceMove) return false;

        // Check if move would put own king in check
        var tempBoard = CloneBoardState();
        tempBoard[toRow, toCol] = piece;
        tempBoard[fromRow, fromCol] = null;

        // Find king position after move
        Point? kingPos = FindKing(piece.Color, tempBoard);
        if (!kingPos.HasValue) return false;

        // If this move would leave own king in check, it's illegal
        return !IsSquareUnderAttack(kingPos.Value.Y, kingPos.Value.X, piece.Color, tempBoard);
    }

    private bool IsValidPawnMove(int fromRow, int fromCol, int toRow, int toCol, PieceColor color)
    {
        int direction = color == PieceColor.White ? 1 : -1;
        int startRow = color == PieceColor.White ? 1 : 6;

        // Forward moves
        if (fromCol == toCol)
        {
            // Single square move
            if (toRow == fromRow + direction && GetPiece(toRow, toCol) == null)
                return true;

            // Double square move from starting position
            if (fromRow == startRow && 
                toRow == fromRow + (2 * direction) && 
                GetPiece(fromRow + direction, toCol) == null && 
                GetPiece(toRow, toCol) == null)
                return true;
        }

        // Captures (including en passant)
        if (Math.Abs(toCol - fromCol) == 1 && toRow == fromRow + direction)
        {
            // Normal capture
            if (GetPiece(toRow, toCol) != null)
                return true;

            // En passant
            if (lastPawnDoubleMove.HasValue &&
                lastPawnDoubleMove.Value.X == toCol &&
                lastPawnDoubleMove.Value.Y == fromRow)
                return true;
        }

        return false;
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
        else
            return IsValidBishopMove(fromRow, fromCol, toRow, toCol);
    }

    private bool IsValidKingMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(toRow - fromRow);
        int colDiff = Math.Abs(toCol - fromCol);

        // Normal king move
        if (rowDiff <= 1 && colDiff <= 1)
            return true;

        // Castling
        if (rowDiff == 0 && colDiff == 2 && !hasMoved[fromRow, fromCol])
        {
            int rookCol = toCol > fromCol ? 7 : 0;
            if (!hasMoved[fromRow, rookCol])
            {
                // Check if path is clear and not under attack
                int step = toCol > fromCol ? 1 : -1;
                for (int col = fromCol + step; col != rookCol; col += step)
                {
                    if (GetPiece(fromRow, col) != null || 
                        IsSquareUnderAttack(fromRow, col, CurrentTurn, board))
                        return false;
                }
                return true;
            }
        }

        return false;
    }

    private bool IsPathClear(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowStep = fromRow == toRow ? 0 : (toRow - fromRow) / Math.Abs(toRow - fromRow);
        int colStep = fromCol == toCol ? 0 : (toCol - fromCol) / Math.Abs(toCol - fromCol);

        int currentRow = fromRow + rowStep;
        int currentCol = fromCol + colStep;

        while (currentRow != toRow || currentCol != toCol)
        {
            if (GetPiece(currentRow, currentCol) != null)
                return false;
            currentRow += rowStep;
            currentCol += colStep;
        }

        return true;
    }

    public bool IsInCheck(PieceColor color)
    {
        // Find the king
        ChessPiece? king = null;
        int kingRow = -1, kingCol = -1;
        
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var piece = board[row, col];
                if (piece?.Type == PieceType.King && piece.Color == color)
                {
                    king = piece;
                    kingRow = row;
                    kingCol = col;
                    break;
                }
            }
            if (king != null) break;
        }
        
        if (king == null) return false;

        // Check if any opponent piece can capture the king
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var piece = board[row, col];
                if (piece != null && piece.Color != color)
                {
                    if (CanPieceMoveToSquare(piece, row, col, kingRow, kingCol))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }

    public bool IsCheckmate()
    {
        if (!IsInCheck(CurrentTurn))
            return false;

        // Check all possible moves for the current player
        for (int fromRow = 0; fromRow < BOARD_SIZE; fromRow++)
        {
            for (int fromCol = 0; fromCol < BOARD_SIZE; fromCol++)
            {
                var piece = board[fromRow, fromCol];
                if (piece?.Color != CurrentTurn) continue;

                // Try all possible destination squares
                for (int toRow = 0; toRow < BOARD_SIZE; toRow++)
                {
                    for (int toCol = 0; toCol < BOARD_SIZE; toCol++)
                    {
                        if (IsValidMove(fromRow, fromCol, toRow, toCol))
                        {
                            return false; // Found a legal move
                        }
                    }
                }
            }
        }
        
        return true; // No legal moves found
    }

    public (int, int)? GetPiecePosition(ChessPiece piece)
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if (board[row, col] == piece)
                {
                    return (row, col);
                }
            }
        }
        return null;
    }

    private bool CanPieceMoveToSquare(ChessPiece piece, int fromRow, int fromCol, int toRow, int toCol)
    {
        // Implement basic piece movement rules
        // This is a simplified version - you'll need to implement the full chess rules
        switch (piece.Type)
        {
            case PieceType.Pawn:
                // Implement pawn movement rules
                return true; // Simplified for now
            case PieceType.Knight:
                var rowDiff = Math.Abs(toRow - fromRow);
                var colDiff = Math.Abs(toCol - fromCol);
                return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
            // Add other piece types...
            default:
                return false;
        }
    }

    private bool IsKingInCheck(PieceColor color, ChessPiece?[,] boardState)
    {
        // Find king position
        Point? kingPos = FindKing(color, boardState);
        if (!kingPos.HasValue) return false;

        // Check if any opponent piece can attack king
        return IsSquareUnderAttack(kingPos.Value.Y, kingPos.Value.X, color, boardState);
    }

    private bool IsSquareUnderAttack(int row, int col, PieceColor defendingColor, ChessPiece?[,] boardState)
    {
        // Check for attacks from all opponent pieces
        for (int fromRow = 0; fromRow < BOARD_SIZE; fromRow++)
        {
            for (int fromCol = 0; fromCol < BOARD_SIZE; fromCol++)
            {
                var piece = boardState[fromRow, fromCol];
                if (piece == null || piece.Color == defendingColor)
                    continue;

                // First check if the piece's movement pattern allows attack
                bool canAttack = piece.Type switch
                {
                    PieceType.Pawn => CanPawnAttack(fromRow, fromCol, row, col, piece.Color),
                    PieceType.Rook => (fromRow == row || fromCol == col),
                    PieceType.Knight => IsValidKnightMove(fromRow, fromCol, row, col),
                    PieceType.Bishop => Math.Abs(row - fromRow) == Math.Abs(col - fromCol),
                    PieceType.Queen => (fromRow == row || fromCol == col) || 
                                     (Math.Abs(row - fromRow) == Math.Abs(col - fromCol)),
                    PieceType.King => Math.Abs(row - fromRow) <= 1 && Math.Abs(col - fromCol) <= 1,
                    _ => false
                };

                // If the piece can potentially attack, check if path is clear
                if (canAttack)
                {
                    // Knights can jump over pieces
                    if (piece.Type == PieceType.Knight)
                        return true;

                    // For all other pieces, check if path is clear
                    bool pathClear = true;
                    int rowStep = fromRow == row ? 0 : (row - fromRow) / Math.Abs(row - fromRow);
                    int colStep = fromCol == col ? 0 : (col - fromCol) / Math.Abs(col - fromCol);

                    int currentRow = fromRow + rowStep;
                    int currentCol = fromCol + colStep;

                    // Check each square along the path
                    while (currentRow != row || currentCol != col)
                    {
                        if (boardState[currentRow, currentCol] != null)
                        {
                            pathClear = false;
                            break;
                        }
                        currentRow += rowStep;
                        currentCol += colStep;
                    }

                    if (pathClear)
                        return true;
                }
            }
        }
        return false;
    }

    private bool CanPawnAttack(int fromRow, int fromCol, int toRow, int toCol, PieceColor color)
    {
        int direction = color == PieceColor.White ? 1 : -1;
        return toRow == fromRow + direction && Math.Abs(toCol - fromCol) == 1;
    }

    public void PromotePawn(int row, int col, PieceType newType)
    {
        if (!isPromotion) return;
        
        var pawn = board[row, col];
        if (pawn?.Type == PieceType.Pawn)
        {
            board[row, col] = new ChessPiece(newType, pawn.Color);
        }
        
        isPromotion = false;
        SwitchTurns();
        isCheck = IsKingInCheck(currentTurn, board);
    }

    private ChessPiece?[,] CloneBoardState()
    {
        var clone = new ChessPiece?[BOARD_SIZE, BOARD_SIZE];
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                clone[row, col] = board[row, col];
            }
        }
        return clone;
    }

    private Point? FindKing(PieceColor color, ChessPiece?[,] boardState)
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var piece = boardState[row, col];
                if (piece?.Type == PieceType.King && piece.Color == color)
                {
                    return new Point(col, row);
                }
            }
        }
        return null;
    }

    private bool HandlePawnPromotion(int toRow, PieceColor color)
    {
        // Pawn reaches the opposite end (promotion)
        return (color == PieceColor.White && toRow == 7) || 
               (color == PieceColor.Black && toRow == 0);
    }

    private void SwitchTurns()
    {
        currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    private bool IsValidKnightMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        int rowDiff = Math.Abs(toRow - fromRow);
        int colDiff = Math.Abs(toCol - fromCol);
        return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
    }

    public IEnumerable<ChessPiece> GetPieces()
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if (board[row, col] != null)
                {
                    yield return board[row, col]!;
                }
            }
        }
    }
}

public class PawnPromotionEventArgs : EventArgs
{
    public int Row { get; }
    public int Col { get; }

    public PawnPromotionEventArgs(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

public class MoveEventArgs : EventArgs
{
    public int FromRow { get; }
    public int FromCol { get; }
    public int ToRow { get; }
    public int ToCol { get; }
    public ChessPiece Piece { get; }
    public bool WasCapture { get; }

    public MoveEventArgs(int fromRow, int fromCol, int toRow, int toCol, ChessPiece piece, bool wasCapture)
    {
        FromRow = fromRow;
        FromCol = fromCol;
        ToRow = toRow;
        ToCol = toCol;
        Piece = piece;
        WasCapture = wasCapture;
    }
} 