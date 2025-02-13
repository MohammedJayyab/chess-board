namespace ChessGame.Forms;

using ChessGame.Models;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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
    private readonly Color selectedSquareColor = Color.FromArgb(130, 151, 105);
    private readonly Color checkSquareColor = Color.FromArgb(220, 80, 80);  // Red tint for check

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
        
        // Subscribe to events ONCE
        board.MoveExecuted += OnMoveExecuted;
        board.PawnPromotionRequired += OnPawnPromotionRequired;
        
        squares = new Button[8, 8];
        boardPanel = boardContainer;
        InitializeChessBoard();

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
            // If clicking the same square, deselect it
            if (selectedSquare.Value.X == col && selectedSquare.Value.Y == row)
            {
                ResetSquareColor(new Point(col, 7-row)); // Reset just this square
                selectedSquare = null;
                return;
            }

            int fromRow = selectedSquare.Value.Y;
            int fromCol = selectedSquare.Value.X;

            if (board.MovePiece(fromRow, fromCol, row, col))
            {
                UpdateBoardDisplay();
                ResetSquareColors(); // Reset all squares after a move
            }
            else
            {
                ResetSquareColor(new Point(fromCol, 7-fromRow)); // Reset previous selection if move invalid
            }
            selectedSquare = null;
        }
        else if (board.GetPiece(row, col)?.Color == board.CurrentTurn)
        {
            selectedSquare = new Point(col, row);
            square.BackColor = selectedSquareColor;
        }
    }

    private void UpdateBoardDisplay()
    {
        Debug.WriteLine($"Current turn: {board.CurrentTurn}, IsCheck: {board.IsCheck}");

        // First reset all squares to their base colors
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var square = squares[row, col];
                bool isDarkSquare = ((row + col) % 2 != 0);
                square.BackColor = isDarkSquare ? darkSquareColor : lightSquareColor;

                var piece = board.GetPiece(7 - row, col);
                square.BackgroundImage = piece?.PieceImage;
                square.BackgroundImageLayout = ImageLayout.Center;
            }
        }

        // Apply highlights in order of priority (from lowest to highest)
        
        // 1. Last move highlights
        if (board.LastMoveFrom.HasValue && board.LastMoveTo.HasValue)
        {
            var from = board.LastMoveFrom.Value;
            var to = board.LastMoveTo.Value;
            squares[7 - from.Y, from.X].BackColor = lastMoveColor;
            squares[7 - to.Y, to.X].BackColor = lastMoveColor;
        }

        // 2. Selected square highlight
        if (selectedSquare.HasValue)
        {
            squares[7 - selectedSquare.Value.Y, selectedSquare.Value.X].BackColor = selectedSquareColor;
        }

        // 3. Check highlight (highest priority - should override other highlights)
        if (board.IsCheck && board.CheckedKing.HasValue)
        {
            var kingPos = board.GetPiecePosition(PieceType.King, board.CheckedKing.Value);
            if (kingPos.HasValue)
            {
                var square = squares[7 - kingPos.Value.Y, kingPos.Value.X];
                square.BackColor = checkSquareColor;
                Debug.WriteLine($"Highlighting checked king at row:{kingPos.Value.Y}, col:{kingPos.Value.X}");
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
                bool isDarkSquare = ((row + col) % 2 != 0);
                Color baseColor = isDarkSquare ? darkSquareColor : lightSquareColor;

                // Keep check highlight if king is in check
                if (board.IsCheck && board.CheckedKing.HasValue)
                {
                    var kingPos = board.GetPiecePosition(PieceType.King, board.CheckedKing.Value);
                    if (kingPos.HasValue && row == (7 - kingPos.Value.Y) && col == kingPos.Value.X)
                    {
                        square.BackColor = checkSquareColor;
                        continue;
                    }
                }

                if (IsLastMoveSquare(7 - row, col))
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

    private void OnMoveExecuted(object? sender, MoveEventArgs e)
    {
        UpdateBoardDisplay();
        UpdateMoveList(e);
    }

    private void OnPawnPromotionRequired(object? sender, PawnPromotionEventArgs e)
    {
        var piece = board.GetPiece(e.Row, e.Col);
        if (piece == null) return;

        using var promotionForm = new PromotionForm(piece.Color);
        if (promotionForm.ShowDialog() == DialogResult.OK)
        {
            board.PromotePawn(e.Row, e.Col, promotionForm.SelectedPieceType);
            UpdateBoardDisplay();
        }
    }

    private void UpdateMoveList(MoveEventArgs e)
    {
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
        string moveText;

        // Pawn moves
        if (piece.Type == PieceType.Pawn)
        {
            if (wasCapture)
            {
                char fromFile = (char)('a' + fromCol);
                moveText = $"{fromFile}x{toFile}{toRank}";  // Capture: "exd5"
            }
            else
            {
                moveText = $"{toFile}{toRank}";  // Simple: "e4"
            }
        }
        else
        {
            string pieceSymbol = piece.Type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                _ => ""
            };

            moveText = wasCapture
                ? $"{pieceSymbol}x{toFile}{toRank}"  // Capture: "Nxe5"
                : $"{pieceSymbol}{toFile}{toRank}";  // Simple: "Nf3"
        }

        // Add check symbol if the opponent's king is in check
        // We need to check if the OPPONENT's king is in check, not the current player's
        PieceColor opponentColor = piece.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        if (board.CheckedKing == opponentColor)
        {
            moveText += "+";
        }

        return moveText;
    }

    private void NewGame()
    {
        if (MessageBox.Show("Start a new game?", "New Game", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            moveList.Items.Clear();
            
            // Unsubscribe old events
            if (board != null)
            {
                board.MoveExecuted -= OnMoveExecuted;
                board.PawnPromotionRequired -= OnPawnPromotionRequired;
            }
            
            board = new ChessBoard();
            
            // Subscribe new events
            board.MoveExecuted += OnMoveExecuted;
            board.PawnPromotionRequired += OnPawnPromotionRequired;
            
            selectedSquare = null;
            UpdateBoardDisplay();
            ResetSquareColors();
        }
    }

    private void ResetSquareColor(Point square)
    {
        var button = squares[square.Y, square.X];
        bool isDarkSquare = ((square.Y + square.X) % 2 != 0);
        
        // Check if this square is the king's square and king is in check
        if (board.IsCheck && board.CheckedKing.HasValue)
        {
            var kingPos = board.GetPiecePosition(PieceType.King, board.CheckedKing.Value);
            if (kingPos.HasValue && square.Y == (7 - kingPos.Value.Y) && square.X == kingPos.Value.X)
            {
                button.BackColor = checkSquareColor;
                return;
            }
        }

        if (IsLastMoveSquare(7 - square.Y, square.X))
        {
            button.BackColor = lastMoveColor;
        }
        else
        {
            button.BackColor = isDarkSquare ? darkSquareColor : lightSquareColor;
        }
    }

    private void UpdateKingInCheckHighlight()
    {
        // Clear previous highlights
        ClearHighlights();

        if (board.CheckedKing.HasValue)
        {
            var kingPos = board.GetPiecePosition(PieceType.King, board.CheckedKing.Value);
            if (kingPos.HasValue)
            {
                squares[7 - kingPos.Value.Y, kingPos.Value.X].BackColor = checkSquareColor;
            }
        }
    }

    private void HighlightLastMove()
    {
        var lastMoveFrom = board.LastMoveFrom;
        var lastMoveTo = board.LastMoveTo;

        if (lastMoveFrom.HasValue)
        {
            squares[7 - lastMoveFrom.Value.Y, lastMoveFrom.Value.X].BackColor = lastMoveColor;
        }
        if (lastMoveTo.HasValue)
        {
            squares[7 - lastMoveTo.Value.Y, lastMoveTo.Value.X].BackColor = lastMoveColor;
        }
    }

    private void ClearHighlights()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                squares[row, col].BackColor = squares[row, col].BackColor == checkSquareColor ? lightSquareColor : squares[row, col].BackColor;
            }
        }
    }
} 