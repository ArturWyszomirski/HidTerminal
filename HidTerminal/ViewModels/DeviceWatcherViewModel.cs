namespace HidTerminal.ViewModels;

public partial class DeviceWatcherViewModel: ViewModelBase
{
    readonly IAlertService _alert;
    readonly IHidUsbService _hidUsbService;

    readonly byte[] _frame = new byte[64];

    public DeviceWatcherViewModel(IAlertService alert, IHidUsbService hidUsbService)
    {
        _alert = alert;
        _hidUsbService = hidUsbService;
        Devices = hidUsbService.HidDevices;

        Devices.CollectionChanged += (s, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                SelectedDevice ??= Devices.First(); // figure why UI is not updated

            if (e.Action == NotifyCollectionChangedAction.Remove)
                if (SelectedDevice is not null)
                    if (!Devices.Contains(SelectedDevice))
                        SelectedDevice = null;
        };

        hidUsbService.FrameReceived += (s, e) => MainThread.BeginInvokeOnMainThread(() =>
        {
            if (e.HidDevice == SelectedDevice)
            {
                Output += $"\n\n{DateTime.Now}";
                Output += "\nReceived frame:\n";
                foreach (byte b in e.ReceivedFrame)
                    Output += b.ToString() + " ";
                Output += "\n";
            }
        });
    }

    [ObservableProperty]
    ushort _vendorId = 0x04D8;

    [ObservableProperty]
    ushort _productId = 0x003F;

    [ObservableProperty]
    ushort _usagePage = 0xFF00;

    [ObservableProperty]
    ushort _usageId = 0x0001;

    [ObservableProperty]
    ObservableCollection<IHidDeviceModel> _devices;

    [ObservableProperty]
    IHidDeviceModel? _selectedDevice;

    [ObservableProperty]
    ushort? _reportId = 0;

    [ObservableProperty]
    string? _data;

    [ObservableProperty]
    string? _output;

    partial void OnDataChanged(string? value)
    {
        string[]? values = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i  = 0; i < values?.Length; i++)
        { 
            if (byte.TryParse(values[i], out byte valueAsByte))
                _frame[i] = valueAsByte;
            else
                throw new ArgumentException("Error trying parsing to byte", value);
        }
    }

    [RelayCommand]
    void StartDeviceWatcher() => _hidUsbService.StartDeviceWatcher(VendorId, ProductId, UsagePage, UsageId);

    [RelayCommand]
    async Task SendFrameAsync()
    {
        Output += $"\n\n---\n{DateTime.Now}";
        Output += "\nSend frame request:\n";
        Output += $"{ReportId} ";
        foreach (byte b in _frame)
            Output += b.ToString() + " ";
        Output += "\n";

        bool result = await _hidUsbService.SendFrameAsync(SelectedDevice, ReportId, _frame);
            if (result)
                Output += $"\n{DateTime.Now}\nSend frame successful!\n";
            else
            {
                Output += $"\n{DateTime.Now}\nSend frame successful!\n";
                await _alert.DisplayAlertAsync("Error", "Sending frame unsuccessful", "Ok");
            }
    }
}
