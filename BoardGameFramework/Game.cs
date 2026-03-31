namespace BoardGameFramework;

public abstract class Game
{
    protected Board board;
    protected Player currentPlayer;
    protected List<Player> players = new List<Player>();
    protected IDisplay display;
    protected HistoryManager historyManager;
    protected GameSaver gameSaver;

    protected Game(IDisplay display, HistoryManager historyManager, GameSaver gameSaver)
    {
        this.display = display;
        this.historyManager = historyManager;
        this.gameSaver = gameSaver;
    }
    public void PlayGame()
    {
        // Template method
        InitialiseGame();
        int currentIndex = 0;
        while (!EndOfGame())
        {
            MakePlay(currentIndex);
            currentIndex = (currentIndex + 1) % players.Count;
        }
        PrintWinner();
    }
    protected abstract void InitialiseGame();
    protected abstract bool EndOfGame();
    protected abstract void MakePlay(int player);
    protected abstract void PrintWinner();
}