using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace SerialComApp.Models
{
    public class DataReceiver
    {
        private SerialPortHandler _serialPortHandler;

        public DataReceiver(SerialPortHandler serialPortHandler)
        {
            _serialPortHandler = serialPortHandler;

            // Subscribe to the DataReceived event
            _serialPortHandler.GetSerialPortObject().DataReceived += DataReceivedHandler;
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadLine();
            // Handle the received data as needed
            Console.WriteLine("Data received: " + data);
        }
    }
}