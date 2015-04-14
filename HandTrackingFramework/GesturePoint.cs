using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandTrackingFramework
{
    public struct GesturePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public DateTime T { get; set; }

        public override bool Equals(object obj)
        {
            var o = (GesturePoint)obj;
            return (X == o.X) && (Y == o.Y) && (Z == o.Z) && (T == o.T);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
