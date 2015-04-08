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
        }

        // 如果传感器的状态发生改变，比如关闭或者初始化完成，将会触发SensorChooserOnKinectChanged事件
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            // MessageBox弹出提示信息
            // MessageBox.Show(args.NewSensor == null ? "No Kinect" : args.NewSensor.Status.ToString());
        }
    }
}
