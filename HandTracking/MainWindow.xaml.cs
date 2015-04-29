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
        private int angleX;
        private int angleY;
        private int preAngleX;
        private int preAngleY;
        private bool _continue;
        Thread serialPortThread;

        public MainWindow()
        {
            InitializeComponent();

            portChat = new PortChat();
            portChat.init("COM3", 9600);
            _continue = true;
            serialPortThread = new Thread(sendServo);
            serialPortThread.Start();

            this._WaveGesture = new WaveGesture();
            this._WaveGesture.GestureDetected += new EventHandler(_WaveGesture_GestureDetected);
            this._kinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            this._kinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Green);
            //listBox1.Items.Add(string.Format("button clicked {0}", DateTime.Now.ToLongTimeString()));
            // portChat.send("s47");
            _continue = true;
            angleX += 20;
            
        }

        private void Button_KinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            var button = sender as KinectButton;
            button.Background = new SolidColorBrush(Colors.Red);
            //_continue = false;

            //listBox1.Items.Add(string.Format("your hand leave button {0}", DateTime.Now.ToLongTimeString()));
            angleX += 20;

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
            //listBox1.Items.Add(string.Format("Wave Detected {0}", DateTime.Now.ToLongTimeString()));
            angleX += 20;
        }


        // 控制舵机角度
        private void sendServo()
        {
            angleX = 0;
            angleY = 0;
            preAngleX = 0;
            preAngleY = 0;
            while (_continue)
            {
                if (angleX != preAngleX || angleY != preAngleY)
                {
                    if (angleX < 0) angleX = 0;
                    else if (angleX > 126) angleX = 127;
                    if (angleY < 0 ) angleY = 0;
                    else if (angleY > 126) angleY = 127;
                    portChat.send("s"+ (char)angleX + (char)angleY);
                    preAngleX = angleX;
                    preAngleY = angleY;
                    //angleX += 10;
                }
                Thread.Sleep(200);
            }
        }

        #region no change
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serialPortThread.Abort();
        }

        private void rightBtnHover_Click(object sender, RoutedEventArgs e)
        {
            angleX -= 20;
        }

        private void leftBtnHover_Click(object sender, RoutedEventArgs e)
        {
            angleX += 20;
        }

        private void leftBtnHover_KinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            angleX += 20;
        }

        private void rightBtnHover_KinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            angleX -= 20;
        }

        private void downBtnHover_Click(object sender, RoutedEventArgs e)
        {
            angleY -= 20;
        }

        private void downBtnHover_KinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            angleY -= 20;
        }

        private void upBtnHover_Click(object sender, RoutedEventArgs e)
        {
            angleY += 20;
        }

        private void upBtnHover_KinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            angleY += 20;
        }
        #endregion

    }
}
