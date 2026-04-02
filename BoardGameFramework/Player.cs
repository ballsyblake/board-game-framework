namespace BoardGameFramework;

public abstract class Player {
    // Abstract class from which HumanPlayer and ComputerPlayer will inherit
    public int PlayerNumber { get; set; }
    public string GamePiece { get; set; }
    protected Player(int playerNumber, string gamePiece) {
        PlayerNumber = playerNumber;
        GamePiece = gamePiece;
    }
    // Need an abstract method called makeMove so that all inherited classes are forced to implement it
    // Each player type (Human, Computer) implements its own logic
    public abstract (int row, int col, string value) MakeMove(Board board, IDisplay display);
}