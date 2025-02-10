using TheTurk.Engine;

var input = Console.ReadLine();
if (input.Contains("xboard"))
{
    var winboard = new Winboard();
    winboard.Start();
}