namespace ChessGame.Models;

public class GameEvents
{
    public event EventHandler<PawnPromotionEventArgs>? PawnPromotionRequired;
    public event EventHandler<MoveEventArgs>? MoveExecuted;

    public void OnPawnPromotion(int row, int col)
    {
        PawnPromotionRequired?.Invoke(this, new PawnPromotionEventArgs(row, col));
    }

    public void OnMoveExecuted(MoveEventArgs args)
    {
        MoveExecuted?.Invoke(this, args);
    }
} 