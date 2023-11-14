using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SerialComApp
{
    public class SerialPortHandler
    {

        private string _portName;
        private int _baudRate;
        private int _dataBits;
        private Parity _parity;
        private StopBits _stopBits;
        private SerialPort _serialPort;
        private Handshake _handshake;

        public SerialPortHandler(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits, Handshake handshake)
        {
            _portName = portName;
            _baudRate = baudRate;
            _dataBits = dataBits;
            _parity = parity;
            _stopBits = stopBits;
            _handshake = handshake;

            SerialPort sp = new SerialPort();

            sp.PortName = _portName;
            sp.BaudRate = _baudRate;
            sp.DataBits = _dataBits;
            sp.Parity = _parity;
            sp.StopBits = _stopBits;
            sp.Handshake = _handshake;

            //sp.DataReceived += DataReceivedHandler;
            sp.ErrorReceived += ErrorReceivedHandler;

            _serialPort = sp;
            sp.Open();
            Console.WriteLine("Opened port: {0}", _portName);
        }

        public void CloseSerialPort()
        {
            _serialPort.Close();
            Console.WriteLine("Closed port: {0}", _portName);
        }

        public SerialPort GetSerialPortObject()
        {
            return _serialPort;
        }

        public void SendData(string data)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
            _serialPort.BaseStream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public async Task SendDataAsync(string data)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes(data);
            await _serialPort.BaseStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
            _serialPort.DiscardOutBuffer();
        }

        public async Task<string> ReceiveDataAsync(int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);
            _serialPort.DiscardInBuffer();
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }

        //private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        //{
        //    SerialPort sp = (SerialPort)sender;
        //    string data = sp.ReadLine();
        //}

        private static void ErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e)
        {
            // Handle the error event here
            switch (e.EventType)
            {
                case SerialError.Frame:
                    Console.WriteLine("A framing error occurred.");
                    break;
                case SerialError.Overrun:
                    Console.WriteLine("An overrun error occurred.");
                    break;
                case SerialError.RXOver:
                    Console.WriteLine("An input buffer overflow occurred.");
                    break;
                case SerialError.RXParity:
                    Console.WriteLine("A parity error occurred in the received data.");
                    break;
                case SerialError.TXFull:
                    Console.WriteLine("The transmit buffer is full.");
                    break;
                default:
                    Console.WriteLine("An unknown error occurred.");
                    break;
            }
        }
    }
}
