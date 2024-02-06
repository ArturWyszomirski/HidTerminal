namespace HidTerminal.Models;

internal class HidTerminalSettingsModel : AppSettingsModel
{
    new public string AppName = "HidTester"; // this is temporary solution as Assembly.GetCallingAssembly().GetName().Name does not return name as it did before
}
