using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerSudoku.Logic
{
    public static class Sudoku
    {
        private static HashSet<int> AllowedSymbols { get; } = new HashSet<int>(Enumerable.Range(1, 9));

        public static int[,] GenerateBoard()
        {
            var board = new int[9, 9];
            var seedline = AllowedSymbols.Shuffle(new Random());
            for (int rowIndex = 0; rowIndex < board.GetLength(0); rowIndex++)
            {
                var boxLine = rowIndex / 3;
                var shiftTo = boxLine + (rowIndex % 3 * 3);
                for (int colIndex = 0; colIndex < board.GetLength(1); colIndex++)
                {
                    var shiftedIndex = (colIndex + shiftTo) % 9;
                    board[rowIndex, colIndex] = seedline[shiftedIndex];
                }
            }
            return board;
        }

        public static int?[,] Puzzle(int[,] board, int maxRowMissingElements)
        {
            var rnd = new Random();
            var puzzled = new int?[board.GetLength(0), board.GetLength(1)];
            for (int row = 0; row < board.GetLength(0); row++)
            {
                var hideCount = rnd.Next(0, maxRowMissingElements);
                var hiddenElements = board.GetRow(row)
                    .ToArray()
                    .Shuffle(rnd)
                    .Take(hideCount);
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    var boardElement = board[row, col];
                    puzzled[row, col] = hiddenElements.Contains(boardElement)
                        ? (int?)null
                        : boardElement;
                }
            }
            return puzzled;
        }

        public static bool TryUpdateElement(int?[,] board, int row, int col, int value)
        {
            if (!AllowedSymbols.Contains(value))
                return false;
            var valueRestricted = board.GetSquareElements(row, col)
                .Concat(board.GetRow(row))
                .Concat(board.GetColumn(col))
                .Contains(value);

            if (!valueRestricted)
            {
                board[row, col] = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsCorrect(int?[,] nullableBoard)
        {
            if (nullableBoard.TryToNotNullable(out var board))
            {
                return IsCorrect(board);
            }
            else
            {
                return false;
            }
        }

        public static bool IsCorrect(int[,] board)
        {
            for (int rowIndex = 0; rowIndex < board.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < board.GetLength(1); colIndex++)
                {
                    var element = board[rowIndex, colIndex];
                    if (!AllowedSymbols.Contains(element))
                        return false;
                    var oneInSquare = board.GetSquareElements(rowIndex, colIndex).IsUnique(element);
                    var oneInRow = board.GetRow(rowIndex).IsUnique(element);
                    var oneInColumn = board.GetColumn(colIndex).IsUnique(element);

                    if (!oneInSquare || !oneInRow || !oneInColumn)
                        return false;
                }
            }
            return true;
        }

        private static IEnumerable<T> GetSquareElements<T>(this T[,] board, int rowIndex, int colIndex)
        {
            var squareRow = (rowIndex) / 3 * 3;
            var squareCol = (colIndex) / 3 * 3;
            for (int row = squareRow; row < squareRow + 3; row++)
            {
                for (int col = squareCol; col < squareCol + 3; col++)
                {
                    yield return board[row, col];
                }
            }
        }

        private static bool IsUnique<T>(this IEnumerable<T> seq, T element)
        {
            return seq
                .Where(el => el.Equals(element))
                .Count() == 1;
        }
    }
}
