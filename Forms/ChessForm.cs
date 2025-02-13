namespace ChessGame.Forms;

using ChessGame.Models;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Linq;

public partial class ChessForm : Form
{
    private ChessBoard board;
    private readonly Button[,] squares;
    private Point? selectedSquare;
    private readonly MenuStrip menuStrip;
    private readonly ListBox moveList;
    private readonly Panel boardPanel;
    private readonly int squareSize = 80; // Fixed square size

    // Colors
    private readonly Color lightSquareColor = Color.FromArgb(240, 217, 181);
    private readonly Color darkSquareColor = Color.FromArgb(181, 136, 99);
    private readonly Color highlightColor = Color.FromArgb(130, 151, 105);
    private readonly Color lastMoveColor = Color.FromArgb(205, 210, 106);
    private readonly Color checkColor = Color.FromArgb(231, 72, 86);

    public ChessForm()
    {
        InitializeComponent();
        
        // Form setup
        this.Size = new Size(1200, 900);
        this.MinimumSize = new Size(1200, 900);
        this.Text = "Chess Game";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(32, 32, 32);

        // Create menu
        menuStrip = new MenuStrip
        {
            BackColor = Color.FromArgb(45, 45, 45),
            ForeColor = Color.White
        };
        var gameMenu = new ToolStripMenuItem("Game");
        var newGame = new ToolStripMenuItem("New Game", null, (s, e) => NewGame());
        var exit = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        gameMenu.DropDownItems.AddRange(new ToolStripItem[] { newGame, exit });
        menuStrip.Items.Add(gameMenu);
        this.Controls.Add(menuStrip);

        // Create TableLayoutPanel for main layout
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(32, 32, 32),
            Padding = new Padding(20)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, squareSize * 8 + 40)); // Board width + padding
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.Controls.Add(mainLayout);

        // Create board container
        var boardContainer = new Panel
        {
            Width = squareSize * 8,
            Height = squareSize * 8,
            BackColor = Color.FromArgb(32, 32, 32),
            Margin = new Padding(10),
            Dock = DockStyle.None,
            AutoSize = false
        };
        mainLayout.Controls.Add(boardContainer, 0, 0);

        // Create move list container
        var moveListContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 45),
            Padding = new Padding(10),
            Margin = new Padding(10)
        };
        mainLayout.Controls.Add(moveListContainer, 1, 0);

        // Add "Move History" label
        var historyLabel = new Label
        {
            Text = "Move History",
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 30
        };
        moveListContainer.Controls.Add(historyLabel);

        // Create move list with explicit size and location
        moveList = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(32, 32, 32),
            ForeColor = Color.White,
            Font = new Font("Consolas", 12F),
            BorderStyle = BorderStyle.None,
            Visible = true,  // Ensure visibility
            Enabled = true   // Ensure enabled
        };
        
        // Ensure the container is properly set up
        moveListContainer.Controls.Clear();  // Clear any existing controls
        moveListContainer.Visible = true;    // Ensure container is visible
        moveListContainer.Controls.Add(moveList);
        
        // Initialize board
        board = new ChessBoard();
        board.MoveExecuted += OnMoveExecuted;
        board.PawnPromotionRequired += HandlePawnPromotion;
        
        squares = new Button[8, 8];
        boardPanel = boardContainer;
        InitializeChessBoard();

        // Add debug logging
        Debug.WriteLine("ChessForm initialized");
    }

    private void InitializeChessBoard()
    {
        // Add rank labels (8-1 from top to bottom)
        for (int row = 0; row < 8; row++)
        {
            var rankLabel = new Label
            {
                Text = (8 - row).ToString(),
                Size = new Size(20, squareSize),
                Location = new Point(0, row * squareSize + 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            boardPanel.Controls.Add(rankLabel);
        }

        // Add file labels (a-h from left to right)
        for (int col = 0; col < 8; col++)
        {
            var fileLabel = new Label
            {
                Text = ((char)('a' + col)).ToString(),
                Size = new Size(squareSize, 20),
                Location = new Point(20 + col * squareSize, 8 * squareSize + 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            boardPanel.Controls.Add(fileLabel);
        }

        // Create board squares
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // Fix square colors: a1 (bottom-left) must be dark
                bool isDarkSquare = ((row + col) % 2 != 0);  // Changed the condition
                var square = new Button
                {
                    Size = new Size(squareSize, squareSize),
                    Location = new Point(20 + col * squareSize, 20 + row * squareSize),
                    BackColor = isDarkSquare ? darkSquareColor : lightSquareColor,
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 }
                };
                square.Click += Square_Click;
                squares[row, col] = square;
                boardPanel.Controls.Add(square);
            }
        }

        boardPanel.Size = new Size(squareSize * 8 + 40, squareSize * 8 + 40);
        UpdateBoardDisplay();
    }

    private void Square_Click(object? sender, EventArgs e)
    {
        if (sender == null) return;
        var square = (Button)sender;

        // Calculate board coordinates
        int col = (square.Location.X - 20) / squareSize;
        int row = 7 - ((square.Location.Y - 20) / squareSize);  // Convert screen Y to chess rank

        if (col < 0 || col > 7 || row < 0 || row > 7)
            return;

        if (selectedSquare.HasValue)
        {
            int fromRow = selectedSquare.Value.Y;
            int fromCol = selectedSquare.Value.X;

            if (board.MovePiece(fromRow, fromCol, row, col))
            {
                UpdateBoardDisplay();
                ResetSquareColors();
            }
            selectedSquare = null;
        }
        else if (board.GetPiece(row, col)?.Color == board.CurrentTurn)
        {
            selectedSquare = new Point(col, row);
            square.BackColor = highlightColor;
        }
    }

    private void UpdateBoardDisplay()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var square = squares[row, col];  // Removed the flip
                var piece = board.GetPiece(7 - row, col);  // Flip the board coordinates instead
                
                if (piece?.PieceImage != null)
                {
                    square.BackgroundImage = new Bitmap(piece.PieceImage, new Size(squareSize, squareSize));
                    square.BackgroundImageLayout = ImageLayout.Center;
                }
                else
                {
                    square.BackgroundImage = null;
                }
            }
        }
    }

    private bool IsLastMoveSquare(int row, int col)
    {
        if (!board.LastMoveFrom.HasValue || !board.LastMoveTo.HasValue)
            return false;

        var from = board.LastMoveFrom.Value;
        var to = board.LastMoveTo.Value;

        return (row == from.Y && col == from.X) || (row == to.Y && col == to.X);
    }

    private void ResetSquareColors()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var square = squares[row, col];
                bool isDarkSquare = ((row + col) % 2 != 0);  // Same pattern as initialization
                Color baseColor = isDarkSquare ? darkSquareColor : lightSquareColor;

                if (IsLastMoveSquare(7 - row, col))  // Flip coordinates for last move check
                {
                    square.BackColor = lastMoveColor;
                }
                else
                {
                    square.BackColor = baseColor;
                }
            }
        }
    }

    private void HandlePawnPromotion(object? sender, PawnPromotionEventArgs e)
    {
        var piece = board.GetPiece(e.Row, e.Col);
        if (piece == null) return;

        using var promotionForm = new PromotionForm(piece.Color);
        if (promotionForm.ShowDialog(this) == DialogResult.OK)
        {
            board.PromotePawn(e.Row, e.Col, promotionForm.SelectedPieceType);
            UpdateBoardDisplay();
        }
    }

    private void OnMoveExecuted(object? sender, MoveEventArgs e)
    {
        Debug.WriteLine($"OnMoveExecuted called: {e.FromRow},{e.FromCol} to {e.ToRow},{e.ToCol}");
        
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnMoveExecuted(sender, e)));
            return;
        }

        string moveText = GetSimpleAlgebraicNotation(e.FromRow, e.FromCol, e.ToRow, e.ToCol, e.Piece, e.WasCapture);
        Debug.WriteLine($"Move to record: {moveText}");

        try
        {
            if (e.Piece.Color == PieceColor.White)
            {
                int moveNumber = moveList.Items.Count + 1;
                string newMove = $"{moveNumber}. {moveText}";
                Debug.WriteLine($"Adding move: {newMove}");
                moveList.Items.Add(newMove);
            }
            else
            {
                if (moveList.Items.Count > 0)
                {
                    int lastIndex = moveList.Items.Count - 1;
                    string currentLine = moveList.Items[lastIndex].ToString()!;
                    moveList.Items[lastIndex] = $"{currentLine} {moveText}";
                }
            }
            moveList.SelectedIndex = moveList.Items.Count - 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error recording move: {ex}");
            throw;
        }
    }

    private string GetSimpleAlgebraicNotation(int fromRow, int fromCol, int toRow, int toCol, ChessPiece piece, bool wasCapture)
    {
        char toFile = (char)('a' + toCol);
        int toRank = toRow + 1;

        // Pawn moves
        if (piece.Type == PieceType.Pawn)
        {
            if (wasCapture)  // Use the passed capture information
            {
                char fromFile = (char)('a' + fromCol);
                return $"{fromFile}x{toFile}{toRank}";  // Capture: "exd5"
            }
            return $"{toFile}{toRank}";  // Simple: "e4"
        }

        string pieceSymbol = piece.Type switch
        {
            PieceType.King => "K",
            PieceType.Queen => "Q",
            PieceType.Rook => "R",
            PieceType.Bishop => "B",
            PieceType.Knight => "N",
            _ => ""
        };

        return wasCapture
            ? $"{pieceSymbol}x{toFile}{toRank}"  // Capture: "Nxe5"
            : $"{pieceSymbol}{toFile}{toRank}";  // Simple: "Nf3"
    }

    private bool IsCapture(int toRow, int toCol)
    {
        return board.GetPiece(toRow, toCol) != null;
    }

    private int GetRow(ChessPiece piece)
    {
        var pos = board.GetPiecePosition(piece);
        return pos?.Item1 ?? -1;
    }

    private int GetCol(ChessPiece piece)
    {
        var pos = board.GetPiecePosition(piece);
        return pos?.Item2 ?? -1;
    }

    private void NewGame()
    {
        if (MessageBox.Show("Start a new game?", "New Game", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            // Clear move list first
            moveList.Items.Clear();

            // Create new board
            board = new ChessBoard();

            // Attach event handlers
            board.MoveExecuted += OnMoveExecuted;
            board.PawnPromotionRequired += HandlePawnPromotion;

            // Reset UI
            selectedSquare = null;
            UpdateBoardDisplay();
            ResetSquareColors();
        }
    }

    private bool IsKingSquare(int row, int col, PieceColor color)
    {
        var piece = board.GetPiece(row, col);
        return piece?.Type == PieceType.King && piece.Color == color;
    }
} 