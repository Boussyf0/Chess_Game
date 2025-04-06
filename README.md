# Chess Game with AI Opponent

A fully-featured chess application built with C# and WPF, featuring an intelligent AI opponent with multiple difficulty levels.

## Features

- **Complete Chess Rules**: Implements all standard chess rules including castling, en passant, and pawn promotion
- **AI Opponent**: Play against an intelligent computer opponent with adjustable difficulty levels
- **Modern UI**: Clean, responsive WPF interface with intuitive controls
- **Game Analytics**: Tracks game statistics and performance metrics

## Technical Highlights

### For Software Engineers
- Clean architecture with separation of concerns (game logic vs UI)
- Object-oriented design with proper inheritance hierarchies for chess pieces
- Efficient move generation and validation algorithms

### For Data Scientists & Data Engineers
- Chess AI implementation using minimax algorithm with alpha-beta pruning
- Position evaluation functions based on piece values, mobility, and board control
- Data structures optimized for chess position representation and analysis

## Project Structure

```
Chess/
├── Chesselogique/          # Core chess logic
│   ├── Board.cs            # Chess board representation
│   ├── GameState.cs        # Game state management
│   ├── ChessAI.cs          # AI opponent implementation
│   ├── Pieces/             # Chess piece implementations
│   └── Moves/              # Move types (normal, castling, etc.)
└── ChesseUI/               # WPF User Interface
    ├── MainWindow.xaml     # Main game window
    ├── GameOverMenu.xaml   # End game UI
    └── Images.cs           # Asset management
```

## Future Enhancements

- Network play capability
- Opening book implementation
- Game analysis tools
- Machine learning-based position evaluation

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Build and run the project

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. 