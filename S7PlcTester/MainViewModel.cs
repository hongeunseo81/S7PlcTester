

using CommunityToolkit.Mvvm.Input;
using S7PlcTester.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace S7PlcTester.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private IPlcCommunicator _plcComm;
        #region ConnectInfo
        private string _ip = "127.0.0.1";
        public string Ip
        {
            get => _ip;
            set
            {
                SetProperty(ref _ip, value);
            }
        }

        private string _port = "102";
        public string Port
        {
            get => _port;
            set
            {
                SetProperty(ref _port, value);
            }
        }

        private string _slot = "0";
        public string Slot
        {
            get => _slot;
            set
            {
                SetProperty(ref _slot, value);
            }
        }

        private string _rack = "0";
        public string Rack
        {
            get => _rack;
            set
            {
                SetProperty(ref _rack, value);
            }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                ConnectState = IsConnected ? "💡 연결 됨 💡" : "☠️ 연결 안됨 ☠️";
            }
        }
        private string _connectState = "☠️ 연결 안됨 ☠️";
        public string ConnectState
        {
            get => _connectState;
            set
            {
                SetProperty(ref _connectState, value);
            }
        }

        [RelayCommand]
        private async Task OnConnect()
        {
            _plcComm = new S7Communicator(_ip, _rack, _slot);
            IsConnected = await _plcComm.ConnectAsync();
        }

        [RelayCommand]
        private async Task OnDisconnect()
        {
            if(_plcComm != null)
            {
                await _plcComm.DisconnectAsync();
            }
            IsConnected = _plcComm.IsConnected;
        }
        #endregion

        #region Read data
        public IEnumerable<PlcDataType> ReadDataTypes { get; }
            = Enum.GetValues(typeof(PlcDataType)).Cast<PlcDataType>();

        private PlcDataType _selectedReadDataType;
        public PlcDataType SelectedReadDataType
        {
            get => _selectedReadDataType;
            set
            {
                SetProperty(ref _selectedReadDataType, value);
            }
        }
        public enum EndianType { Little, Big }

        private EndianType _selectedEndian = EndianType.Little;
        public EndianType SelectedEndian
        {
            get => _selectedEndian;
            set
            {
                _selectedEndian = value;
                SetProperty(ref _selectedEndian, value);
            }
        }

        private int _readBlock;
        public int ReadBlock
        {
            get => _readBlock;
            set
            {
                SetProperty(ref _readBlock, value);
            }
        }

        private int _readIndex;
        public int ReadIndex
        {
            get => _readIndex;
            set
            {
                SetProperty(ref _readIndex, value); 
            }
        }
        private int _readSize;
        public int ReadSize
        {
            get => _readSize;
            set
            {
                SetProperty(ref _readSize, value);
            }
        }

        private string _readValue;
        public string ReadValue
        {
            get => _readValue;
            set
            {
                SetProperty(ref _readValue, value);
            }
        }

        [RelayCommand]
        private async Task OnReadData()
        {
            if (_plcComm != null)
            {
                object? value = await _plcComm.ReadPlcAsync(SelectedEndian,SelectedReadDataType, ReadBlock, ReadIndex,ReadSize);
                if(value != null) 
                {
                    ReadValue = value.ToString();
                }
                else
                {
                    Console.WriteLine("PLC Read Failed");
                }
            }
        }

        #endregion

        #region Write data
        public IEnumerable<PlcDataType> WriteDataTypes { get; }
            = Enum.GetValues(typeof(PlcDataType)).Cast<PlcDataType>();

        private PlcDataType _selectedWriteDataType;
        public PlcDataType SelectedWriteDataType
        {
            get => _selectedWriteDataType;
            set
            {
                SetProperty(ref _selectedWriteDataType, value);
            }
        }
        private int _writeBlock;
        public int WriteBlock
        {
            get => _writeBlock;
            set
            {
                SetProperty(ref _writeBlock, value);
            }
        }
        private int _writeIndex;
        public int WriteIndex
        {
            get => _writeIndex;
            set
            {
                SetProperty(ref _writeIndex, value);
            }
        }
        private int _writeSize;
        public int WriteSize
        {
            get => _writeSize;
            set
            {
                SetProperty(ref _writeSize, value);
            }
        }

        private string _writeValue;
        public string WriteValue
        {
            get => _writeValue;
            set
            {
                SetProperty(ref _writeValue, value);
            }
        }
        private RelayCommand _writeDataCommand;
        public ICommand WriteDataCommand => _writeDataCommand ??= new RelayCommand(() =>
        {
            if (_plcComm != null)
            {
                _plcComm.WritePlcAsync(SelectedEndian, SelectedWriteDataType, WriteBlock, WriteIndex, WriteValue, WriteSize);
            }
        });

        #endregion

        public MainViewModel()
        {

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
