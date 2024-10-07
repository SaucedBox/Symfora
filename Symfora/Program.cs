public class Program {
    public static void Main(string[] args)
    {
        using var game = new Symfora.Core(args);
        game.Run();
    }
}