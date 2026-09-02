using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace VishalXOpt.App;

public partial class App : Application
{
    private static readonly string LogFile =
        Path.Combine(
            AppContext.BaseDirectory,
            "VishalXOpt-startup.log");

    public App()
    {
        InitializeComponent();

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;
        TaskScheduler.UnobservedTaskException += App_UnobservedTaskException;

        Startup += App_Startup;
        Exit += App_Exit;
    }

    private void App_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            WriteLog("========================================");
            WriteLog("Vishal X Opt starting");
            WriteLog($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            WriteLog($"BaseDirectory: {AppContext.BaseDirectory}");
            WriteLog($"OS: {Environment.OSVersion}");
            WriteLog($".NET Runtime: {Environment.Version}");
            WriteLog($"64-bit OS: {Environment.Is64BitOperatingSystem}");
            WriteLog($"64-bit Process: {Environment.Is64BitProcess}");
            WriteLog("Startup event completed.");

            if (MainWindow != null)
            {
                WriteLog(
                    $"MainWindow created: {MainWindow.GetType().FullName}");
            }
            else
            {
                WriteLog("WARNING: MainWindow is still null after startup.");
            }
        }
        catch (Exception ex)
        {
            WriteException("Startup handler failed", ex);
        }
    }

    private void App_Exit(object sender, ExitEventArgs e)
    {
        WriteLog(
            $"Application exiting. ExitCode={e.ApplicationExitCode}");
    }

    private void App_DispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        WriteException(
            "Unhandled WPF dispatcher exception",
            e.Exception);

        try
        {
            MessageBox.Show(
                "Vishal X Opt encountered an error.\n\n" +
                e.Exception.Message +
                "\n\nDetailed log:\n" +
                LogFile,
                "Vishal X Opt - Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch
        {
        }

        e.Handled = true;
    }

    private static void App_UnhandledException(
        object sender,
        UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            WriteException(
                "Unhandled AppDomain exception",
                ex);
        }
        else
        {
            WriteLog(
                $"Unhandled AppDomain exception: {e.ExceptionObject}");
        }
    }

    private static void App_UnobservedTaskException(
        object? sender,
        UnobservedTaskExceptionEventArgs e)
    {
        WriteException(
            "Unobserved task exception",
            e.Exception);

        e.SetObserved();
    }

    private static void WriteLog(string message)
    {
        try
        {
            File.AppendAllText(
                LogFile,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
                $"{message}{Environment.NewLine}");
        }
        catch
        {
        }
    }

    private static void WriteException(
        string context,
        Exception exception)
    {
        WriteLog(context);
        WriteLog(exception.ToString());
    }
}
