using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerSudoku.Logic
{
    internal static class Extensions
    {
        public static IEnumerable<T> GetRow<T>(this T[,] matrix, int rowIndex)
        {
            return Enumerable
                .Range(0, matrix.GetLength(1))
                .Select(x => matrix[rowIndex, x]);
        }

        public static IEnumerable<T> GetColumn<T>(this T[,] matrix, int colIndex)
        {
            return Enumerable
                .Range(0, matrix.GetLength(0))
                .Select(x => matrix[x, colIndex]);
        }

        public static bool TryToNotNullable<T>(this Nullable<T>[,] matrix, out T[,] result) where T : struct
        {
            result = new T[matrix.GetLength(0), matrix.GetLength(1)];
            for (int row = 0; row < result.GetLength(0); row++)
            {
                for (int col = 0; col < result.GetLength(1); col++)
                {
                    var element = matrix[row, col];
                    if(element.HasValue)
                    {
                        result[row, col] = element.Value;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static T[] Shuffle<T>(this IReadOnlyCollection<T> list, Random rnd)
        {
            var shuffled = list.ToArray();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }
            return shuffled;
        }
    }
}
