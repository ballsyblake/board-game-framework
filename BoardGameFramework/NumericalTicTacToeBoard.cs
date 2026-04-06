using BoardGameFramework.Core;

namespace BoardGameFramework.Games;

// The board for Numerical Tic-Tac-Toe. Extends Board with NTT-specific rules:
// each number can only be placed once across the whole game, and players are
// restricted to odd or even numbers depending on which player they are.
public class NumericalTicTacToeBoard : Board
{
    // Tracks which numbers have already been placed so duplicates are rejected
    private readonly HashSet<int> _usedNumbers = new HashSet<int>();

    public NumericalTicTacToeBoard() : base(3, 3) { }

    // Returns only the numbers from the given player's pool that haven't been placed yet.
    // Yielding one at a time avoids building an unnecessary intermediate list.
    public IEnumerable<int> GetAvailableNumbers(bool oddOnly)
    {
        var pool = oddOnly
            ? new[] { 1, 3, 5, 7, 9 }
            : new[] { 2, 4, 6, 8 };

        foreach (int n in pool)
            if (!_usedNumbers.Contains(n))
                yield return n;
    }

    // Checks all three conditions a valid NTT move must satisfy:
    // the cell must be empty, the number must not have been used before,
    // and the number must belong to the current player's odd/even pool.
    public bool IsValidNTTMove(int row, int col, int number, bool isOddPlayer)
    {
        if (!IsValidMove(row, col)) return false;
        if (_usedNumbers.Contains(number)) return false;
        bool numberIsOdd = number % 2 != 0;
        return numberIsOdd == isOddPlayer;
    }

    // Overrides PlaceMove to also register the number as used.
    // This ensures _usedNumbers stays in sync whether the move comes from
    // a MoveCommand, RestoreFromSaveData, or anywhere else.
    public override void PlaceMove(int row, int col, string value)
    {
        base.PlaceMove(row, col, value);
        if (int.TryParse(value, out int number))
            _usedNumbers.Add(number);
    }

    // Overrides RevertMove to also remove the number from the used set,
    // keeping _usedNumbers consistent when a move is undone
    public override void RevertMove(int row, int col)
    {
        string? cell = GetCell(row, col);
        if (cell != null && int.TryParse(cell, out int number))
            _usedNumbers.Remove(number);
        base.RevertMove(row, col);
    }

    // Convenience method for restoring board state from a save file using a typed int rather than a string
    public void PlaceNTTMove(int row, int col, int number)
    {
        PlaceMove(row, col, number.ToString());
    }

    // Checks every row, column, and diagonal to see if any line of three cells sums to 15.
    // A line that contains an empty cell will never sum to 15 so no null checks are needed in the logic.
    public override bool CheckWin()
    {
        for (int r = 0; r < Rows; r++)
            if (LineSum(r, 0, 0, 1) == 15) return true;

        for (int c = 0; c < Cols; c++)
            if (LineSum(0, c, 1, 0) == 15) return true;

        // Main diagonal (top-left → bottom-right)
        if (LineSum(0, 0, 1, 1) == 15) return true;

        // Anti-diagonal (top-right → bottom-left)
        if (LineSum(0, 2, 1, -1) == 15) return true;

        return false;
    }

    // Returns true when every cell has been filled, signalling a draw if no winner has been found
    public bool IsFull()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (IsCellEmpty(r, c)) return false;
        return true;
    }

    // Walks three cells in the direction given by (dRow, dCol) and returns their sum.
    // Cells that are empty or non-numeric contribute 0, so partial lines never falsely trigger a win.
    private int LineSum(int startRow, int startCol, int dRow, int dCol)
    {
        int sum = 0;
        int r = startRow, c = startCol;
        for (int i = 0; i < 3; i++)
        {
            string? cell = GetCell(r, c);
            if (cell != null && int.TryParse(cell, out int val))
                sum += val;
            r += dRow;
            c += dCol;
        }
        return sum;
    }
}
