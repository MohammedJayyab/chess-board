namespace ChessGame.Forms;

using ChessGame.Models;
using System.Drawing;
using System.Windows.Forms;

public class PromotionForm : Form
{
    public PieceType SelectedPieceType { get; private set; }
    private readonly PieceColor promotionColor;
    private readonly Button[] pieceButtons;

    public PromotionForm(PieceColor color)
    {
        promotionColor = color;
        SelectedPieceType = PieceType.Queen; // Default
        pieceButtons = new Button[4];

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "Promote Pawn";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        
        // Increase window size to accommodate pieces and window borders
        this.ClientSize = new Size(280, 80);  // Changed from Size to ClientSize
        
        var pieces = new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        // Center the buttons vertically
        int buttonY = (ClientSize.Height - 64) / 2;

        for (int i = 0; i < pieces.Length; i++)
        {
            var piece = new ChessPiece(pieces[i], promotionColor);
            var button = new Button
            {
                Size = new Size(64, 64),
                Location = new Point(i * 70 + 5, buttonY),  // Adjusted spacing and vertical position
                BackgroundImage = piece.PieceImage ?? null,
                BackgroundImageLayout = ImageLayout.Center,
                Tag = pieces[i],
                FlatStyle = FlatStyle.Flat,  // Make buttons look better
                FlatAppearance = { BorderSize = 1 }  // Thin border
            };

            // Highlight button on hover
            button.MouseEnter += (s, e) => { if (s is Button b) b.FlatAppearance.BorderSize = 2; };
            button.MouseLeave += (s, e) => { if (s is Button b) b.FlatAppearance.BorderSize = 1; };

            button.Click += (s, e) =>
            {
                if (s is Button clickedButton)
                {
                    SelectedPieceType = (PieceType)clickedButton.Tag;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };

            pieceButtons[i] = button;
            Controls.Add(button);
        }
    }
} 