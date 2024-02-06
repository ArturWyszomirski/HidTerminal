namespace HidTerminal.ViewModels;

public partial class DeviceWatcherViewModel: ViewModelBase
{
    private readonly IHidUsbService _hidUsbService;

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

    [RelayCommand]
    void ScanDevices() => _hidUsbService.StartDeviceWatcher(VendorId, ProductId, UsagePage, UsageId);
}
