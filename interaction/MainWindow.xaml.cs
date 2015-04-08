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
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;

namespace interaction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensorChooser sensorChooser;

        public MainWindow()
        {
            InitializeComponent();
            // 在主窗体的构造函数中注册OnLoad事件
            Loaded += OnLoaded;
        }

        // 创建OnLoaded的委托方法
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            //fill scroll content
            for (int i = 1; i < 20; i++)
            {
                var button = new KinectCircleButton
                {
                    Content = i,
                    Height = 200
                };

                int i1 = i;
                button.Click +=
                    (o, args) => MessageBox.Show("You clicked button #" + i1);

                scrollContent.Children.Add(button);
            }
        }

        // 如果传感器的状态发生改变，比如关闭或者初始化完成，将会触发SensorChooserOnKinectChanged事件
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            // MessageBox弹出提示信息,kinect是否连接成功
            // MessageBox.Show(args.NewSensor == null ? "No Kinect" : args.NewSensor.Status.ToString());

            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
                if (!error)
                    kinectRegion.KinectSensor = args.NewSensor;

            }

        }

        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You clicked me!");
        }
    }
}
