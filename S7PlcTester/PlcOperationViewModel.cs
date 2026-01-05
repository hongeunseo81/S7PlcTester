using CommunityToolkit.Mvvm.ComponentModel;
using S7PlcTester.Enums;

namespace S7PlcTester.ViewModels
{
    public partial class PlcOperationViewModel : ObservableObject
    {
        public PlcOperationViewModel()
        {
            OperationType = OperationType.Read;
            PlcDataType = PlcDataType.Bool;
            EndianType = EndianType.Little;
        }

        public string OperationTypeGroupName { get; } = $"OperationType_{Guid.NewGuid():N}";
        public string EndianGroupName { get; } = $"Endian_{Guid.NewGuid():N}";

        [ObservableProperty]
        private OperationType _operationType;

        [ObservableProperty]
        private PlcDataType _plcDataType;

        [ObservableProperty]
        private EndianType _endianType;

        [ObservableProperty]
        private int _block;

        [ObservableProperty]
        private int _base;

        [ObservableProperty]
        private int _index;

        [ObservableProperty]
        private int _size;

        [ObservableProperty]
        private string _writeValue = string.Empty;

        [ObservableProperty]
        private string _readValue = string.Empty;
    }
}
