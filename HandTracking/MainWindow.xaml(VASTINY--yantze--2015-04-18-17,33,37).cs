using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using HandTrackingFramework;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Expression.Drawing;
using SerialPortChat;
using System.Threading;

namespace HandTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _kinectDevice;
        private Skeleton[] _FrameSkeletons;
        private WaveGesture _WaveGesture;
        private PortChat portChat;
        private int i;
        private bool _continue;
        Thread readThread;

        public MainWindow()
        {
            InitializeComponent();

            portChat = new PortChat();
            portChat.init("COM3", 9600);
            _continue = false;
            readThread = new Thread(sendServo);

            this._WaveGesture = new WaveGesture();
            this._WaveGesture.GestureDetected += new EventHandler(_WaveGesture_GestureDetected);
            this._kinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            this._kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Green);
            listBox1.Items.Add(string.Format("button clicked {0}", DateTime.Now.ToLongTimeString()));
            portChat.send("s45");
            //_continue = true;
            //if (readThread.IsAlive)
            //    readThread.Start();
            
        }

        private void Button_KinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Red);
            // _continue = false;
            readThread.Interrupt();

        }


        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    this._FrameSkeletons = new Skeleton[_kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                    frame.CopySkeletonDataTo(this._FrameSkeletons);

                    // 检测挥动手势
                    DateTime startMarker = DateTime.Now;
                    this._WaveGesture.Update(this._FrameSkeletons, frame.Timestamp);

                    
                }
            }
        }

        private void _WaveGesture_GestureDetected(object sender, EventArgs e)
        {
            listBox1.Items.Add(string.Format("Wave Detected {0}", DateTime.Now.ToLongTimeString()));
        }

        private void sendServo()
        {
            int angleX = 0;
            while (_continue)
            {
                char chanX = (char)angleX;
                portChat.send("s"+chanX + chanX);
                angleX += 20;
                if (angleX > 180) angleX = 0;
                Thread.Sleep(1000);
            }
        }
    }
}
