﻿using System;
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
using Microsoft.Kinect.Interop;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.Interaction;

using Coding4Fun.Kinect.Wpf;

namespace basic_c4f
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 这个项目主要是使用了Coding4Fun的工具库,代码更加简洁一些
    /// 项目的变量和界面全部继承自basic项目
    /// </summary>
    public partial class MainWindow : Window
    {
        //私有Kinectsensor对象
        private KinectSensor kinect;

        private WriteableBitmap colorImageBitmap;
        private Int32Rect colorImageBitmapRect;
        private int colorImageStride;
        private byte[] colorImagePixelData;

        public KinectSensor Kinect
        {
            get { return this.kinect; }
            set
            {
                //如果带赋值的传感器和目前的不一样
                if (this.kinect != value)
                {
                    //如果当前的传感对象不为null
                    if (this.kinect != null)
                    {
                        UninitializeKinectSensor(this.kinect);
                        //uninitailize当前对象
                        this.kinect = null;
                    }
                    //如果传入的对象不为空，且状态为连接状态
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this.kinect = value;
                        InitializeKinectSensor(kinect);
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => DiscoverKinectSensor();
            this.Unloaded += (s, e) => this.kinect = null;

        }

        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {

            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.kinect == null)
                        this.kinect = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    if (this.kinect == e.Sensor)
                    {
                        this.kinect = null;
                        this.kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        if (this.kinect == null)
                        {
                            //TODO:通知用于Kinect已拔出
                        }
                    }
                    break;
                //TODO:处理其他情况下的状态
            }
        }

        private void InitializeKinectSensor(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                ColorImageStream colorStream = this.kinect.ColorStream;
                kinectSensor.ColorStream.Enable();

                kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
                kinectSensor.Start();
            }
        }
        private void UninitializeKinectSensor(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor.ColorFrameReady -= new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
            }
        }

        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    // 这里用了C4F的ToBitmapSource
                    ColorImageElement.Source = frame.ToBitmapSource();

                }
            }
        }

    }
}
