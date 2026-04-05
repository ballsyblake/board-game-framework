namespace BoardGameFramework;

public class NumericalTicTacToeGame : Game
{
    private NumericalTicTacToeBoard _nttBoard;

    // Tracks who won so PrintWinner knows what to say.
    private Player? _winner;

    public NumericalTicTacToeGame(IDisplay display,
                                   HistoryManager historyManager,
                                   GameSaver gameSaver)
        : base(display, historyManager, gameSaver)
    {
        // board field is assigned in InitialiseGame; satisfy the compiler with a
        // temporary placeholder until then.
        _nttBoard = new NumericalTicTacToeBoard();
        _board = _nttBoard;
    }

    protected override void InitialiseGame()
    {
        _nttBoard = new NumericalTicTacToeBoard();
        _board = _nttBoard;
        _winner = null;
        _historyManager.Clear();

        // GamePiece is used here purely as a label shown in messages. The actual pieces placed are the numbers chosen each turn.
        _players.Clear();
        _players.Add(new HumanPlayer(1, "Odd"));
        _players.Add(new HumanPlayer(2, "Even"));
        _currentPlayer = _players[0];

        _display.ShowMessage("=== Numerical Tic-Tac-Toe ===");
        _display.ShowHelp(
            "Player 1 places ODD numbers (1,3,5,7,9). " +
            "Player 2 places EVEN numbers (2,4,6,8). " +
            "First to make a row/col/diagonal sum to 15 wins!\n" +
            "Commands: enter row col value (rows/cols are 1-3)  |  undo  |  redo  |  save <path>");
    }

    protected override void MakePlay(int playerIndex)
    {
        _currentPlayer = _players[playerIndex];
        bool isOddPlayer = _currentPlayer.PlayerNumber == 1;

        // Show the current board state and whose turn it is.
        _display.ShowBoard(_board);
        var available = _nttBoard.GetAvailableNumbers(isOddPlayer);
        _display.ShowMessage(
            $"\nPlayer {_currentPlayer.PlayerNumber} ({_currentPlayer.GamePiece}) — " +
            $"available numbers: [{string.Join(", ", available)}]");

        // Handle meta-commands first, then delegate move input to the player.
        while (true)
        {
            string metaInput = _display.GetInput("Command (undo/redo/save <path>) or press enter to move: ").Trim();

            if (metaInput.Equals("undo", StringComparison.OrdinalIgnoreCase))
            {
                UndoMove();
                // SwitchPlayer inside UndoMove cancels the index bump PlayGame
                // would otherwise apply, so returning here replays this turn.
                return;
            }

            if (metaInput.Equals("redo", StringComparison.OrdinalIgnoreCase))
            {
                RedoMove();
                return;
            }

            if (metaInput.StartsWith("save ", StringComparison.OrdinalIgnoreCase))
            {
                string path = metaInput.Substring(5).Trim();
                SaveGame(path);
                continue; // Stay on the same player's turn after saving
            }

            if (metaInput.Length > 0)
            {
                _display.ShowMessage("Unknown command. Use undo, redo, save <path>, or press enter to make a move.");
                continue;
            }

            var (row, col, value) = _currentPlayer.MakeMove(_board, _display);

            if (!int.TryParse(value, out int number))
            {
                _display.ShowMessage("Value must be a whole number between 1 and 9.");
                continue;
            }

            try
            {
                if (!_nttBoard.IsValidNTTMove(row, col, number, isOddPlayer))
                {
                    if (number < 1 || number > 9)
                        _display.ShowMessage("Number must be between 1 and 9.");
                    else if (number % 2 == 0 && isOddPlayer)
                        _display.ShowMessage("Player 1 must place ODD numbers.");
                    else if (number % 2 != 0 && !isOddPlayer)
                        _display.ShowMessage("Player 2 must place EVEN numbers.");
                    else
                        _display.ShowMessage("That number has already been placed. Choose a different number.");
                    continue;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _display.ShowMessage($"Invalid position: {ex.Message}");
                continue;
            }

            var command = new MoveCommand(_nttBoard, row, col, number.ToString());
            _historyManager.Execute(command);
            return;
        }
    }

    protected override bool EndOfGame()
    {
        if (_nttBoard.CheckWin())
        {
            _winner = _currentPlayer;
            return true;
        }

        if (_nttBoard.IsFull())
        {
            _winner = null; // Draw
            return true;
        }

        return false;
    }

    protected override void PrintWinner()
    {
        _display.ShowBoard(_board);
        if (_winner != null)
            _display.ShowResult(
                $"Player {_winner.PlayerNumber} ({_winner.GamePiece}) wins!");
        else
            _display.ShowResult("It's a draw! No line sums to 15.");
    }

    public void RestoreFromSaveData(SaveData data)
    {
        _nttBoard = new NumericalTicTacToeBoard();
        _board = _nttBoard;
        _winner = null;

        // Restore grid
        if (data.Grid != null)
        {
            for (int r = 0; r < data.Grid.Count; r++)
                for (int c = 0; c < data.Grid[r].Count; c++)
                {
                    string? cell = data.Grid[r][c];
                    if (cell != null)
                        _nttBoard.PlaceMove(r, c, cell);
                }
        }

        // Restore players
        _players.Clear();
        foreach (var pd in data.Players)
        {
            Player p = pd.IsHuman ? new HumanPlayer(pd.PlayerNumber, pd.GamePiece) : (Player)new HumanPlayer(pd.PlayerNumber, pd.GamePiece); // extend for ComputerPlayer
            _players.Add(p);
        }

        _currentPlayer = _players.Count > data.CurrentPlayerIndex ? _players[data.CurrentPlayerIndex] : _players[0];
    }

    public override SaveData ToSaveData()
    {
        var gridCopy = new List<List<string?>>();
        for (int r = 0; r < _nttBoard.Rows; r++)
        {
            var row = new List<string?>();
            for (int c = 0; c < _nttBoard.Cols; c++)
                row.Add(_nttBoard.GetCell(r, c));
            gridCopy.Add(row);
        }

        return new SaveData
        {
            GameType = "ntt",
            Rows = _nttBoard.Rows,
            Cols = _nttBoard.Cols,
            Grid = gridCopy,
            CurrentPlayerIndex = _players.IndexOf(_currentPlayer),
            Players = _players.Select(p => new PlayerData
            {
                PlayerNumber = p.PlayerNumber,
                GamePiece = p.GamePiece,
                IsHuman = p is HumanPlayer
            }).ToList()
        };
    }
}
