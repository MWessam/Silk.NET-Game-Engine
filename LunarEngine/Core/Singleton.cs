namespace LunarEngine.GameEngine;

public abstract class Singleton<T> where T : ISingletonObject, IDisposable, new()
{
    protected static T? s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new();
                s_instance.InitSingleton();
            }
            return s_instance;
        }
    }

    public Singleton()
    {
        if (s_instance != null)
        {
            s_instance.Dispose();
            s_instance = new();
            s_instance.InitSingleton();
            return;
        }
    }
}

public interface ISingletonObject
{
    void InitSingleton();
}