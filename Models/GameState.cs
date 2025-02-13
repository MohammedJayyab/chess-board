namespace ChessGame.Models;

public class GameState
{
    private const int BOARD_SIZE = 8;
    public PieceColor CurrentTurn { get; private set; }
    public bool IsCheck { get; set; }
    public PieceColor? CheckedKing { get; set; }
    public Point? LastPawnDoubleMove { get; private set; }
    public int HalfMoveClock { get; private set; }
    public int FullMoveNumber { get; private set; }

    public GameState()
    {
        CurrentTurn = PieceColor.White;
        HalfMoveClock = 0;
        FullMoveNumber = 1;
    }

    public void SwitchTurns()
    {
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }

    public void UpdateGameState(Move move)
    {
        if (move.Piece.Type == PieceType.Pawn || move.IsCapture)
            HalfMoveClock = 0;
        else
            HalfMoveClock++;

        if (CurrentTurn == PieceColor.Black)
            FullMoveNumber++;

        if (move.Piece.Type == PieceType.Pawn && Math.Abs(move.To.Y - move.From.Y) == 2)
            LastPawnDoubleMove = move.To;
        else
            LastPawnDoubleMove = null;
    }

    // Add methods to update and check game state
} 