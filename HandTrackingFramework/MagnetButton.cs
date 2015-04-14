using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls;

namespace HandTrackingFramework
{
    public class MagnetButton : HoverButton
    {
        protected bool isLockOn = true;
        public static readonly RoutedEvent KinectCursorLockEvent = KinectInput.KinectCursorUnlockEvent.AddOwner(typeof(MagnetButton));
        public static readonly RoutedEvent KinectCursorUnlockEvent = KinectInput.KinectCursorLockEvent.AddOwner(typeof(MagnetButton));
        private Storyboard move;
        public event KinectCursorEventHandler KinectCursorLock
        {
            add { base.AddHandler(KinectCursorLockEvent, value); }
            remove { base.RemoveHandler(KinectCursorLockEvent, value); }
        }

        public event KinectCursorEventHandler KinectCursorUnLock
        {
            add { base.AddHandler(KinectCursorUnlockEvent, value); }
            remove { base.RemoveHandler(KinectCursorUnlockEvent, value); }
        }

        public double LockInterval
        {
            get { return (double)GetValue(LockIntervalProperty); }
            set { SetValue(LockIntervalProperty, value); }
        }

        public static readonly DependencyProperty LockIntervalProperty =
            DependencyProperty.Register("LockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(200d));

        public double UnlockInterval
        {
            get { return (double)GetValue(UnlockIntervalProperty); }
            set { SetValue(UnlockIntervalProperty, value); }
        }

        public static readonly DependencyProperty UnlockIntervalProperty =
            DependencyProperty.Register("UnlockInterval", typeof(double), typeof(MagnetButton), new UIPropertyMetadata(80d));


        private T FindAncestor<T>(DependencyObject dependencyObject) where T : class
        {
            DependencyObject target = dependencyObject;
            do
            {
                target = VisualTreeHelper.GetParent(target);
            }
            while (target != null && !(target is T));
            return target as T;
        }

        protected override void OnKinectCursorEnter(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            if (!this.isLockOn)
                return;

            //获取按钮位置
            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual).Transform(new Point(0, 0));

            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;
            cursor.UpdateCursor(new Point(e.X, e.Y), true);

            //找到目的位置
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2, y - cursor.CursorVisual.ActualHeight / 2);
            //当前位置
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);
            //将光标从当前位置传送到目的位置
            AnimateCursorToLockPosition(e, x, y, cursor, ref lockPoint, ref cursorPoint);
            base.OnKinectCursorEnter(sender, e);
        }

        protected override void OnKinectCursorLeave(object sender, KinectCursorEventArgs e)
        {
            if (this.Opacity == 0)
                return;
            base.OnKinectCursorLeave(sender, e);
            if (!isLockOn)
                return;
            e.Cursor.UpdateCursor(new Point(e.X, e.Y), false);

            var rootVisual = FindAncestor<Window>(this);
            var point = this.TransformToAncestor(rootVisual).Transform(new Point(0, 0));

            var x = point.X + this.ActualWidth / 2;
            var y = point.Y + this.ActualHeight / 2;

            var cursor = e.Cursor;

            //找到目的位置
            Point lockPoint = new Point(x - cursor.CursorVisual.ActualWidth / 2, y - cursor.CursorVisual.ActualHeight / 2);
            //当前位置
            Point cursorPoint = new Point(e.X - cursor.CursorVisual.ActualWidth / 2, e.Y - cursor.CursorVisual.ActualHeight / 2);

            AnimateCursorAwayFromLockPosition(e, cursor, ref lockPoint, ref cursorPoint);
        }

        private void AnimateCursorAwayFromLockPosition(KinectCursorEventArgs e, CursorAdorner cursor, ref Point lockPoint, ref Point cursorPoint)
        {
            DoubleAnimation moveLeft = new DoubleAnimation(lockPoint.X, cursorPoint.X, new Duration(TimeSpan.FromMilliseconds(UnlockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));
            DoubleAnimation moveTop = new DoubleAnimation(lockPoint.Y, cursorPoint.Y, new Duration(TimeSpan.FromMilliseconds(UnlockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));
            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);
            move.Completed += delegate
            {
                move.Stop(cursor);
                cursor.UpdateCursor(new Point(e.X, e.Y), false);
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorUnlockEvent, new Point(e.X, e.Y), e.Z) { Cursor = e.Cursor });
            };
            move.Begin(cursor, true);
        }

        private void AnimateCursorToLockPosition(KinectCursorEventArgs e, double x, double y, CursorAdorner cursor, ref Point lockPoint, ref Point cursorPoint)
        {
            DoubleAnimation moveLeft = new DoubleAnimation(cursorPoint.X, lockPoint.X, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveLeft, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Canvas.LeftProperty));

            DoubleAnimation moveTop = new DoubleAnimation(cursorPoint.Y, lockPoint.Y, new Duration(TimeSpan.FromMilliseconds(LockInterval)));
            Storyboard.SetTarget(moveTop, cursor.CursorVisual);
            Storyboard.SetTargetProperty(moveTop, new PropertyPath(Canvas.TopProperty));
            move = new Storyboard();
            move.Children.Add(moveTop);
            move.Children.Add(moveLeft);
            move.Completed += delegate
            {
                this.RaiseEvent(new KinectCursorEventArgs(KinectCursorLockEvent, new Point(x, y), e.Z) { Cursor = e.Cursor });
            };
            if (move != null)
                move.Stop(e.Cursor);
            move.Begin(cursor, false);
        }
    }
}
