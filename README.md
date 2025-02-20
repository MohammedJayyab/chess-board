# Chess Game

A feature-rich chess implementation in C# using Windows Forms, following standard chess rules and conventions.

## Features

- Complete chess rule implementation
  - All standard piece movements
  - Special moves (castling, en passant, pawn promotion)
  - Check and checkmate detection
  - Legal move validation
- Modern user interface
  - Clean, intuitive board design
  - Piece highlighting for selected pieces
  - Move history with standard algebraic notation
  - Last move highlighting
  - Check position highlighting
- Game state management
  - New game option
  - Move tracking
  - Turn management

## Technical Details

### Architecture
The project follows SOLID principles and clean architecture:
- **Models**: Core chess logic (pieces, board, move validation)
- **Forms**: UI components and user interaction
- **Events**: Communication between components

### Key Components
- `ChessBoard`: Central game state manager
- `MoveValidation`: Ensures moves follow chess rules
- `CheckDetection`: Handles check and checkmate logic
- `PieceMovement`: Manages piece movement and special moves
- `GameState`: Tracks game progress and turn management

### Visual Features
- SVG-based piece graphics
- Color-coded squares and highlights
- Move history panel
- Intuitive piece selection and movement

## Chess Rules Implementation

- Standard piece movement rules
- Complete special move support:
  - Castling (kingside and queenside)
  - En passant captures
  - Pawn promotion
- Check and checkmate detection
- Move validation including:
  - Piece-specific movement patterns
  - Path obstruction checking
  - Check prevention/escape validation

## User Interface

- Standard 8x8 chess board
- Rank (1-8) and file (a-h) labels
- Move history panel showing algebraic notation
- Visual feedback:
  - Selected piece highlighting
  - Last move highlighting
  - Check position marking
  - Legal move indication

## Requirements

- .NET 6.0 or higher
- Windows OS (Windows Forms application)
- SVG support (via Svg.NET package)

## Future Enhancements

- Checkmate detection and game end handling
- Draw conditions (stalemate, insufficient material, etc.)
- Move take-back functionality
- PGN export/import
- Game analysis tools
- AI opponent implementation

## Contributing

Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

## License

[Add appropriate license information]
