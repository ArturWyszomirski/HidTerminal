namespace HidTerminal.ViewModels;

public partial class DeviceWatcherViewModel: ViewModelBase
{
    readonly IAlertService _alert;
    readonly IHidUsbService _hidUsbService;

    byte[] _frame = new byte[64];

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
                //string receivedFrame = BitConverter.ToString(e.ReceivedFrame).Replace("-", " ");
                //Output += $"\n\n{DateTime.Now}\nReceived frame:\n{receivedFrame}\n";
                Output += $"\n\n{DateTime.Now}";
                Output += "\nReceived frame:\n";

                string receivedFrame = String.Empty;
                foreach (byte b in e.ReceivedFrame)
                    receivedFrame += b.ToString() + " ";

                Output += $"{receivedFrame}\n";

                int point = e.ReceivedFrame[4];
                float frequency = BitConverter.ToSingle(e.ReceivedFrame, 5);
                float resistance = BitConverter.ToSingle(e.ReceivedFrame, 9);
                float reactance = BitConverter.ToSingle(e.ReceivedFrame, 13);
                float resistanceCalibration = BitConverter.ToSingle(e.ReceivedFrame, 17);
                float reactanceCalibration = BitConverter.ToSingle(e.ReceivedFrame, 21);
                if (e.ReceivedFrame[1] == 32 || e.ReceivedFrame[1] == 33)
                    Output += $"\nPoint: {point}. Freq: {frequency} Hz. " +
                    $"Resistance: {resistance} Ohm. " +
                    $"Reactance: {reactance} Ohm. " +
                    $"Resistance calibration: {resistanceCalibration} Ohm." +
                    $"Resistance calibration: {reactanceCalibration} Ohm.";
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
    string? _data;// = "33 4 0 0 112 65 0 0 0 0 20 0 0 0 19 0 0 64 28 70 0 0 32 65"; // log, P2P = 15 mv, DC = 0, 20 points, speed = 19, 10000 - 10 Hz

    [ObservableProperty]
    string? _output;

    partial void OnDataChanged(string? value)
    {
        string[]? values = value?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        byte[] frame = new byte[64];
        for (int i  = 0; i < values?.Length; i++)
        { 
            if (byte.TryParse(values[i], out byte valueAsByte))
                frame[i] = valueAsByte;
            else
                throw new ArgumentException("Error trying parsing to byte", value);
        }

        _frame = frame;
    }

    [RelayCommand]
    void ScanDevices() => _hidUsbService.StartDeviceWatcher(VendorId, ProductId, UsagePage, UsageId);

    [RelayCommand]
    async Task SendFrameAsync()
    {
        Output += $"\n\n---\n{DateTime.Now}";
        Output += "\nSend frame request:\n";
        Output += "0 ";
        foreach (byte b in _frame)
            Output += b.ToString() + " ";
        Output += "\n";

        bool result = await _hidUsbService.SendFrameAsync(SelectedDevice, _frame);
            if (result)
                Output += $"\n{DateTime.Now}\nSend frame successful!";
            else
            {
                Output += $"\n{DateTime.Now}\nSend frame successful!";
                await _alert.DisplayAlertAsync("Error", "Sending frame unsuccessful", "Ok");
            }
    }
}
