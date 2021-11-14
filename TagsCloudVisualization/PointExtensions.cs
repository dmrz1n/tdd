﻿using System;
using System.Drawing;

namespace TagsCloudVisualization
{
    internal static class PointExtensions
    {
        public static double GetDistanceTo(this Point start, Point finish)
        {
            var xDist = Math.Abs(finish.X - start.X);
            var yDist = Math.Abs(finish.Y - start.Y);
            return Math.Sqrt(xDist * xDist + yDist * yDist);
        }

        public static Point GetShiftTo(this Point from, Point to)
        {
            return new Point(to.X - from.X, to.Y - from.Y);
        }

        public static Point AddShift(this Point point, Point shift)
        {
            return new Point(point.X + shift.X, point.Y + shift.Y);
        }
    }
}
