using System;

namespace Doomnet
{
    public struct PointD
    {
        private const double TOLERANCE = 0.001;

        public double X;
        public double Y;
        
        public static bool operator ==(PointD left, PointD right)
        {
            return Math.Abs(left.X - right.X) < TOLERANCE && Math.Abs(left.Y - right.Y) < TOLERANCE;
        }
        
        public static bool operator !=(PointD left, PointD right)
        {
            return !(left == right);
        }
        
        public override bool Equals(object obj)
        {
            if (!(obj is PointD))
                return false;
            PointD comp = (PointD)obj;
            return comp == this;
        }
        
        public void Offset(double dx, double dy)
        {
            X += dx;
            Y += dy;
        }
        
        public void Offset(PointD p)
        {
            Offset(p.X, p.Y);
        }

        public override string ToString()
        {
            return $"{{X={X},Y={Y}}}";
        }
    }
}