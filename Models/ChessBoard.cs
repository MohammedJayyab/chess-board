namespace ChessGame.Models;

using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

public class ChessBoard
{
    private const int BOARD_SIZE = 8;
    private readonly ChessPiece?[,] board;
    private readonly GameState gameState;
    private readonly PieceMovement pieceMovement;
    private readonly BoardInitializer boardInitializer;
    private readonly MoveValidation moveValidation;
    private readonly CheckDetection checkDetection;
    private readonly GameEvents gameEvents;
    private readonly List<Move> moveHistory;

    public event EventHandler<PawnPromotionEventArgs>? PawnPromotionRequired;
    public event EventHandler<MoveEventArgs>? MoveExecuted;

    public Point? LastMoveFrom => pieceMovement.LastMoveFrom;
    public Point? LastMoveTo => pieceMovement.LastMoveTo;
    public bool IsCheck => checkDetection.IsInCheck(gameState.CurrentTurn);
    public PieceColor CurrentTurn => gameState.CurrentTurn;

    public PieceColor? CheckedKing => checkDetection.IsInCheck(PieceColor.White) ? PieceColor.White : 
                                     checkDetection.IsInCheck(PieceColor.Black) ? PieceColor.Black : null;

    public ChessBoard()
    {
        board = new ChessPiece[BOARD_SIZE, BOARD_SIZE];
        moveHistory = new List<Move>();
        gameState = new GameState();
        boardInitializer = new BoardInitializer(board);
        checkDetection = new CheckDetection(board);
        moveValidation = new MoveValidation(board, checkDetection);
        checkDetection.SetMoveValidation(moveValidation);
        gameEvents = new GameEvents();
        pieceMovement = new PieceMovement(board, gameState, moveValidation, checkDetection, gameEvents);
        
        // Connect events
        gameEvents.MoveExecuted += (s, e) => MoveExecuted?.Invoke(this, e);
        gameEvents.PawnPromotionRequired += (s, e) => PawnPromotionRequired?.Invoke(this, e);
        
        InitializeGame();
    }

    private void InitializeGame()
    {
        boardInitializer.Initialize();
    }

    public ChessPiece? GetPiece(int row, int col)
    {
        return IsValidPosition(row, col) ? board[row, col] : null;
    }

    public bool MovePiece(int fromRow, int fromCol, int toRow, int toCol)
    {
        return pieceMovement.Move(fromRow, fromCol, toRow, toCol);
    }

    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
    }

    public bool IsStalemate()
    {
        if (IsCheck) return false;
        
        for (int fromRow = 0; fromRow < BOARD_SIZE; fromRow++)
        {
            for (int fromCol = 0; fromCol < BOARD_SIZE; fromCol++)
            {
                var piece = board[fromRow, fromCol];
                if (piece?.Color != CurrentTurn) continue;
                
                for (int toRow = 0; toRow < BOARD_SIZE; toRow++)
                {
                    for (int toCol = 0; toCol < BOARD_SIZE; toCol++)
                    {
                        if (moveValidation.IsValidMove(fromRow, fromCol, toRow, toCol, CurrentTurn))
                            return false;
                    }
                }
            }
        }
        return true;
    }

    public void PromotePawn(int row, int col, PieceType newType)
    {
        var piece = board[row, col];
        if (piece?.Type == PieceType.Pawn)
        {
            board[row, col] = new ChessPiece(newType, piece.Color);
        }
    }

    public Point? GetPiecePosition(PieceType type, PieceColor color)
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var piece = board[row, col];
                if (piece?.Type == type && piece.Color == color)
                {
                    return new Point(col, row);
                }
            }
        }
        return null;
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