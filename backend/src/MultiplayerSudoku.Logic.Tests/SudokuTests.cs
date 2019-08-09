using MultiplayerSudoku.Logic;
using Xunit;

namespace MultiplayerSudoku.Tests
{
    public class SudokuTests
    {
        [Fact]
        public void GeneratesCorrectBoard()
        {
            //act
            var board = Sudoku.GenerateBoard();

            //assert
            Assert.True(Sudoku.IsCorrect(board));
        }

        [Fact]
        public void PuzzlesBoard()
        {
            //arrange
            var board = Sudoku.GenerateBoard();

            //act
            var puzzled = Sudoku.Puzzle(board, 8);

            //assert
            Assert.False(Sudoku.IsCorrect(puzzled));
        }

        [Fact]
        public void NotCompleted_IsNotCorrect()
        {
            //arrange
            var board = Sudoku.GenerateBoard();
            var nullableBoard = Sudoku.Puzzle(board, 0);
            nullableBoard[8, 8] = null;

            //act
            var isCorrect = Sudoku.IsCorrect(nullableBoard);

            //assert
            Assert.False(isCorrect);
        }

        [Fact]
        public void IncorrectElement_NotUpdatesBoard()
        {
            //arrange
            var board = Sudoku.GenerateBoard();
            var nullableBoard = Sudoku.Puzzle(board, 0);
            var correctElement = nullableBoard[0, 0];
            nullableBoard[0, 0] = null;

            //act
            var isUpdated = Sudoku.TryUpdateElement(nullableBoard, 0, 0, correctElement.Value + 1);

            //assert
            Assert.False(isUpdated);
            Assert.Null(nullableBoard[0, 0]);
        }

        [Fact]
        public void CorrectElement_UpdatesBoard()
        {
            //arrange
            var board = Sudoku.GenerateBoard();
            var nullableBoard = Sudoku.Puzzle(board, 0);
            var correctElement = nullableBoard[0, 0];
            nullableBoard[0, 0] = null;

            //act
            var isUpdated = Sudoku.TryUpdateElement(nullableBoard, 0, 0, correctElement.Value);

            //assert
            Assert.True(isUpdated);
            Assert.NotNull(nullableBoard[0, 0]);
        }
    }
}
