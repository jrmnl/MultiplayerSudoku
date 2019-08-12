using System;

namespace MultiplayerSudoku.Application.Contract.Messages
{
    public abstract class UserMessage
    {
        protected UserMessage(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Type { get; } // discriminator value

        public class CurrentState : UserMessage
        {
            public CurrentState(int?[,] sudokuBoard) : base("state")
            {
                SudokuBoard = sudokuBoard ?? throw new ArgumentNullException(nameof(sudokuBoard));
            }

            public int?[,] SudokuBoard { get; }
        }

        public class NameConflict : UserMessage
        {
            public NameConflict(string name) : base("nameconflict")
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
            }

            public string Name { get; }
        }

        public class GameEnd : UserMessage
        {
            public GameEnd(string winner) : base("end")
            {
                Winner = winner;
            }

            public string Winner { get; }
        }
    }
}
