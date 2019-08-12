using System;

namespace MultiplayerSudoku.Logic
{
    public class SudokuBoard
    {
        private readonly int?[,] _board;

        public SudokuBoard(int rowMissingElements)
        {
            if (rowMissingElements < 1)
                throw new ArgumentOutOfRangeException(nameof(rowMissingElements));

            _board = Sudoku.Puzzle(Sudoku.GenerateBoard(), rowMissingElements);
            UpdateStatus();
        }

        public GameStatus Status { get; private set; }

        public int?[,] CurrentState => _board.Copy();

        public bool TryUpdateAt(int row, int column, int value)
        {
            if (Sudoku.TryUpdateElement(_board, row, column, value))
            {
                UpdateStatus();
                return true;
            }
            else return false;
        }

        private void UpdateStatus()
        {
            if (Sudoku.IsCorrect(_board))
            {
                Status = GameStatus.Correct;
            }
            else if (Sudoku.HasInputVariants(_board))
            {
                Status = GameStatus.InProgress;
            }
            else
            {
                Status = GameStatus.Broken;
            }
        }

        public enum GameStatus
        {
            InProgress,
            Broken,
            Correct
        }
    }
}
