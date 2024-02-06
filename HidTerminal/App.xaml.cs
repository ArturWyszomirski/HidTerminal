namespace HidTerminal;

public partial class App : Application
{
    public App(IFileLogService log)
    {
        InitializeComponent();

        MainPage = new AppShell();

        log.CreateFile();
        log.AppendLine("App started");
    }
}
