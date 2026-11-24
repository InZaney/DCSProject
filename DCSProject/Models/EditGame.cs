namespace DCSProject.Models;

public class EditGame : AddGame
{
    /// <summary>
    /// Which game is being edited. Sent to the form in a hidden field
    /// </summary>
    public int GameId { get; set; }
}
