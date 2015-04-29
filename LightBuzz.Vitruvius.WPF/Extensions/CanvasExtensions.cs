#region using...
using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
#endregion

namespace LightBuzz.Vitruvius.WPF
{
    public static class CanvasExtensions
    {
        #region Constants

        static readonly string TAG = "GestureLib";

        static Color DEFAULT_COLOR = Colors.LightCyan;

        static double DEFAULT_ELLIPSE_RADIUS = 20;

        static double DEFAULT_LINE_THICKNESS = 8;

        #endregion

        #region Methods

        public static void DrawPoint(this Canvas canvas, Joint joint, Color color, double radius)
        {
            if (joint.TrackingState == JointTrackingState.NotTracked) return;

            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Ellipse ellipse = new Ellipse
            {
                Tag = TAG,
                Width = radius,
                Height = radius,
                Fill = new SolidColorBrush(color)
            };

            Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawPoint(this Canvas canvas, Joint joint, Color color)
        {
            DrawPoint(canvas, joint, color, DEFAULT_ELLIPSE_RADIUS);
        }

        public static void DrawPoint(this Canvas canvas, Joint joint)
        {
            DrawPoint(canvas, joint, DEFAULT_COLOR, DEFAULT_ELLIPSE_RADIUS);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second, Color color, double thickness)
        {
            if (first.TrackingState == JointTrackingState.NotTracked || second.TrackingState == JointTrackingState.NotTracked) return;

            first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Line line = new Line
            {
                Tag = TAG,
                X1 = first.Position.X,
                Y1 = first.Position.Y,
                X2 = second.Position.X,
                Y2 = second.Position.Y,
                StrokeThickness = thickness,
                Stroke = new SolidColorBrush(color)
            };

            canvas.Children.Add(line);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second, Color color)
        {
            DrawLine(canvas, first, second, color, DEFAULT_LINE_THICKNESS);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second)
        {
            DrawLine(canvas, first, second, DEFAULT_COLOR, DEFAULT_LINE_THICKNESS);
        }

        public static void DrawSkeleton(this Canvas canvas, Skeleton skeleton, Color color)
        {
            if (skeleton == null) return;

            foreach (Joint joint in skeleton.Joints)
            {
                canvas.DrawPoint(joint, color);
            }

            canvas.DrawLine(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter], color);
            canvas.DrawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine], color);
            canvas.DrawLine(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter], color);
            canvas.DrawLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight], color);
            canvas.DrawLine(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft], color);
            canvas.DrawLine(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight], color);
        }

        public static void DrawSkeleton(this Canvas canvas, Skeleton skeleton)
        {
            DrawSkeleton(canvas, skeleton, DEFAULT_COLOR);
        }

        public static void ClearSkeletons(this Canvas canvas)
        {
            List<UIElement> items = new List<UIElement>();

            foreach (UIElement item in canvas.Children)
            {
                if (item is Shape)
                {
                    Shape shape = item as Shape;

                    if (shape.Tag == null || shape.Tag.ToString() != TAG)
                    {
                        items.Add(item);
                    }
                }
            }

            canvas.Children.Clear();

            foreach (UIElement item in items)
            {
                canvas.Children.Add(item);
            }
        }

        #endregion
    }
}