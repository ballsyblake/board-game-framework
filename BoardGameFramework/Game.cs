namespace BoardGameFramework;

public abstract class Game {
    protected Board _board;
    protected Player _currentPlayer;
    protected List<Player> _players = new List<Player>();
    protected IDisplay _display;
    protected HistoryManager _historyManager;
    protected GameSaver _gameSaver;

    protected Game(IDisplay display, HistoryManager historyManager, GameSaver gameSaver)
    {
        _display = display;
        _historyManager = historyManager;
        _gameSaver = gameSaver;
    }
    public void PlayGame()
    {
        InitialiseGame();
        int currentIndex = 0;
        while (!EndOfGame())
        {
            MakePlay(currentIndex);
            currentIndex = (currentIndex + 1) % _players.Count;
        }
        PrintWinner();
    }
    protected abstract void InitialiseGame();
    protected abstract bool EndOfGame();
    protected abstract void MakePlay(int player);
    protected abstract void PrintWinner();
    protected void SwitchPlayer()
    {
        _currentPlayer = _players[(_players.IndexOf(_currentPlayer) + 1) % _players.Count];
    }
    public void UndoMove()
    {
        //First check if undo is possible, then call historyManager to undo the last move
        if (_historyManager.CanUndo())
        {
            _historyManager.Undo();
            SwitchPlayer(); //Return back to the previous player
        } else
        {
            _display.ShowMessage("No moves to undo.");
        }
    }
    public void RedoMove()
    {
        //First check if redo is possible, then call historyManager to redo the last move
        if (_historyManager.CanRedo())
        {
            _historyManager.Redo();
            SwitchPlayer(); //Go forward to the next player
        } else
        {
            _display.ShowMessage("No moves to redo.");
        }
    }
    public void SaveGame(string filePath)
    {
        //tells gameSaver to save the current game state to a file located in filePath. filepath can be pretty much anyting at the moment.
        _gameSaver.SaveGame(this, filePath);
        _display.ShowMessage("Your game has been saved successfully.");
    }
    public void LoadGame(string filePath)
    {
        //gets from gameSaver. Restores board, players, history.
        _gameSaver.LoadGame(filePath);
        _display.ShowMessage("Your game has been loaded successfully.");
    }

    public virtual SaveData ToSaveData()
    {
        throw new NotImplementedException("ToSaveData() must be overridden in game classes");
    }
}