namespace ChessGame;

using ChessGame.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ChessForm());
    }
} 