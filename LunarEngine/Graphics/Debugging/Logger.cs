using Serilog;

namespace LunarEngine.Graphics.Debugging;

public static class Logger
{
    public static void InitializeLogger()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo
            .File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();
    }

    public static async Task Shutdown()
    {
        await Log.CloseAndFlushAsync();
    }
}