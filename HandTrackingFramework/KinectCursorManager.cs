using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Kinect;

namespace HandTrackingFramework
{
    public class KinectCursorManager
    {
        private KinectSensor kinectSensor;
        private CursorAdorner cursorAdorner;
        private readonly Window window;
        private UIElement lastElementOver;
        private bool isSkeletonTrackingActivated;
        private static bool isInitialized;
        private static KinectCursorManager instance;
        private bool isHandTrackingActivated;

        private List<GesturePoint> gesturePoints;
        private bool gesturePointTrackingEnabled;
        private double swipeLength, swipeDeviation;
        private int swipeTime;
        public event KinectCursorEventHandler swipeDetected;
        public event KinectCursorEventHandler swipeOutofBoundDetected;

        private double xOutOfBoundsLength;
        private static double initialSwipeX;
        public static void Create(Window window)
        {
            if (!isInitialized)
            {
                instance = new KinectCursorManager(window);
                isInitialized = true;
            }
        }

        public static void Create(Window window, FrameworkElement cursor)
        {
            if (!isInitialized)
            {
                instance = new KinectCursorManager(window, cursor);
                isInitialized = true;
            }
        }

        public static void Create(Window window, KinectSensor sensor)
        {
            if (!isInitialized)
            {
                instance = new KinectCursorManager(window, sensor);
                isInitialized = true;
            }
        }

        public static void Create(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            if (!isInitialized)
            {
                instance = new KinectCursorManager(window, sensor, cursor);
                isInitialized = true;
            }
        }

        public static KinectCursorManager Instance
        {
            get { return instance; }
        }

        private KinectCursorManager(Window window) : this(window, KinectSensor.KinectSensors[0]) { }
        private KinectCursorManager(Window window, FrameworkElement cursor) : this(window, KinectSensor.KinectSensors[0], cursor) { }
        private KinectCursorManager(Window window, KinectSensor sensor) : this(window, sensor, null) { }
        private KinectCursorManager(Window window, KinectSensor sensor, FrameworkElement cursor)
        {
            this.window = window;
            if (KinectSensor.KinectSensors.Count > 0)
            {
                window.Unloaded += delegate
                {
                    if (this.kinectSensor.SkeletonStream.IsEnabled)
                        this.kinectSensor.SkeletonStream.Disable();
                };
                window.Loaded += delegate
                {
                    if (cursor == null)
                        cursorAdorner = new CursorAdorner((FrameworkElement)window.Content);
                    else
                        cursorAdorner = new CursorAdorner((FrameworkElement)window.Content, cursor);

                    this.kinectSensor = sensor;
                    this.kinectSensor.SkeletonFrameReady += SkeletonFrameReady;
                    this.kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters());
                    this.kinectSensor.Start();
                };
            }
        }



        private static UIElement GetElementAtScreenPoint(Point point, Window window)
        {
            if (!window.IsVisible)
                return null;
            Point windowPoint = window.PointFromScreen(point);
            IInputElement element = window.InputHitTest(windowPoint);
            if (element is UIElement)
                return (UIElement)element;
            else
                return null;
        }

        private static Skeleton GetPrimarySkeleton(IEnumerable<Skeleton> skeletons)
        {
            Skeleton primarySkeleton = null;
            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }
                if (primarySkeleton == null)
                    primarySkeleton = skeleton;
                else if (primarySkeleton.Position.Z > skeleton.Position.Z)
                    primarySkeleton = skeleton;
            }
            return primarySkeleton;
        }

        private static Joint? GetPrimaryHand(Skeleton skeleton)
        {
            Joint leftHand = skeleton.Joints[JointType.HandLeft];
            Joint rightHand = skeleton.Joints[JointType.HandRight];
            if (rightHand.TrackingState == JointTrackingState.Tracked)
            {
                if (leftHand.TrackingState != JointTrackingState.Tracked)
                    return rightHand;
                else if (leftHand.Position.Z > rightHand.Position.Z)
                    return rightHand;
                else
                    return leftHand;
            }

            if (leftHand.TrackingState == JointTrackingState.Tracked)
            {
                return leftHand;
            }
            else
                return null;
        }

        private void SetSkeletonTrackingActivated()
        {
            if (lastElementOver != null && isSkeletonTrackingActivated == false)
            {
                lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorActivatedEvent));
            }
            isSkeletonTrackingActivated = true;
        }

        private void SetSkeletonTrackingDeactivated()
        {
            if (lastElementOver != null && isSkeletonTrackingActivated == false)
            {
                lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorDeactivatedEvent));
            }
            isSkeletonTrackingActivated = false;
        }

        private void HandleCursorEvents(Point point, double z)
        {
            UIElement element = GetElementAtScreenPoint(point, window);
            if (element != null)
            {
                element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorMoveEvent, point, z) { Cursor = cursorAdorner });
                if (element != lastElementOver)
                {
                    if (lastElementOver != null)
                    {
                        lastElementOver.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorLeaveEvent, point, z) { Cursor = cursorAdorner });
                    }
                    element.RaiseEvent(new KinectCursorEventArgs(KinectInput.KinectCursorEnterEvent, point, z) { Cursor = cursorAdorner });
                }
            }
            lastElementOver = element;
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null || frame.SkeletonArrayLength == 0) return;

                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                Skeleton skeleton = GetPrimarySkeleton(skeletons);

                if (skeleton == null)
                {
                    SetHandTrackingDeactivated();
                }
                else
                {
                    Joint? primaryHand = GetPrimaryHand(skeleton);
                    if (primaryHand.HasValue)
                    {
                        UpdateCursor(primaryHand.Value);
                    }
                    else
                    {
                        SetHandTrackingDeactivated();
                    }
                }
            }
        }

        private void SetHandTrackingDeactivated()
        {
            cursorAdorner.SetVisibility(false);
            if (lastElementOver != null && isHandTrackingActivated == true)
            { lastElementOver.RaiseEvent(new RoutedEventArgs(KinectInput.KinectCursorDeactivatedEvent)); };
            isHandTrackingActivated = false;
        }

        private void UpdateCursor(Joint hand)
        {
            var point = kinectSensor.MapSkeletonPointToDepth(hand.Position, kinectSensor.DepthStream.Format);
            float x = point.X;
            float y = point.Y;
            float z = point.Depth;
            x = (float)(x * window.ActualWidth / kinectSensor.DepthStream.FrameWidth);
            y = (float)(y * window.ActualHeight / kinectSensor.DepthStream.FrameHeight);

            Point cursorPoint = new Point(x, y);
            HandleCursorEvents(cursorPoint, z);
            cursorAdorner.UpdateCursor(cursorPoint);
        }

        public void GesturePointTrackingInitialize(double swipeLength, double swipeDeviation, int swipeTime, double xOutOfBounds)
        {
            this.swipeLength = swipeLength;
            this.swipeDeviation = swipeDeviation;
            this.swipeTime = swipeTime;
            this.xOutOfBoundsLength = xOutOfBounds;
        }

        public void GesturePointTrackingStart()
        {
            if (swipeLength + swipeDeviation + swipeTime == 0)
                throw new InvalidOperationException("挥动手势识别参数没有初始化！");
            gesturePointTrackingEnabled = true;
        }

        public void GesturePointTrackingStop()
        {
            xOutOfBoundsLength = 0;
            gesturePointTrackingEnabled = false;
            gesturePoints.Clear();
        }

        public bool GesturePointTrackingEnabled
        {
            get { return gesturePointTrackingEnabled; }
        }

        private void ResetGesturePoint(GesturePoint point)
        {
            bool startRemoving = false;
            for (int i = gesturePoints.Count; i >= 0; i--)
            {
                if (startRemoving)
                    gesturePoints.RemoveAt(i);
                else
                    if (gesturePoints[i].Equals(point))
                        startRemoving = true;
            }
        }

        private void ResetGesturePoint(int point)
        {
            if (point < 1)
                return;
            for (int i = point - 1; i >= 0; i--)
            {
                gesturePoints.RemoveAt(i);
            }
        }

        private void HandleGestureTracking(float x, float y, float z)
        {
            if (!gesturePointTrackingEnabled)
                return;
            // check to see if xOutOfBounds is being used
            if (xOutOfBoundsLength != 0 && initialSwipeX == 0)
            {
                initialSwipeX = x;
            }

            GesturePoint newPoint = new GesturePoint() { X = x, Y = y, Z = z, T = DateTime.Now };
            gesturePoints.Add(newPoint);

            GesturePoint startPoint = gesturePoints[0];
            var point = new Point(x, y);


            //check for deviation
            if (Math.Abs(newPoint.Y - startPoint.Y) > swipeDeviation)
            {
                //Debug.WriteLine("Y out of bounds");
                if (swipeOutofBoundDetected != null)
                    swipeOutofBoundDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = cursorAdorner });
                ResetGesturePoint(gesturePoints.Count);
                return;
            }
            if ((newPoint.T - startPoint.T).Milliseconds > swipeTime) //check time
            {
                gesturePoints.RemoveAt(0);
                startPoint = gesturePoints[0];
            }
            if ((swipeLength < 0 && newPoint.X - startPoint.X < swipeLength) // check to see if distance has been achieved swipe left
                || (swipeLength > 0 && newPoint.X - startPoint.X > swipeLength)) // check to see if distance has been achieved swipe right
            {
                gesturePoints.Clear();

                //throw local event
                if (swipeDetected != null)
                    swipeDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = cursorAdorner });
                return;
            }
            if (xOutOfBoundsLength != 0 &&
                ((xOutOfBoundsLength < 0 && newPoint.X - initialSwipeX < xOutOfBoundsLength) // check to see if distance has been achieved swipe left
                || (xOutOfBoundsLength > 0 && newPoint.X - initialSwipeX > xOutOfBoundsLength))
                )
            {
                if (swipeOutofBoundDetected != null)
                    swipeOutofBoundDetected(this, new KinectCursorEventArgs(point) { Z = z, Cursor = cursorAdorner });
            }
        }
    }
}
