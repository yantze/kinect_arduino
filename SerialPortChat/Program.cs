using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;



namespace SerialPortChat
{
    public class PortChat
    {
        static bool _continue;
        static SerialPort _serialPort;

        public string _portName;
        public int _baudRate;

        public void send(string str)
        {
            System.IO.Ports.SerialPort chat = new System.IO.Ports.SerialPort(_portName, _baudRate);

            try
            {
                chat.Open();
            }
            catch (System.IO.IOException e)
            {
                // 如果没有串口没有打开
                Console.Write(e.Message);
                return;
            }
            chat.Write(str);
            chat.Close();
        }

        public void init(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public static void Main()
        {

        }
        public static void Text()
        {
            string name;
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = _serialPort.BaudRate;  // 9600 hz
            _serialPort.Parity = _serialPort.Parity;  // 
            _serialPort.DataBits = _serialPort.DataBits;  // 8 bits
            _serialPort.StopBits = _serialPort.StopBits;  // 1 bit
            _serialPort.Handshake = _serialPort.Handshake;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;
            readThread.Start();

            Console.Write("Content: ");
            name = Console.ReadLine();

            Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {
                    _serialPort.WriteLine(String.Format("<{0}>: {1}", name, message));
                }
            }

            readThread.Join();
            
            _serialPort.Close();
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    Console.WriteLine(message);
                }
                catch (System.IO.IOException)
                {
                    // 输入输出错误
                }
                catch (TimeoutException)
                {
                    // 超时
                }
            }
        }

        // Display Port values and prompt user to enter a port. 
        public static string SetPortName(string defaultPortName)
        {
            string portName;
            int port = 0;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !int.TryParse(portName, out port))
            {
                portName = defaultPortName;
            }
            else
            {
                portName = "COM" + port;
            }
            return portName;
        }
    }
}


