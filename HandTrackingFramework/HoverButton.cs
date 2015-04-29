using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;

namespace HandTrackingFramework
{
    public class HoverButton : KinectButton
    {
        readonly DispatcherTimer hoverTimer = new DispatcherTimer();
        protected bool timerEnabled = true;

        public double HoverInterval
        {
            get { return (double)GetValue(HoverIntervalProperty); }
            set
            {
                SetValue(HoverIntervalProperty, value);
            }
        } 

        public static readonly DependencyProperty HoverIntervalProperty =
            DependencyProperty.Register("HoverInterval", typeof(double), typeof(HoverButton), new UIPropertyMetadata(2000d));

        public HoverButton()
        {
            hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
            hoverTimer.Tick += new EventHandler(hoverTimer_Tick);
            hoverTimer.Stop();
        }

        void hoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        protected override void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (timerEnabled)
            {
                e.Cursor.StopCursorAnimation();
                hoverTimer.Stop();
            }
        }

        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            if (timerEnabled)
            {
                hoverTimer.Interval = TimeSpan.FromMilliseconds(HoverInterval);
                e.Cursor.AnimateCursor(HoverInterval);
                hoverTimer.Start();
            }
        }
    }
}


