using Avalonia.Data.Converters;
using Avalonia.Styling;
using DynamicData;
using ReactiveUI;
using SerialComApp.Models;
using SerialComApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SerialComApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _sendMessageField = string.Empty;
        private string _receivedMessageLog = string.Empty;

        private string _comPortName = string.Empty;
        private string _baudRate = string.Empty;
        private string _selectedDataBitsItem = string.Empty;
        private string _selectedParityItem = string.Empty;
        private string _selectedStopBitsItem = string.Empty;
        private string _selectedFlowControlItem = string.Empty;
        private string _selectedEncodingItem = string.Empty;

        private int _scrollPosition;
        private bool _autoscrollToggle = false;
        private bool _isConnected = false;
        private string _connectDisconnectCommandString = string.Empty;

        private IFileService _fileService;

        private SerialPortHandler? _comport;
        private AppConfig _appConfig;


        public ReactiveCommand<Unit, Unit> ConnectDisconnectCommand { get; }
        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveConfigCommand { get; }
        public ReactiveCommand<Unit, Unit> SetDefaultsCommand { get; }
        public ReactiveCommand<Unit, Task> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> AutoscrollCommand { get; }


        public ObservableCollection<string> DataBitsItems { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ParityItems { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> StopBitsItems { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> FlowControlItems { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> EncodingItems { get; } = new ObservableCollection<string>();

        #pragma warning disable CS8618 // Design time constructor for Avalonia preview
        public MainWindowViewModel() { }
        #pragma warning restore CS8618

        public MainWindowViewModel(IFileService fileService)
        {
            _fileService = fileService;
            ConnectDisconnectCommandString = "";

            // var isValidObservable = this.WhenAnyValue(
            //     x => x.ComPortName,
            //     x => x.BaudRate,
            //     x => x.IsConnected,
            //     (comPort, baudRate, isConnected) => !string.IsNullOrWhiteSpace(comPort) && int.TryParse(baudRate, out _) && !isConnected);

            var isValidConnectOrDisconnect = this.WhenAnyValue(
                x => x.ComPortName,
                x => x.BaudRate,
                (comPort, baudRate) => !string.IsNullOrWhiteSpace(comPort) && int.TryParse(baudRate, out _));

            var isConnectedObservable = this.WhenAnyValue(
                x => x.IsConnected);

            var isNotRunningObservable = this.WhenAnyValue(
                x => x.IsConnected,
                x => !x);


            this
                .WhenAnyValue(
                    x => x.IsConnected)
                .Subscribe(_ =>
                {
                    ConnectDisconnectCommandString = GetConnectDisconnectCommandString();
                });

            ConnectDisconnectCommand = ReactiveCommand.Create(ConnectDisconnect, isValidConnectOrDisconnect);
            //ConnectCommand = ReactiveCommand.Create(Connect, isValidObservable);
            DisconnectCommand = ReactiveCommand.Create(Disconnect, isConnectedObservable);
            SaveConfigCommand = ReactiveCommand.Create(LoadConfig, isValidConnectOrDisconnect);
            SetDefaultsCommand = ReactiveCommand.Create(SetDefaults, isNotRunningObservable);
            SendMessageCommand = ReactiveCommand.Create(SendMessage, isConnectedObservable);
            AutoscrollCommand = ReactiveCommand.Create(Autoscroll);

            DataBitsItems.AddRange(new List<string> { "5", "6", "7", "8" });
            ParityItems.AddRange(new List<string> { "None", "Odd", "Even", "Mark", "Space" });
            StopBitsItems.AddRange(new List<string> { "0", "1", "2", "1.5" });
            FlowControlItems.AddRange(new List<string> { "None", "XOnXOff", "RequestToSend", "RequestToSendXOnXOff" });
            EncodingItems.AddRange(new List<string> { "Hex", "ASCII", "Uint 8" });

            _appConfig = new AppConfig();
            SetFromConfig();

        }

        private void ConnectDisconnect()
        {
            if (IsConnected) { Disconnect(); }
            else { Connect(); }
        }

        private string GetConnectDisconnectCommandString()
        {
            if (this.IsConnected)
            {
                return "Disconnect";
            }
            else
            {
                return "Connect";
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            switch (SelectedEncodingItem)
            {
                case "Hex":
                    byte[] hexBuffer = new byte[sp.BytesToRead];
                    sp.Read(hexBuffer, 0, hexBuffer.Length);
                    string hexData = BitConverter.ToString(hexBuffer).Replace("-", " ");
                    this.ReceivedMessageLog += DateTime.Now.ToString() + " > " + hexData + "\n";
                    break;
                case "ASCII":
                    string asciiData = sp.ReadLine();
                    this.ReceivedMessageLog += DateTime.Now.ToString() + " > " + asciiData + "\n";
                    break;
                case "Uint 8":
                    byte[] byteBuffer = new byte[sp.BytesToRead];
                    sp.Read(byteBuffer, 0, byteBuffer.Length);
                    foreach (byte value in byteBuffer)
                    {
                        uint uint8Data = value;
                        this.ReceivedMessageLog += DateTime.Now.ToString() + " > " + uint8Data + "\n";
                    }
                    break;
            }
        }

        public string ComPortName
        {
            get { return _comPortName; }
            set
            {
                this.RaiseAndSetIfChanged(ref _comPortName, value);
                _appConfig.ComPortName = value;
            }
        }

        public string BaudRate
        {
            get { return _baudRate; }
            set
            {
                this.RaiseAndSetIfChanged(ref _baudRate, value);
                _appConfig.BaudRate = value;
            }
        }

        public string SelectedDataBitsItem
        {
            get { return _selectedDataBitsItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedDataBitsItem, value);
                _appConfig.SelectedDataBitsItem = value;
            }
        }

        public string SelectedParityItem
        {
            get { return _selectedParityItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedParityItem, value);
                _appConfig.SelectedParityItem = value;
            }
        }

        public string SelectedStopBitsItem
        {
            get { return _selectedStopBitsItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedStopBitsItem, value);
                _appConfig.SelectedStopBitsItem = value;
            }
        }

        public string SelectedFlowControlItem
        {
            get { return _selectedFlowControlItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFlowControlItem, value);
                _appConfig.SelectedFlowControlItem = value;
            }
        }

        public string SelectedEncodingItem
        {
            get { return _selectedEncodingItem; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedEncodingItem, value);
                _appConfig.SelectedEncodingItem = value;
            }
        }

        public string SendMessageField
        {
            get => _sendMessageField;
            set => this.RaiseAndSetIfChanged(ref _sendMessageField, value);
        }

        public string ReceivedMessageLog
        {
            get => _receivedMessageLog;
            set => this.RaiseAndSetIfChanged(ref _receivedMessageLog, value);
        }

        public bool AutoscrollToggle
        {
            get => _autoscrollToggle;
            set => this.RaiseAndSetIfChanged(ref _autoscrollToggle, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        public int ScrollPosition
        {
            get => _scrollPosition;
            set => this.RaiseAndSetIfChanged(ref _scrollPosition, value);
        }

        public string ConnectDisconnectCommandString
        {
            get => _connectDisconnectCommandString;
            set => this.RaiseAndSetIfChanged(ref _connectDisconnectCommandString, value);
        }

        private void Connect()
        {
            int baudR;
            int.TryParse(BaudRate, out baudR);

            int dataBits;
            int.TryParse(SelectedDataBitsItem, out dataBits);

            int parityIndex = ParityItems.IndexOf(SelectedParityItem);
            Parity selectedParity = EnumFromIndex(parityIndex, ParityItems, Parity.None);

            int stopBitsIndex = StopBitsItems.IndexOf(SelectedStopBitsItem);
            StopBits selectedStopBits = EnumFromIndex(stopBitsIndex, StopBitsItems, StopBits.One);

            int handshakeIndex = FlowControlItems.IndexOf(SelectedFlowControlItem);
            Handshake selectedHandshake = EnumFromIndex(handshakeIndex, FlowControlItems, Handshake.None);


            _comport = new SerialPortHandler(ComPortName, baudR, dataBits, selectedParity, selectedStopBits, selectedHandshake);
            _comport.GetSerialPortObject().DataReceived += DataReceivedHandler;
            IsConnected = true;
        }

        private T EnumFromIndex<T>(int index, IList<string> items, T defaultValue)
        {
            if (index >= 0 && index < items.Count)
            {
                if (Enum.TryParse(typeof(T), items[index], out var value) && value is T enumValue)
                {
                    return enumValue;
                }
            }
            return defaultValue;
        }

        private void SetDefaults()
        {
            _appConfig = new AppConfig();
            SetFromConfig();
        }

        private void SetFromConfig()
        {
            ComPortName = _appConfig.ComPortName;
            BaudRate = _appConfig.BaudRate;
            SelectedDataBitsItem = _appConfig.SelectedDataBitsItem;
            SelectedParityItem = _appConfig.SelectedParityItem;
            SelectedStopBitsItem = _appConfig.SelectedStopBitsItem;
            SelectedFlowControlItem = _appConfig.SelectedFlowControlItem;
            SelectedEncodingItem = _appConfig.SelectedEncodingItem;
        }

        private async void SaveConfig()
        {
            if (_fileService is null) { throw new NullReferenceException("File service instance not set."); }

            var file = await _fileService.SaveFileAsync();
            if (file != null)
            {
                var uriPath = file.Path;
                var chosenFilePath = uriPath.LocalPath;
                _appConfig.SerializeSettings(chosenFilePath);
            }
        }

        private async void LoadConfig()
        {
            if (_fileService is null) { throw new NullReferenceException("File service instance not set."); }

            var file = await _fileService.OpenFileAsync();
            if (file != null)
            {
                var uriPath = file.Path;
                var chosenFilePath = uriPath.LocalPath;
                _appConfig.DeserializeSettings(chosenFilePath);
                SetFromConfig();
            }
        }

        private void Disconnect()
        {
            if (_comport is null) return;
            _comport.CloseSerialPort();
            IsConnected = false;
            ReceivedMessageLog = string.Empty;
        }

        private async Task SendMessage()
        {
            if (_comport is null) return;
            await _comport.SendDataAsync(SendMessageField + "\n");
            SendMessageField = string.Empty;
        }

        private void Autoscroll()
        {
            _autoscrollToggle = !_autoscrollToggle;
        }
    }
}