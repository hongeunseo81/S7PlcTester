using S7PlcTester.Enums;
using Sharp7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace S7PlcTester.Plc
{
    public class S7Communicator : IPlcCommunicator, IDisposable
    {
        private string _ip;
        private int _rack;
        private int _slot;

        private S7Client _client = new();

        private byte[] _readBuf = new byte[128];
        private byte[] _writeBuf = new byte[128];
        private byte[] _writeSignalBuf = new byte[4];

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            private set => _isConnected = value;
        }
        private bool _isConnecting = false;
        public bool IsConnecting
        {
            get => _isConnecting;
            private set => _isConnecting = value;
        }

        public bool Connect()
        {
            if (IsConnected || IsConnecting)
            {
                Console.WriteLine("PLC Already Connected or Connecting");
                return false;
            }
            IsConnecting = true;
            try
            {
                var connection = _client.ConnectTo(_ip, _rack, _slot);
                if(connection == 0)
                {
                    IsConnected = true;
                    Console.WriteLine("PLC Connected");
                }
                else
                {

                    Console.WriteLine($"Connection Failure Reason is : {_client.ErrorText(connection)}");
                    IsConnected = false;
                    Console.WriteLine("PLC Connect Failed");
                }
            }
            catch(Exception ex) 
            {
                IsConnected = false;
                Console.WriteLine($"Connect S7 PLC Failed: {ex.Message}");
            }
            finally
            {
                IsConnecting = false;
            }
            return IsConnected;
        }

        public Task<bool> ConnectAsync()
        {
            return Task.Run(() => Connect());
        }

        public bool Disconnect()
        {
            if (_client.Disconnect() == 0)
            {
                Console.WriteLine("PLC Disconnected");
                IsConnected = false;
            }
            return !IsConnected;
        }

        public Task<bool> DisconnectAsync()
        {
            return Task.Run(() => Disconnect());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object? ReadPlc(EndianType endianType, PlcDataType dataType, int readBlockNum, int readBase, int pos, int size = 0)
        {
            Console.WriteLine($"Read PLC : write DB Num : {readBlockNum} / base: {readBase}  /  pos : {pos} ");
            switch (dataType) 
            {
                case PlcDataType.Bool:
                    // pos is absolute bit address (e.g., pos:9 = byte 1, bit 1)
                    int readByteOffset = pos / 8;
                    int readBitIndex = pos % 8;
                    if (_client.DBRead(readBlockNum, readBase + readByteOffset, 1, _readBuf) != 0)
                    {
                        Console.WriteLine("PLC Read Bit Failed");
                        return null;
                    }
                    return _readBuf.GetBitAt(0, readBitIndex);
                case PlcDataType.Int:
                    if (_client.DBRead(readBlockNum, readBase + pos, 2, _readBuf) != 0)
                    {
                        Console.WriteLine($"PLC Read Int Failed, dbNum: {readBlockNum}, pos:{pos}");
                        return null;
                    }
                    if (endianType == EndianType.Big) 
                    {
                        int low = _readBuf[0];
                        int high = _readBuf[1];
                        return (short)((high << 8) | low);
                    }
                    else 
                    {
                        return _readBuf.GetWordAt(0);
                    }
                case PlcDataType.Float:
                    if (_client.DBRead(readBlockNum, readBase + pos, 4, _readBuf) != 0)
                    {
                        Console.WriteLine("PLC Read Float Failed");
                        return null;
                    }
                    return _readBuf.GetRealAt(0);
                case PlcDataType.String:
                    
                    if (_client.DBRead(readBlockNum, readBase + pos, size * 2, _readBuf) != 0)
                    {
                        Console.WriteLine("PLC Read String Failed");
                        return null;
                    }
                    var result = Encoding.ASCII.GetString(_readBuf, 0, size * 2);
                    return result.TrimEnd('\0');
            }
            return null;
        }

        public Task<object?> ReadPlcAsync(EndianType endianType, PlcDataType dataType, int readBlockNum, int readBase, int pos, int size = 0)
        {
            return Task.Run(() => ReadPlc(endianType,dataType, readBlockNum, readBase, pos, size));
        }

        public bool WritePlc(EndianType endianType, PlcDataType dataType, int writeBlockNum, int writeBase, int pos, string value, int size = 0)
        {
            int ret = -1;
            Console.WriteLine($"WRITE PLC : write DB Num : {writeBlockNum} / base: {writeBase}  /  pos : {pos} / val : {value}");
            switch (dataType)
            {
                case PlcDataType.Bool when bool.TryParse(value, out bool val):
                    // pos is absolute bit address (e.g., pos:9 = byte 1, bit 1)
                    int writeByteOffset = pos / 8;
                    int writeBitIndex = pos % 8;
                    // Read current byte first to preserve other bits
                    if (_client.DBRead(writeBlockNum, writeBase + writeByteOffset, 1, _writeSignalBuf) != 0)
                    {
                        Console.WriteLine("PLC Read before Write Failed");
                        break;
                    }
                    _writeSignalBuf.SetBitAt(0, writeBitIndex, val);
                    ret = _client.DBWrite(writeBlockNum, writeBase + writeByteOffset, 1, _writeSignalBuf);
                    break;
                case PlcDataType.Int when short.TryParse(value, out short val):
                    ushort raw = (ushort)val;

                    if (endianType == EndianType.Big)
                    {
                        raw = (ushort)((raw >> 8) | (raw << 8));
                    }
                    _writeBuf.SetUIntAt(0, raw);
                    ret = _client.DBWrite(writeBlockNum, pos, 2, _writeBuf);
                    break;
                case PlcDataType.Float when float.TryParse(value, out float val):
                    _writeBuf.SetRealAt(0, val);
                    ret = _client.DBWrite(writeBlockNum, writeBase + pos, 4, _writeBuf);
                    break;
                case PlcDataType.String:
                    string text = value;
                    byte[] raws = Encoding.ASCII.GetBytes(text);
                    _writeBuf = new byte[raws.Length];
                    Array.Copy(raws, _writeBuf, raws.Length);

                    ret = _client.DBWrite(writeBlockNum, writeBase + pos, raws.Length, _writeBuf);
                    break;
            }
            if (ret == 0)
            {
                Console.WriteLine("PLC Write Succeed");
                return true;
            }
            else
            {
                Console.WriteLine("PLC Write Failed");
                return false;
            }
        }

        public Task<bool> WritePlcAsync(EndianType endianType, PlcDataType dataType, int writeBlockNum, int writeBase, int pos, string value, int size = 0)
        {

            return Task.Run(() => WritePlc(endianType, dataType, writeBlockNum, writeBase, pos, value, size));
        }


        public S7Communicator(string ip, string rack, string slot)
        {
            _ip = ip;
            _rack = int.TryParse(rack, out var r) ? r : 0;  
            _slot = int.TryParse(slot, out var s) ? s : 0;
            Console.WriteLine($"Connect Info: ip: {_ip}, rack: {_rack}, slot: {_slot}");
        }
    }
}
