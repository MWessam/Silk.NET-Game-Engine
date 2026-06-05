using Serilog;

namespace LunarEngine.Core;

public static class Logger
{
    public static void Initialize()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void Shutdown()
    {
        Log.CloseAndFlush();
    }
}
