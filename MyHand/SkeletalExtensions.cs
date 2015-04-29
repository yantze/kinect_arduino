using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace MyHand
{
    public static class SkeletalExtensions
    {

        public static Skeleton GetPrimarySkeleton(this IEnumerable<Skeleton> skeletons)
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

        public static Joint? GetPrimaryHand(this Skeleton skeleton)
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



        public static Joint ScaleTo(this Joint joint, int width, int height, float skeletonMaxX, float skeletonMaxY)
        {
            Microsoft.Kinect.SkeletonPoint pos = new SkeletonPoint()
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            joint.Position = pos;

            return joint;
        }

        private static float Scale(int maxPixel, float maxSkeleton, float position)
        {
            float value = ((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));
            if (value > maxPixel)
                return maxPixel;
            if (value < 0)
                return 0;
            return value;
        }
    }
}
