namespace LunarEngine.GameEngine;

public abstract class Singleton<T> where T : ISingletonObject, IDisposable, new()
{
    protected static T? s_instance;
    private static bool _isCreating;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new();
            }
            return s_instance;
        }
    }

    protected Singleton()
    {
        if (_isCreating) return;
        if (s_instance != null)
        {
            s_instance.Dispose();
            s_instance = new();
            s_instance.InitSingleton();
            return;
        }
        else
        {
            _isCreating = true;
            s_instance = new();
            s_instance.InitSingleton();
        }
    }
}


public interface ISingletonObject
{
    void InitSingleton();
}