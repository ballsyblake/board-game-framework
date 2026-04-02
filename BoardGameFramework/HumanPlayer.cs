namespace BoardGameFramework;

public class HumanPlayer : Player
{
    public HumanPlayer(int playerNumber, string gamePiece) : base(playerNumber, gamePiece) {}
    public override (int row, int col, string value) MakeMove(Board board, IDisplay display)
    {
        // Method that gets a move from a Human Player.
        /*
        Show whose turn it is via display.ShowMessage()
        Prompt for row via display.GetInput()
        Prompt for col via display.GetInput()
        Prompt for value via display.GetInput() (the value is what to place — a number for NTT, or the player's Piece for Gomoku/Notakto)
        Validate all inputs are valid integers in range
        Check board.IsValidMove(row, col) before returning
        If invalid, show error via display.ShowError() and loop
        */
    }
}