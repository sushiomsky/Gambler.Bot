using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using Velopack;

namespace Gambler.Bot.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // It's important to Run() the VelopackApp as early as possible in app startup.
            VelopackApp.Build()
                .Run();
            var serilogLogger = new LoggerConfiguration()
   .Enrich.FromLogContext()
#if DEBUG
   .MinimumLevel.Debug()
   #else
   .MinimumLevel.Warning()
#endif
   .WriteTo.File("gamblerbotlog.log") // Serilog.Sinks.Debug
   .CreateLogger();
            Log.Logger = serilogLogger;
            Log.Logger.Information("App starting");
            // Now it's time to run Avalonia
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        }
        catch (Exception ex)
        {
            string message = "Unhandled exception: " + ex.ToString();
            Console.WriteLine(message);
            throw;
        }

    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var config = new ConfigurationBuilder()
           .AddUserSecrets<Program>()           
           .Build();
        
        var builder =  AppBuilder.Configure<App>()                
              .UsePlatformDetect()
#if DEBUG
              .LogToTrace(Avalonia.Logging.LogEventLevel.Debug)
#else
              .LogToTrace(Avalonia.Logging.LogEventLevel.Warning)
#endif
              .WithInterFont()
              .LogToTrace()
              .UseReactiveUI()
              ;
        try
        {
            builder.RegisterActiproLicense(config.GetValue<string>("ActiproLicense:Licensee"), config.GetValue<string>("ActiproLicense:LisenceKey"));
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error registering Actipro license");
        }
        return builder;
    }
}
