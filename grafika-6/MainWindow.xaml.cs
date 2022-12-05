using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace grafika_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //DOs - love me some shitty naming
        private List<KeyValuePair<int, List<Vector>>>? bezierCurves;
        private List<KeyValuePair<int, List<Line>>>? bezierLines;
        private List<KeyValuePair<int, List<Line>>>? curveLines;
        private List<KeyValuePair<int, List<Ellipse>>>? ellipseList;
        private List<Vector>? pointList;
        private List<Line>? lineList;
        private List<Line>? curveList;
        private List<Ellipse>? ellipses;

        private List<KeyValuePair<int, List<Vector>>>? polygonPoints;
        private List<KeyValuePair<int, Polygon>>? polygonList;
        private List<KeyValuePair<int, List<Ellipse>>>? polygonEllipseList;
        private List<Vector>? polygonPointList;
        private Polygon? polygon;
        private PointCollection? points;
        private List<Ellipse>? polygonEllipses;

        //vars

        //mode: 0 - bezier, 1 - 2d
        private int mode = 0;
        //bezMode: 0 - Draw, 1 - Edit, 2 - rotate, 3 - scaling
        private int bezMode = 0;

        //polyMode: 0 - Draw, 1 - move, 2 - rotate, 3 - scaling
        private int polyMode = 0;

        private readonly int ellipseRadius = 8;
        private readonly int lineThickness = 1;
        private readonly int polygonThickness = 2;

        private int editPosition;
        private int key;
        private Vector mouseStartPosition;
        private Point? turnPoint = null;
        private List<Vector>? rotateList;
        private Ellipse turnEllipse;

        private bool found = false;

        //brushes
        SolidColorBrush ellipseBrush, lineBrush, curveBrush, polyBrush;


        public MainWindow()
        {
            InitializeComponent();
            ellipseBrush = new();
            lineBrush = new();
            curveBrush = new();
            polyBrush = new();
            bezierCurves = new();
            bezierLines = new();
            curveLines = new();
            ellipseList = new();
            polygonPoints = new();
            polygonList = new();
            polygonEllipseList = new();
            ellipseBrush.Color = Colors.Blue;
            lineBrush.Color = Colors.Red;
            curveBrush.Color = Colors.Green;
            polyBrush.Color = Colors.Purple;
        }

        private void Bezier_Click(object sender, RoutedEventArgs e)
        {
            mode = 0;
            bezMode = 0;
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
        }

        private void D_Click(object sender, RoutedEventArgs e)
        {
            mode = 1;
            polyMode = 0;
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
        }

        private void cnv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mode == 0)
            {
                if (bezMode == 4) return;
                if (bezMode == 0)
                {
                    //create new lists if there is none
                    if (pointList == null)
                        pointList = new List<Vector>();

                    if (lineList == null)
                        lineList = new List<Line>();

                    if (curveList == null)
                        curveList = new List<Line>();
                    if (ellipses == null)
                        ellipses = new List<Ellipse>();

                    //get the click values and add to list
                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                    pointList.Add(point);

                    //Draw a point on click position
                    Ellipse ellipse = new Ellipse();
                    ellipse.Width = ellipse.Height = ellipseRadius;
                    ellipse.Stroke = ellipseBrush;
                    Canvas.SetTop(ellipse, point.Y - ellipseRadius / 2);
                    Canvas.SetLeft(ellipse, point.X - ellipseRadius / 2);
                    cnv.Children.Add(ellipse);
                    ellipses.Add(ellipse);

                    if (pointList.Count > 1)
                    {
                        //if more than 1 point draw a line
                        Line line = new Line();
                        line.StrokeThickness = lineThickness;
                        line.Stroke = lineBrush;
                        line.X1 = pointList[pointList.Count - 2].X;
                        line.Y1 = pointList[pointList.Count - 2].Y;
                        line.X2 = pointList[pointList.Count - 1].X;
                        line.Y2 = pointList[pointList.Count - 1].Y;
                        lineList.Add(line);
                        cnv.Children.Add(line);
                        List<Vector> bezCurvePoints = Algorithm.Bezier(pointList);

                        for (int i = 0; i < bezCurvePoints.Count - 1; i++)
                        {
                            if (curveList.Count < bezCurvePoints.Count - 1)
                            {
                                Line l = new Line();
                                l.StrokeThickness = lineThickness;
                                l.Stroke = curveBrush;
                                l.X1 = bezCurvePoints[i].X;
                                l.Y1 = bezCurvePoints[i].Y;
                                l.X2 = bezCurvePoints[i + 1].X;
                                l.Y2 = bezCurvePoints[i + 1].Y;
                                cnv.Children.Add(l);
                                curveList.Add(l);
                            }
                            else
                            {
                                curveList[i].X1 = bezCurvePoints[i].X;
                                curveList[i].Y1 = bezCurvePoints[i].Y;
                                curveList[i].X2 = bezCurvePoints[i + 1].X;
                                curveList[i].Y2 = bezCurvePoints[i + 1].Y;
                            }
                        }
                    }

                }
                else if (bezMode == 1)
                {
                    if (bezierCurves == null || bezierLines == null || curveLines == null) return;

                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                    foreach (var list in bezierCurves)
                    {
                        int i = 0;
                        foreach (var vector in list.Value)
                        {
                            if (Math.Abs(point.X - vector.X) < ellipseRadius / 2 && Math.Abs(point.Y - vector.Y) < ellipseRadius / 2)
                            {
                                found = true;
                                editPosition = i;
                                key = list.Key;
                                mouseStartPosition = point;

                                return;
                            }
                            i++;
                        }
                    }
                }
                else if (bezMode == 2 || bezMode == 3)
                {
                    if (curveLines == null || bezierCurves == null) return;

                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));

                    if (turnPoint == null)
                    {
                        turnPoint = new Point(point.X, point.Y);
                        Ellipse ellipse = new Ellipse();
                        ellipse.Width = ellipse.Height = ellipseRadius;
                        ellipse.Stroke = ellipse.Fill = Brushes.Yellow;
                        Canvas.SetTop(ellipse, point.Y - ellipseRadius / 2);
                        Canvas.SetLeft(ellipse, point.X - ellipseRadius / 2);
                        cnv.Children.Add(ellipse);
                        turnEllipse = ellipse;
                        return;
                    }

                    foreach (var list in bezierCurves)
                    {
                        int i = 0;
                        foreach (var vector in list.Value)
                        {
                            if (Math.Abs(point.X - vector.X) < ellipseRadius / 2 && Math.Abs(point.Y - vector.Y) < ellipseRadius / 2)
                            {
                                found = true;
                                editPosition = i;
                                key = list.Key;
                                mouseStartPosition = point;

                                if (rotateList == null)
                                    rotateList = new();
                                if (rotateList.Count > 0)
                                    rotateList.Clear();
                                foreach (Vector points in list.Value)
                                    rotateList.Add(points);
                                return;
                            }
                            i++;
                        }
                    }

                }

                else if (bezMode == 3)
                {

                }
            }
            if (mode == 1)
            {
                if (polyMode == 4) return;
                if (polyMode == 0)
                {
                    //create new lists if there is none
                    if (polygonPointList == null)
                        polygonPointList = new List<Vector>();

                    if (polygonEllipses == null)
                        polygonEllipses = new List<Ellipse>();

                    if (points == null)
                        points = new PointCollection();

                    if (polygon == null)
                    {
                        polygon = new Polygon();
                        polygon.Fill = polyBrush;
                        polygon.Stroke = Brushes.Black;
                        polygon.StrokeThickness = polygonThickness;
                    }

                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                    polygonPointList.Add(point);
                    Ellipse ellipse = new Ellipse();
                    ellipse.Width = ellipse.Height = ellipseRadius;
                    ellipse.Stroke = ellipseBrush;
                    Canvas.SetTop(ellipse, point.Y - ellipseRadius / 2);
                    Canvas.SetLeft(ellipse, point.X - ellipseRadius / 2);
                    cnv.Children.Add(ellipse);
                    polygonEllipses.Add(ellipse);
                    points.Add(new Point(point.X, point.Y));

                    if (polygonPointList.Count > 2)
                    {
                        polygon.Points = points;
                        if (!cnv.Children.Contains(polygon))
                            cnv.Children.Add(polygon);
                    }

                }

                if (polyMode == 1)
                {
                    if (polygonList == null) return;

                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                    Point p = new Point(point.X, point.Y);
                    foreach (KeyValuePair<int, Polygon> poly in polygonList)
                    {
                        if (InputHitTest(p) == poly.Value)
                        {
                            editPosition = poly.Key;
                            mouseStartPosition = point;
                            found = true;
                            return;
                        }
                    }
                }

                if (polyMode == 2 || polyMode == 3)
                {
                    if (polygonList == null) return;

                    Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));

                    if (turnPoint == null)
                    {
                        turnPoint = new Point(point.X, point.Y);
                        Ellipse ellipse = new Ellipse();
                        ellipse.Width = ellipse.Height = ellipseRadius;
                        ellipse.Stroke = ellipse.Fill = Brushes.Yellow;
                        Canvas.SetTop(ellipse, point.Y - ellipseRadius / 2);
                        Canvas.SetLeft(ellipse, point.X - ellipseRadius / 2);
                        cnv.Children.Add(ellipse);
                        turnEllipse = ellipse;
                        return;
                    }
                    Point p = new Point(point.X, point.Y);
                    foreach (KeyValuePair<int, Polygon> poly in polygonList)
                    {
                        if (InputHitTest(p) == poly.Value)
                        {
                            editPosition = poly.Key;
                            mouseStartPosition = point;
                            found = true;
                            if (rotateList == null)
                                rotateList = new();
                            if (rotateList.Count > 0)
                                rotateList.Clear();
                            foreach (Vector points in polygonPoints[editPosition].Value)
                                rotateList.Add(points);

                            return;
                        }
                    }

                }
            }


        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {

        }

        private void bezDraw_Click(object sender, RoutedEventArgs e)
        {
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
            if (mode == 0)
            {
                pointList = new List<Vector>();
                lineList = new List<Line>();
                curveList = new List<Line>();
                ellipses = new List<Ellipse>();
                bezMode = 0;
            }
            else if (mode == 1)
            {
                polygon = new Polygon();
                polygon.Fill = polyBrush;
                polygon.Stroke = Brushes.Black;
                polygon.StrokeThickness = polygonThickness;
                polygonEllipses = new List<Ellipse>();
                points = new PointCollection();
                polygonPointList = new List<Vector>();
                polyMode = 0;
            }
        }

        private void cnv_MouseMove(object sender, MouseEventArgs e)
        {
            if (mode == 0)
            {
                if (bezMode == 0 || bezMode == 4 || !found) return;

                Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                Vector difference = Vector.Subtract(point, mouseStartPosition);

                if (bezMode == 1)
                {


                    if (Math.Abs(difference.X) > 0 || Math.Abs(difference.Y) > 0)
                    {
                        //change point list
                        bezierCurves[key].Value[editPosition] = point;

                        //change ellipse position
                        Ellipse ellipse = ellipseList[key].Value[editPosition];
                        Canvas.SetTop(ellipse, point.Y - ellipseRadius / 2);
                        Canvas.SetLeft(ellipse, point.X - ellipseRadius / 2);
                        ellipseList[key].Value[editPosition] = ellipse;

                        //change line position
                        if (editPosition != 0)
                        {
                            Line line = bezierLines[key].Value[editPosition - 1];
                            line.X2 = point.X;
                            line.Y2 = point.Y;
                            bezierLines[key].Value[editPosition - 1] = line;
                        }
                        if (editPosition < bezierLines[key].Value.Count)
                        {
                            Line line = bezierLines[key].Value[editPosition];
                            line.X1 = point.X;
                            line.Y1 = point.Y;
                            bezierLines[key].Value[editPosition] = line;
                        }

                        //change bezier curve
                        List<Vector> bezCurvePoints = Algorithm.Bezier(bezierCurves[key].Value);

                        for (int i = 0; i < bezCurvePoints.Count - 1; i++)
                        {
                            curveLines[key].Value[i].X1 = bezCurvePoints[i].X;
                            curveLines[key].Value[i].Y1 = bezCurvePoints[i].Y;
                            curveLines[key].Value[i].X2 = bezCurvePoints[i + 1].X;
                            curveLines[key].Value[i].Y2 = bezCurvePoints[i + 1].Y;
                        }
                    }
                }
                if (bezMode == 2)
                {
                    if (Math.Abs(difference.X) > 0)
                    {
                        double rotationDeg = (difference.X / 2);
                        for (int i = 0; i < rotateList.Count; i++)
                        {
                            Point v = Algorithm.RotatePoint(new Point(rotateList[i].X, rotateList[i].Y), turnPoint.Value, rotationDeg);

                            bezierCurves[key].Value[i] = new Vector(Math.Round(v.X), Math.Round(v.Y));

                            Canvas.SetLeft(ellipseList[key].Value[i], v.X - ellipseRadius / 2);
                            Canvas.SetTop(ellipseList[key].Value[i], v.Y - ellipseRadius / 2);
                        }

                        for(int i = 0; i < bezierLines[key].Value.Count; i++)
                        {
                            bezierLines[key].Value[i].X1 = bezierCurves[key].Value[i].X;
                            bezierLines[key].Value[i].Y1 = bezierCurves[key].Value[i].Y;
                            bezierLines[key].Value[i].X2 = bezierCurves[key].Value[i + 1].X;
                            bezierLines[key].Value[i].Y2 = bezierCurves[key].Value[i + 1].Y;
                        }

                        List<Vector> bezCurvePoints = Algorithm.Bezier(bezierCurves[key].Value);

                        for (int i = 0; i < bezCurvePoints.Count - 1; i++)
                        {
                            curveLines[key].Value[i].X1 = bezCurvePoints[i].X;
                            curveLines[key].Value[i].Y1 = bezCurvePoints[i].Y;
                            curveLines[key].Value[i].X2 = bezCurvePoints[i + 1].X;
                            curveLines[key].Value[i].Y2 = bezCurvePoints[i + 1].Y;
                        }

                    }
                }

                if (bezMode == 3)
                {
                    if (Math.Abs(difference.Y) > 0)
                    {
                        double k = (difference.Y / 40);

                        for (int i = 0; i < rotateList.Count; i++)
                        {
                            Point v = Algorithm.ScalePoint(new Point(rotateList[i].X, rotateList[i].Y), turnPoint.Value, k);

                            bezierCurves[key].Value[i] = new Vector(Math.Round(v.X), Math.Round(v.Y));

                            Canvas.SetLeft(ellipseList[key].Value[i], v.X - ellipseRadius / 2);
                            Canvas.SetTop(ellipseList[key].Value[i], v.Y - ellipseRadius / 2);
                        }

                        for (int i = 0; i < bezierLines[key].Value.Count; i++)
                        {
                            bezierLines[key].Value[i].X1 = bezierCurves[key].Value[i].X;
                            bezierLines[key].Value[i].Y1 = bezierCurves[key].Value[i].Y;
                            bezierLines[key].Value[i].X2 = bezierCurves[key].Value[i + 1].X;
                            bezierLines[key].Value[i].Y2 = bezierCurves[key].Value[i + 1].Y;
                        }

                        List<Vector> bezCurvePoints = Algorithm.Bezier(bezierCurves[key].Value);

                        for (int i = 0; i < bezCurvePoints.Count - 1; i++)
                        {
                            curveLines[key].Value[i].X1 = bezCurvePoints[i].X;
                            curveLines[key].Value[i].Y1 = bezCurvePoints[i].Y;
                            curveLines[key].Value[i].X2 = bezCurvePoints[i + 1].X;
                            curveLines[key].Value[i].Y2 = bezCurvePoints[i + 1].Y;
                        }

                    }
                }

            }
            if (mode == 1)
            {
                if (polyMode == 0 || polyMode == 4 || !found) return;

                Vector point = new(Math.Round(e.GetPosition(cnv).X), Math.Round(e.GetPosition(cnv).Y));
                Vector difference = Vector.Subtract(point, mouseStartPosition);

                if (polyMode == 1)
                {
                    if (Math.Abs(difference.X) > 0 || Math.Abs(difference.Y) > 0)
                    {
                        foreach (Ellipse ellipse in polygonEllipseList[editPosition].Value)
                        {
                            Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + difference.X);
                            Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + difference.Y);
                        }
                        for (int i = 0; i < polygonPoints[editPosition].Value.Count; i++)
                        {
                            Vector vector = polygonPoints[editPosition].Value[i];
                            vector.X += difference.X;
                            vector.Y += difference.Y;
                            polygonPoints[editPosition].Value[i] = vector;
                        }
                        PointCollection pointCollection = new();
                        foreach (Vector vector in polygonPoints[editPosition].Value)
                        {
                            pointCollection.Add(new Point(vector.X, vector.Y));
                        }
                        polygonList[editPosition].Value.Points = pointCollection;
                        mouseStartPosition = point;
                    }
                }

                if (polyMode == 2)
                {
                    if (Math.Abs(difference.X) > 0)
                    {
                        double rotationDeg = (difference.X / 2);
                        for (int i = 0; i < rotateList.Count; i++)
                        {
                            Point v = Algorithm.RotatePoint(new Point(rotateList[i].X, rotateList[i].Y), turnPoint.Value, rotationDeg);

                            polygonPoints[editPosition].Value[i] = new Vector(Math.Round(v.X), Math.Round(v.Y));

                            Canvas.SetLeft(polygonEllipseList[editPosition].Value[i], v.X - ellipseRadius / 2);
                            Canvas.SetTop(polygonEllipseList[editPosition].Value[i], v.Y - ellipseRadius / 2);
                        }
                        PointCollection pointCollection = new();
                        foreach (Vector vector in polygonPoints[editPosition].Value)
                        {
                            pointCollection.Add(new Point(vector.X, vector.Y));
                        }
                        polygonList[editPosition].Value.Points = pointCollection;
                    }
                }

                if (polyMode == 3)
                {
                    if (Math.Abs(difference.Y) > 0)
                    {
                        double k = (difference.Y / 40);

                        for (int i = 0; i < rotateList.Count; i++)
                        {
                            Point v = Algorithm.ScalePoint(new Point(rotateList[i].X, rotateList[i].Y), turnPoint.Value, k);

                            polygonPoints[editPosition].Value[i] = new Vector(Math.Round(v.X), Math.Round(v.Y));

                            Canvas.SetLeft(polygonEllipseList[editPosition].Value[i], v.X - ellipseRadius / 2);
                            Canvas.SetTop(polygonEllipseList[editPosition].Value[i], v.Y - ellipseRadius / 2);
                        }
                        PointCollection pointCollection = new();
                        foreach (Vector vector in polygonPoints[editPosition].Value)
                        {
                            pointCollection.Add(new Point(vector.X, vector.Y));
                        }
                        polygonList[editPosition].Value.Points = pointCollection;
                    }
                }
            }

        }

        private void polyRotate_Click(object sender, RoutedEventArgs e)
        {
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
            polyMode = bezMode = 2;
            turnPoint = null;
        }

        private void polyMove_Click(object sender, RoutedEventArgs e)
        {
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
            polyMode = 1;
        }

        private void polyScale_Click(object sender, RoutedEventArgs e)
        {
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
            polyMode = bezMode = 3;
            turnPoint = null;
        }

        private void cnv_MouseUp(object sender, MouseButtonEventArgs e)
        {
            found = false;
        }

        private void bezEdit_Click(object sender, RoutedEventArgs e)
        {
            if (cnv.Children.Contains(turnEllipse))
                cnv.Children.Remove(turnEllipse);
            bezMode = 1;
        }

        private void bezDone_Click(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                if (pointList == null || lineList == null || curveList == null || ellipses == null) return;

                if (pointList.Count > 1)
                {
                    int count = bezierCurves.Count;
                    bezierCurves.Add(new KeyValuePair<int, List<Vector>>(count, pointList));
                    bezierLines.Add(new KeyValuePair<int, List<Line>>(count, lineList));
                    curveLines.Add(new KeyValuePair<int, List<Line>>(count, curveList));
                    ellipseList.Add(new KeyValuePair<int, List<Ellipse>>(count, ellipses));
                    bezMode = 4;
                    pointList = null;
                    lineList = null;
                    curveList = null;
                    ellipses = null;
                    return;
                }
                MessageBox.Show("Not enough points!");

            }
            else if (mode == 1)
            {
                if (polygonPointList == null || polygon == null || points == null || polygonEllipses == null) return;

                if (polygonPointList.Count > 2)
                {
                    int count = polygonPoints.Count;
                    polygonPoints.Add(new KeyValuePair<int, List<Vector>>(count, polygonPointList));
                    polygonList.Add(new KeyValuePair<int, Polygon>(count, polygon));
                    polygonEllipseList.Add(new KeyValuePair<int, List<Ellipse>>(count, polygonEllipses));
                    polyMode = 4;
                    polygonPointList = null;
                    polygon = null;
                    points = null;
                    polygonEllipses = null;
                    return;
                }
                MessageBox.Show("Not enough points!");
            }
        }
    }
}
