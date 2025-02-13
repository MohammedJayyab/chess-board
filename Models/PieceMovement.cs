namespace ChessGame.Models;

public class PieceMovement
{
    private const int BOARD_SIZE = 8;
    private readonly ChessPiece?[,] board;
    private readonly GameState gameState;
    private readonly MoveValidation moveValidation;
    private readonly CheckDetection checkDetection;
    private readonly GameEvents gameEvents;
    private Point? lastMoveFrom;
    private Point? lastMoveTo;

    public Point? LastMoveFrom => lastMoveFrom;
    public Point? LastMoveTo => lastMoveTo;

    public PieceMovement(ChessPiece?[,] board, GameState gameState, 
        MoveValidation moveValidation, CheckDetection checkDetection, GameEvents gameEvents)
    {
        this.board = board;
        this.gameState = gameState;
        this.moveValidation = moveValidation;
        this.checkDetection = checkDetection;
        this.gameEvents = gameEvents;
    }

    public bool Move(int fromRow, int fromCol, int toRow, int toCol)
    {
        if (!moveValidation.IsValidMove(fromRow, fromCol, toRow, toCol, gameState.CurrentTurn))
            return false;

        var piece = board[fromRow, fromCol];
        if (piece == null) return false;
        
        var targetPiece = board[toRow, toCol];
        bool isCapture = targetPiece != null;

        // Handle special moves
        if (piece.Type == PieceType.King && Math.Abs(toCol - fromCol) == 2)
        {
            // Castling move
            ExecuteCastling(fromRow, fromCol, toRow, toCol);
        }
        else if (piece.Type == PieceType.Pawn && fromCol != toCol && targetPiece == null)
        {
            // En passant capture
            ExecuteEnPassant(fromRow, fromCol, toRow, toCol);
        }
        else
        {
            // Standard move
            ExecuteStandardMove(fromRow, fromCol, toRow, toCol);
        }

        // Record move
        lastMoveFrom = new Point(fromCol, fromRow);
        lastMoveTo = new Point(toCol, toRow);

        // Notify about move
        gameEvents.OnMoveExecuted(new MoveEventArgs(fromRow, fromCol, toRow, toCol, piece, isCapture));

        // Check for pawn promotion
        if (piece.Type == PieceType.Pawn && (toRow == 0 || toRow == 7))
        {
            gameEvents.OnPawnPromotion(toRow, toCol);
        }

        // Update game state
        gameState.SwitchTurns();

        return true;
    }

    private void ExecuteStandardMove(int fromRow, int fromCol, int toRow, int toCol)
    {
        board[toRow, toCol] = board[fromRow, fromCol];
        board[fromRow, fromCol] = null;
    }

    private void ExecuteCastling(int fromRow, int fromCol, int toRow, int toCol)
    {
        bool isKingsideCastling = toCol > fromCol;
        int rookFromCol = isKingsideCastling ? 7 : 0;
        int rookToCol = isKingsideCastling ? toCol - 1 : toCol + 1;

        // Move king
        board[toRow, toCol] = board[fromRow, fromCol];
        board[fromRow, fromCol] = null;

        // Move rook
        board[toRow, rookToCol] = board[fromRow, rookFromCol];
        board[fromRow, rookFromCol] = null;
    }

    private void ExecuteEnPassant(int fromRow, int fromCol, int toRow, int toCol)
    {
        // Move pawn
        board[toRow, toCol] = board[fromRow, fromCol];
        board[fromRow, fromCol] = null;

        // Remove captured pawn
        board[fromRow, toCol] = null;
    }

    // Add methods for specific piece movements
} 