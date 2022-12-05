using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace grafika_6
{
    public static class Algorithm
    {
        public static List<Vector> Bezier(List<Vector> pointList)
        {
            List<Vector> bezCurvePoints = new List<Vector>();

            List<int> binCoef = new List<int>();

            int nFact = 1;

            for (int i = 2; i < pointList.Count; i++)
                nFact *= i;

            int iFact = 1, nMinIFact = nFact;

            for (int i = 0; i < pointList.Count; i++)
            {
                if (i != 0)
                    iFact *= i;

                int coe = nFact / (iFact * nMinIFact);
                binCoef.Add(coe);
                
                if (nMinIFact != 1)
                    nMinIFact /= (pointList.Count - 1) - i;
            }

            //boze ale mi sie nie chce
            for (double i = 0; i <= 1; i += 0.01)
            {
                int xSum = 0, ySum = 0;
                for (int y = 0; y < pointList.Count; y++)
                {
                    xSum += (int)Math.Round(binCoef[y] * Math.Pow(1.0 - i, pointList.Count - 1 - y) * Math.Pow(i, y) * pointList[y].X);
                    ySum += (int)Math.Round(binCoef[y] * Math.Pow(1.0 - i, pointList.Count - 1 - y) * Math.Pow(i, y) * pointList[y].Y);
                }
                Vector vector = new Vector(xSum, ySum);
                bezCurvePoints.Add(vector);
            }

            return bezCurvePoints;
        }

        public static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public static Point ScalePoint(Point pointToScale, Point centerPoint, double k)
        {
            Point distance = new Point(pointToScale.X - centerPoint.X, pointToScale.Y - centerPoint.Y);
            return new Point(distance.X * k + pointToScale.X, distance.Y * k + pointToScale.Y);
        }

    }
}
