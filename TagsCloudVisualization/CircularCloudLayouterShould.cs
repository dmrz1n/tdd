﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using NUnit.Framework;
using FluentAssertions;


namespace TagsCloudVisualization
{
    [TestFixture]
    public class CircularCloudLayouterShould
    {
        [Test]
        public void InitializeFieldsAfterInstanceCreation()
        {
            var center = new Point(10, 10);

            var layouter = new CircularCloudLayouter(center);

            layouter.Rectangles.Should().NotBeNull();
            layouter.CloudCenter.Should().BeEquivalentTo(center);
        }

        [TestCase(-1, 20, TestName = "width or height is negative")]
        [TestCase(10, 0, TestName = "width or height equals zero")]
        public void ThrowExceptionWhenRectangleSizeIsIncorrect(int width, int height)
        {
            var rectangleSize = new Size(-1, 20);
            var center = new Point(10, 10);
            var layouter = new CircularCloudLayouter(center);
            Action act = () => layouter.PutNextRectangle(rectangleSize);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PutFirstRectangleInTheCloudCenter()
        {
            var rectangleSize = new Size(100, 100);
            var center = new Point(0, 0);
            var layouter = new CircularCloudLayouter(center);
            var expectedLocation = new Point(-50, -50);

            var firstRectangle = layouter.PutNextRectangle(rectangleSize);

            firstRectangle.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Test]
        public void MakeCloudCircleDeviationLessThanTwentyFivePercents()
        {
            var center = new Point(750, 750);
            var layouter = new CircularCloudLayouter(center);

            PutRandomRectangles(layouter, 1000);
            var cloudConvexHull = GetCloudConvexHull(layouter);
            var (minLength, maxLength) = GetMinMaxHullVectorsLengths(center, cloudConvexHull);
            var deviation = GetCloudDeviation(minLength, maxLength);

            deviation.Should().BeLessOrEqualTo(0.25);
        }

        [Test]
        public void MakeCloudDense()
        {
            var center = new Point(750, 750);
            var layouter = new CircularCloudLayouter(center);

            PutRandomRectangles(layouter, 500);
            var cloudConvexHull = GetCloudConvexHull(layouter);
            var enclosingCircleRadius = GetMinMaxHullVectorsLengths(center, cloudConvexHull).maxLength;
            var enclosingCircleArea = Math.PI * enclosingCircleRadius * enclosingCircleRadius;
            var cloudArea = layouter.Rectangles.Sum(rect => rect.Width * rect.Height);
            var deviation = GetCloudDeviation(cloudArea, enclosingCircleArea);

            deviation.Should().BeLessOrEqualTo(0.25);
        }

        private static IEnumerable<Point> GetCloudConvexHull(CircularCloudLayouter layouter)
        {
            var rectanglesPoints = ConvexHullBuilder.GetRectanglesPointsSet(layouter.Rectangles);
            var cloudConvexHull = ConvexHullBuilder.GetConvexHull(rectanglesPoints);
            return cloudConvexHull;
        }

        [Test]
        public void PutRectanglesWithousIntersects()
        {
            var center = new Point(0, 0);
            var layouter = new CircularCloudLayouter(center);

            PutRandomRectangles(layouter, 100);

            AreRectanglesIntersected(layouter).Should().BeFalse();
        }


        private static bool AreRectanglesIntersected(CircularCloudLayouter layouter)
        {
            foreach (var firstRect in layouter.Rectangles)
                foreach (var secondRect in layouter.Rectangles)
                    if (firstRect.IntersectsWith(secondRect) && firstRect != secondRect)
                        return true;
            return false;
        }

        private static void PutRandomRectangles(CircularCloudLayouter layouter, int rectanglesCount)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var minWidth = 10;
            var maxWidth = 70;
            var minHeight = 10;
            var maxHeight = 35;

            for (var i = 0; i < rectanglesCount; i++) 
                layouter.PutNextRectangle(
                    new Size(rnd.Next(minWidth, maxWidth), rnd.Next(minHeight, maxHeight)));
        }

        private double GetCloudDeviation(double cloudValue, double deviateFrom)
        {
            return 1 - Math.Abs(cloudValue / deviateFrom);
        }

        private (double minLength, double maxLength) GetMinMaxHullVectorsLengths(
            Point center, IEnumerable<Point> hull)
        {
            var hullVectorsLengths = hull.Select(point => new Vector(center, point).GetLength());
            return (hullVectorsLengths.Min(), hullVectorsLengths.Max());
        }
    }
}
