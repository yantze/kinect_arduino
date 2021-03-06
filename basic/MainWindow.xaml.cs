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
using System.IO;
using System.Drawing;

using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;

namespace basic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //私有Kinectsensor对象
        private KinectSensor kinect;

        private WriteableBitmap colorImageBitmap;
        private Int32Rect colorImageBitmapRect;
        private int colorImageStride;
        private byte[] colorImagePixelData;

        private WriteableBitmap depthImageBitmap;
        private Int32Rect depthImageBitmapRect;
        private int depthImageStride;
        private byte[] depthImagePixelData;

        // 保存最后的一张深度图像
        private DepthImageFrame lastDepthFrame;
        private short[] depthPixelData;

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
                        //InitializeKinectSensor(kinect);
                        InitializeKinectSensor_Depth(kinect);
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
                this.colorImageBitmap = new WriteableBitmap(colorStream.FrameWidth,
                    colorStream.FrameHeight, 96, 96, PixelFormats.Bgr32, null);
                this.colorImageBitmapRect = new Int32Rect(0, 0, colorStream.FrameWidth,
                    colorStream.FrameHeight);
                this.colorImageStride = colorStream.FrameWidth * colorStream.FrameBytesPerPixel;
                ColorImageElement.Source = this.colorImageBitmap;

                kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
                kinectSensor.Start();
            }
        }
        private void InitializeKinectSensor_Depth(KinectSensor kinectSensor)
        {
            if (kinectSensor != null)
            {
                DepthImageStream depthStream = kinectSensor.DepthStream;
                depthStream.Enable();

                depthImageBitmap = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                depthImageBitmapRect = new Int32Rect(0, 0, depthStream.FrameWidth, depthStream.FrameHeight);
                depthImageStride = depthStream.FrameWidth * depthStream.FrameBytesPerPixel;

                DepthImageElement.Source = depthImageBitmap;
                kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;
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
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    //ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,
                    //                                            PixelFormats.Bgr32, null, pixelData,
                    //                                            frame.Width * frame.BytesPerPixel);
                    this.colorImageBitmap.WritePixels(this.colorImageBitmapRect, pixelData,
                        this.colorImageStride, 0);

                    //ColorImageElement.Source = frame.ToBitmapSource();

                }
            }
        }
        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (lastDepthFrame != null)
                {
                    lastDepthFrame.Dispose();
                    lastDepthFrame = null;
                }
                lastDepthFrame = e.OpenDepthImageFrame();
                if (lastDepthFrame != null)
                {
                    depthPixelData = new short[lastDepthFrame.PixelDataLength];
                    lastDepthFrame.CopyPixelDataTo(depthPixelData);
                    depthImageBitmap.WritePixels(depthImageBitmapRect, depthPixelData, depthImageStride, 0);

                    CreateLighterShadesOfGray(depthFrame, depthPixelData);
                }
            }
        }


        // 添加点击事件获取深度图像的距离信息
        private void DepthImageElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(DepthImageElement);
            if (depthPixelData != null && depthPixelData.Length > 0)
            {
                Int32 pixelIndex = (Int32)(p.X + ((Int32)p.Y * this.lastDepthFrame.Width));
                Int32 depth = this.depthPixelData[pixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                Int32 depthInches = (Int32)(depth * 0.0393700787);
                Int32 depthFt = depthInches / 12;
                depthInches = depthInches % 12;
                PixelDepth.Text = String.Format("{0}mm~{1}'{2}", depth, depthFt, depthInches);
            }
        }

        // 将之前的深度位数据取反获取更好的深度影像数据
        // 过滤掉了大于3.5米小于0米的数据，将这些数据设置为白色
        private void CreateLighterShadesOfGray(DepthImageFrame depthFrame, short[] pixelData)
        {
            Int32 depth;
            Int32 loThreashold = 10;
            Int32 hiThreshold = 3500;
            short[] enhPixelData = new short[depthFrame.Width * depthFrame.Height];
            for (int i = 0; i < pixelData.Length; i++)
            {
                depth = pixelData[i] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                if (depth < loThreashold || depth > hiThreshold)
                {
                    enhPixelData[i] = 0xFF;
                }
                else
                {
                    enhPixelData[i] = (short)~pixelData[i];
                }
            }
            EnhancedDepthImage.Source = BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Gray16, null, enhPixelData, depthFrame.Width * depthFrame.BytesPerPixel);
        }
    }
}
