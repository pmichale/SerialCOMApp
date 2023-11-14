using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SerialComApp.Models
{
    public class AppConfig
    {
        public string ComPortName { get; set; }
        public string BaudRate { get; set; }
        public string SelectedDataBitsItem { get; set; }
        public string SelectedParityItem { get; set; }
        public string SelectedStopBitsItem { get; set; }
        public string SelectedFlowControlItem { get; set; }
        public string SelectedEncodingItem { get; set; }

        public AppConfig()
        {
            ComPortName = "COM7";
            BaudRate = "9600";
            SelectedDataBitsItem = "8";
            SelectedParityItem = "None";
            SelectedStopBitsItem = "1";
            SelectedFlowControlItem = "None";
            SelectedEncodingItem = "ASCII";
        }

        public AppConfig(string comPortName, string baudRate, string selDataBitsItem, string selParityItem, string selStopBitsItem, string selFlowControlItem, string selEncodingItem)
        {
            ComPortName = comPortName;
            BaudRate = baudRate;
            SelectedDataBitsItem = selDataBitsItem;
            SelectedParityItem = selParityItem;
            SelectedStopBitsItem = selStopBitsItem;
            SelectedFlowControlItem = selFlowControlItem;
            SelectedEncodingItem = selEncodingItem;
        }

        public void SerializeSettings(string filePath)
        {
            var serializer = new XmlSerializer(typeof(AppConfig));

            using (var streamWriter = new StreamWriter(filePath))
            {
                serializer.Serialize(streamWriter, this);
            }
        }

        public void DeserializeSettings(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var serializer = new XmlSerializer(typeof(AppConfig));

            using (var streamReader = new StreamReader(filePath))
            {
                var cfg = (AppConfig)serializer.Deserialize(streamReader)!;

                if (cfg != null)
                {
                    ComPortName = cfg.ComPortName;
                    BaudRate = cfg.BaudRate;
                    SelectedDataBitsItem = cfg.SelectedDataBitsItem;
                    SelectedParityItem = cfg.SelectedParityItem;
                    SelectedStopBitsItem = cfg.SelectedStopBitsItem;
                    SelectedFlowControlItem = cfg.SelectedFlowControlItem;
                    SelectedEncodingItem = cfg.SelectedEncodingItem;
                }
                else
                {
                    throw new InvalidOperationException("Deserialization failed or resulted in a null value.");
                }
            }
        }
    }
}
