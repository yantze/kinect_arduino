﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;


namespace KinectSimonSaysPoses
{
    public struct SkeletonAngle
    {
        public JointType CenterJoint;
        public JointType AngleJoint;
        public double Angle;
    }
}
