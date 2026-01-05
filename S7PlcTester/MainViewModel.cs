using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using S7PlcTester.Enums;
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
        private string _slot = "0";

        [ObservableProperty]
        private string _rack = "0";

        [ObservableProperty]
        private bool _isConnected;

        [ObservableProperty]
        private string _connectState = "☠️ 연결 안됨 ☠️";

        partial void OnIsConnectedChanged(bool value)
        {
            ConnectState = value ? "💡 연결 됨 💡" : "☠️ 연결 안됨 ☠️";
        }

        [RelayCommand]
        private async Task Connect()
        {
            _plcComm = new S7Communicator(Ip, Rack, Slot);
            IsConnected = await _plcComm.ConnectAsync();
        }

        [RelayCommand]
        private async Task Disconnect()
        {
            if (_plcComm != null)
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

        public ObservableCollection<PlcOperationViewModel> Operations { get; } = [];

        [RelayCommand]
        private void AddOperation()
        {
            if (Operations.Count == 0)
            {
                Operations.Add(new PlcOperationViewModel());
            }
            else
            {
                var last = Operations[^1];
                Operations.Add(new PlcOperationViewModel
                {
                    OperationType = last.OperationType,
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
        private void RemoveOperation(PlcOperationViewModel? operation)
        {
            if (operation == null)
            {
                return;
            }

            Operations.Remove(operation);
        }

        [RelayCommand]
        private async Task ExecuteOperation(PlcOperationViewModel? operation)
        {
            if (operation == null || _plcComm == null || !IsConnected)
            {
                return;
            }

            try
            {
                switch (operation.OperationType)
                {
                    case OperationType.Read:
                        object? value = await _plcComm.ReadPlcAsync(
                            operation.EndianType,
                            operation.PlcDataType,
                            operation.Block,
                            operation.Base,
                            operation.Index,
                            operation.Size);

                        if (value != null)
                        {
                            operation.ReadValue = value.ToString() ?? string.Empty;
                        }
                        else
                        {
                            operation.ReadValue = string.Empty;
                        }

                        break;
                    case OperationType.Write:
                        bool success = await _plcComm.WritePlcAsync(
                            operation.EndianType,
                            operation.PlcDataType,
                            operation.Block,
                            operation.Base,
                            operation.Index,
                            operation.WriteValue,
                            operation.Size);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
