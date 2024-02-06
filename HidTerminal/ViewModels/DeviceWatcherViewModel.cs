namespace HidTerminal.ViewModels;

public partial class DeviceWatcherViewModel: ViewModelBase
{
    readonly IHidUsbService _hidUsbService;
    readonly byte[] _frame = new byte[64];

    public DeviceWatcherViewModel(IHidUsbService hidUsbService)
    {
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
    string? _data;

    partial void OnDataChanged(string? value)
    {
        string[] values = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i  = 0; i < values.Length; i++)
        { 
            if (byte.TryParse(values[i], out byte valueAsByte))
                _frame[i] = valueAsByte;
            else
                throw new ArgumentException("Error trying parsing to byte", value);
        }
    }

    [RelayCommand]
    void ScanDevices() => _hidUsbService.StartDeviceWatcher(VendorId, ProductId, UsagePage, UsageId);

    [RelayCommand]
    async Task SendFrameAsync() => await _hidUsbService.SendFrameAsync(SelectedDevice, _frame);
}
