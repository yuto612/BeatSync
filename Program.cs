using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;

namespace BGMSyncVisualizer;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine("=== BPM Sync Visualizer Application Starting ===");
        Console.WriteLine($"Program.Main: Args = [{string.Join(", ", args)}]");
        
        // 全ての未処理例外を補足するグローバルハンドラを設定
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            HandleException(e.ExceptionObject as Exception);
        };

        try
        {
            Console.WriteLine("Program.Main: Building Avalonia app...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            throw;
        }
        
        Console.WriteLine("Program.Main: Application finished");
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        Console.WriteLine("Program.BuildAvaloniaApp: Configuring Avalonia...");
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }

    public static void HandleException(Exception? ex)
    {
        if (ex == null) return;
        var logPath = Path.Combine(AppContext.BaseDirectory, "crash_log.txt");
        var logMessage = @$"
=================================================
CRASH REPORT - {DateTime.Now}
=================================================

{ex}
";
        try
        {
            File.AppendAllText(logPath, logMessage);
        }
        catch
        {
            // Failsafe: if logging to file fails, do nothing.
        }
    }
}