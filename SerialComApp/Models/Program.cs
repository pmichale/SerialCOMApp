using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using SerialComApp;

class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Creating COM port objects.");
        SerialPortHandler COM7 = new SerialPortHandler("COM7", 9600, 8, Parity.None, StopBits.One, Handshake.None);
        SerialPortHandler COM12 = new SerialPortHandler("COM12", 9600, 8, Parity.None, StopBits.One, Handshake.None);

        Console.WriteLine("Writing data to COM port.");
        COM12.SendData("Hello, COM7!");

        Console.WriteLine("Reading data from COM port if available.");
        string receivedData = await COM7.ReceiveDataAsync(bufferSize: 64);

        //Console.WriteLine("Writing data to COM port.");
        //COM7.SendData("Hello, COM12!");


        Console.WriteLine(receivedData);

        //if (serialPort.BytesToRead > 0)
        //{
        //    int data = serialPort.ReadByte();
        //    Console.WriteLine(data);
        //}
        //else
        //{
        //    Console.WriteLine("No data available to read.");
        //}

        Console.WriteLine("Closing COM ports.");
        COM7.CloseSerialPort();
        COM12.CloseSerialPort();

    }
}

