using S7PlcTester.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static S7PlcTester.ViewModels.MainViewModel;

namespace S7PlcTester
{
    public interface IPlcCommunicator
    {
        bool IsConnected { get; }
        bool IsConnecting { get; }
        bool Connect();
        bool Disconnect();
        void Dispose();

        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();
        bool WritePlc(EndianType endianType,PlcDataType dataType, int writeBlockNum, int writeBase, int pos, string value, int size = 0);
        Task<bool> WritePlcAsync(EndianType endianType, PlcDataType dataType, int writeBlockNum, int writeBase, int pos, string value, int size = 0);
        object? ReadPlc(EndianType endianType, PlcDataType dataType, int readBlockNum, int readBase, int pos, int size = 0);
        Task<object?> ReadPlcAsync(EndianType endianType, PlcDataType dataType, int readBlockNum, int readBase, int pos, int size = 0);
    }
}
