using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S7PlcTester.Enums;
using S7PlcTester.Plc;
using System.Collections.ObjectModel;

namespace S7PlcTester.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private IPlcCommunicator? _plcComm;

        #region ConnectInfo
        [ObservableProperty]
        private string _ip = "127.0.0.1";

        [ObservableProperty]
        private string _port = "102";

        [ObservableProperty]
        private string _slot = "1";

        [ObservableProperty]
        private string _rack = "0";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddSignalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExecuteSignalCommand))]
        private bool _isConnected;

        [RelayCommand]
        private async Task Connect()
        {
            _plcComm = new S7Communicator(Ip, Rack, Slot);
            IsConnected = await _plcComm.ConnectAsync();
        }

        [RelayCommand(CanExecute = nameof(IsConnected))]
        private async Task Disconnect()
        {
            if (_plcComm is not null)
            {
                await _plcComm.DisconnectAsync();
                IsConnected = _plcComm.IsConnected;
                return;
            }

            IsConnected = false;
        }
        #endregion

        public IEnumerable<PlcDataType> DataTypes { get; }
            = Enum.GetValues(typeof(PlcDataType)).Cast<PlcDataType>();

        public IEnumerable<EndianType> EndianTypes { get; }
            = Enum.GetValues(typeof(EndianType)).Cast<EndianType>();

        public ObservableCollection<PlcSignalViewModel> Signals { get; } = [];

        [RelayCommand(CanExecute = nameof(IsConnected))]
        private void AddSignal()
        {
            if (Signals.Count == 0)
            {
                Signals.Add(new PlcSignalViewModel());
            }
            else
            {
                var last = Signals[^1];
                Signals.Add(new PlcSignalViewModel
                {
                    SignalType = last.SignalType,
                    PlcDataType = last.PlcDataType,
                    EndianType = last.EndianType,
                    Block = last.Block,
                    Base = last.Base,
                    Index = last.Index,
                    Size = last.Size,
                    WriteValue = last.WriteValue,
                    ReadValue = last.ReadValue
                });
            }
        }

        [RelayCommand]
        private void RemoveSignal(PlcSignalViewModel? signal)
        {
            if (signal is null)
            {
                return;
            }

            Signals.Remove(signal);
        }

        [RelayCommand(CanExecute = nameof(IsConnected))]
        private async Task ExecuteSignal(PlcSignalViewModel? signal)
        {
            if (signal is null || _plcComm is null || !IsConnected)
            {
                return;
            }

            try
            {
                switch (signal.SignalType)
                {
                    case SignalType.Read:
                        object? value = await _plcComm.ReadPlcAsync(
                            signal.EndianType,
                            signal.PlcDataType,
                            signal.Block,
                            signal.Base,
                            signal.Index,
                            signal.Size);

                        signal.Success = value is not null;
                        signal.ReadValue = value?.ToString() ?? string.Empty;
                        break;

                    case SignalType.Write:
                        signal.Success = await _plcComm.WritePlcAsync(
                            signal.EndianType,
                            signal.PlcDataType,
                            signal.Block,
                            signal.Base,
                            signal.Index,
                            signal.WriteValue,
                            signal.Size);
                        break;
                }
            }
            catch (Exception ex)
            {
                signal.Success = false;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
