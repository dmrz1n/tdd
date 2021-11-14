﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace TagsCloudVisualization
{
    public class CircularCloudLayouter
    { 
        private readonly List<Rectangle> _rectangles;
        public IReadOnlyCollection<Rectangle> Rectangles
            => _rectangles;
        private readonly Point _cloudCenter;
        public Point CloudCenter
            => new Point(_cloudCenter.X, _cloudCenter.Y);
        private const double CurveStartingRadius = 0;
        private const double MinimumCurveAngleStep = 0.2;
        private const double CurveAngleStepSlowDown = 0.002;
        private const double DirectionBetweenRoundsCoeff = 1 / (2 * Math.PI);
        private double _curveAngleStep = Math.PI / 10;
        private double _currentCurveAngle;
        private double _enclosingCircleRadius;

        public CircularCloudLayouter(Point center)
        {
            _rectangles = new List<Rectangle>();
            _cloudCenter = center;
            _enclosingCircleRadius = 0;
        }

        public Rectangle PutNextRectangle(Size rectangleSize)
        {
            CheckRectangleSizeCorrectness(rectangleSize);
            var nextRectangleCoordinates = GetNextRectangleCoordinates(rectangleSize);
            var result = GetRectangleByCenter(nextRectangleCoordinates, rectangleSize);
            _enclosingCircleRadius = RecalculateEnclosingCircleRadius(result);
            _rectangles.Add(result);
            return result;
        }

        public double GetCloudEnclosingRadius()
            => _enclosingCircleRadius;

        private Point GetNextRectangleCoordinates(Size rectSize)
        {
            var nextRectCenter = GetNextRectCenter(rectSize);
            nextRectCenter = ShiftRectangleToCloudCenter(nextRectCenter, rectSize);
            return nextRectCenter;
        }

        private Point ShiftRectangleToCloudCenter(Point rectCenter, Size rectSize)
        {
            rectCenter = GetSuitableRectCenterInDirection(rectCenter,
                rectSize, CloudCenter);
            rectCenter = GetSuitableRectCenterInDirection(rectCenter,
                rectSize, new Point(rectCenter.X, CloudCenter.Y));
            rectCenter = GetSuitableRectCenterInDirection(rectCenter,
                rectSize, new Point(CloudCenter.X, rectCenter.Y));
            return rectCenter;
        }

        private Point GetNextRectCenter(Size rectSize)
        {
            var nextRectCenter = GetNextCurvePoint();
            var nextRect = GetRectangleByCenter(nextRectCenter, rectSize);
            while (DoesRectIntersectAnyOther(nextRect))
            {
                nextRectCenter = GetNextCurvePoint();
                nextRect = GetRectangleByCenter(nextRectCenter, rectSize);
            }

            return nextRectCenter;
        }

        private Point GetNextCurvePoint()
        {
            _currentCurveAngle += _curveAngleStep;
            if (_curveAngleStep > MinimumCurveAngleStep)
                _curveAngleStep -= CurveAngleStepSlowDown;
            return new Point(
                Convert.ToInt32((CurveStartingRadius + DirectionBetweenRoundsCoeff * _currentCurveAngle) 
                                * Math.Cos(_currentCurveAngle)) + CloudCenter.X,
                Convert.ToInt32((CurveStartingRadius + DirectionBetweenRoundsCoeff * _currentCurveAngle) 
                                * Math.Sin(_currentCurveAngle)) + CloudCenter.Y);
        }

        private static Rectangle GetRectangleByCenter(Point centerCoords, Size rectangleSize)
        {
            var location = new Point(centerCoords.X - rectangleSize.Width / 2,
                centerCoords.Y - rectangleSize.Height / 2);
            return new Rectangle(location, rectangleSize);
        }

        private static void CheckRectangleSizeCorrectness(Size rectangleSize)
        {
            if (rectangleSize.Width <= 0 || rectangleSize.Height <= 0)
                throw new ArgumentException("Width or height is less than or equal to 0");
        }

        private bool DoesRectIntersectAnyOther(Rectangle rect)
            => _rectangles.Any(r => r != rect && rect.IntersectsWith(r));

        private Point GetSuitableRectCenterInDirection(Point rectCenter, Size rectSize,
            Point directionPoint)
        {
            var leftBorder = directionPoint;
            var rightBorder = rectCenter;
            var borderDistance = leftBorder.GetDistanceTo(rightBorder);
            const int eps = 2;
            while (borderDistance > eps)
            {
                var middle = new Point(
                    (leftBorder.X + rightBorder.X) / 2,
                    (leftBorder.Y + rightBorder.Y) / 2);
                var pos = GetRectangleByCenter(middle, rectSize);
                if (DoesRectIntersectAnyOther(pos))
                    leftBorder = middle;
                else
                    rightBorder = middle;
                borderDistance = leftBorder.GetDistanceTo(rightBorder);
            }
            return rightBorder;
        }

        private double RecalculateEnclosingCircleRadius(Rectangle rect)
        {
            var rectEdges = new List<Point>
            {
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Left, rect.Height),
                new Point(rect.Right, rect.Height),
            };
            var edgeDistancesToCenter = rectEdges
                .Select(p => p.GetDistanceTo(CloudCenter))
                .Max();
            return Math.Max(_enclosingCircleRadius, edgeDistancesToCenter);
        }
    }
}
