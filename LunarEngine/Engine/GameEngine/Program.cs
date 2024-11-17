namespace LunarEngine.GameEngine;
// Graphics Engine:
// Camera, Window
public class Program
{
    static void Main(string[] args)
    {
        GameEngine engine = GameEngine.CreateGameEngine();
        engine.StartEngine();
    }
}