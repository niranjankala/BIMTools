using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using ClipperLib;

namespace XbimInvestigator.Business
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public class XPolygon : List<XbimPoint3D>
    {
        public string Name { get; set; }
        public string FillColor { get; set; }
        public string LineColor { get; set; }
        public int LineWidth { get; set; }

        /// <summary>
        /// Shofts the points int he polygon relative to the origin.
        /// </summary>
        /// <remarks>
        /// Because XbimPoint3 wont allow us to modify the X,Y,Z values then we have to create new points
        /// </remarks>
        /// <param name="origin"></param>
        public void Normalize(XbimPoint3D origin)
        {
            XbimVector3D offset = new XbimVector3D(-origin.X, -origin.Y, -origin.Z);
            XbimMatrix3D tr = new XbimMatrix3D(offset);
            List<XbimPoint3D> temp = new List<XbimPoint3D>(this);
            Clear();
            temp.ForEach(pt => Add(tr.Transform(pt)));
        }

        public XPolygon Normalized(XbimPoint3D origin)
        {
            XPolygon temp = new XPolygon();
            ForEach(pt => temp.Add(new XbimPoint3D(pt.X, pt.Y, pt.Z)));
            temp.Normalize(origin);
            return temp;
        }

        public XbimPoint3D GetCentroid()
        {
            double area = GetArea();
            double cx = 0, cy = 0;

            for (int i = 0; i < Count; i++)
            {
                XbimPoint3D pnt1 = this[i];
                XbimPoint3D pnt2 = this[(i + 1) % Count];
                double dFactor = (pnt1.X * pnt2.Y) - (pnt2.X * pnt1.Y);
                cx += (pnt1.X + pnt2.X) * dFactor;
                cy += (pnt1.Y + pnt2.Y) * dFactor;
            }
            cx *= 1 / (area * 6);
            cy *= 1 / (area * 6);
            XbimPoint3D centroid = new XbimPoint3D(cx, cy, this.First().Z);
            return centroid;

        }

        public XbimPoint3D Centroid => GetCentroid();


        public double GetArea()
        {
            double area = 0;
            for (int i = 0; i < Count; i++)
            {
                area += this[i].X * this[(i + 1) % Count].Y;
                area -= this[i].Y * this[(i + 1) % Count].X;
            }

            area /= 2;
            return area;
        }

        public double Area => GetArea();

        /// <summary>
        /// Creates an identical copy of the polygon
        /// </summary>
        /// <returns></returns>
        public XPolygon Clone()
        {
            XPolygon copy = new XPolygon();
            ForEach(pt => copy.Add(new XbimPoint3D(pt.X, pt.Y, pt.Z)));
            return copy;
        }

        /// <summary>
        /// Creates a transformed copy of the polygon
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public XPolygon Transformed(XbimMatrix3D transform)
        {
            XPolygon transformed = new XPolygon();
            ForEach(p => transformed.Add(transform.Transform(p)));
            return transformed;
        }

        #region Clipping

        public List<XPolygon> Clip(ClipType clipType, XPolygon clip, double scaleBy = 1000)
        {
            return XPolygon.Clip(clipType, new List<XPolygon>() { this }, new List<XPolygon>() { clip }, scaleBy);
        }

        enum PathOrientation
        {
            Clockwise,
            Anticlockwise
        }

        public static List<XPolygon> Clip(ClipType clipType, List<XPolygon> subjects, List<XPolygon> clips, double scaleBy = 1000)
        {

            List<XPolygon> results = new List<XPolygon>();

            Paths subjPaths = ToPaths(subjects, scaleBy);
            Paths clipPaths = ToPaths(clips, scaleBy);

            Clipper clipper = new Clipper();
            clipper.AddPaths(clipPaths, PolyType.ptSubject, closed: true);
            clipper.AddPaths(subjPaths, PolyType.ptClip, closed: true);
            PolyTree result = new PolyTree();
            clipper.Execute(clipType, result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            PathOrientation orientation = Clipper.Orientation(subjPaths.First()) == true ? PathOrientation.Anticlockwise : PathOrientation.Clockwise;

            return FromPolyTree(result, orientation, scaleBy);
        }

        #region converting from polygons to paths

        private static Paths ToPaths(XPolygon polygon, double scaleBy = 1)
        {
            Paths paths = new Paths();
            paths.Add(ToPath(polygon, scaleBy));
            return paths;
        }

        private static Paths ToPaths(List<XPolygon> polygons, double scaleBy = 1)
        {
            Paths paths = new Paths();
            polygons.ForEach(p => paths.AddRange(ToPaths(p, scaleBy)));
            return paths;
        }

        private static Path ToPath(List<XbimPoint3D> path, double scaleBy = 1)
        {
            Path clipPath = new Path();
            for (int i = 0; i < path.Count; i++)
            {
                IntPoint p = new IntPoint((int)(path[i].X * scaleBy), (int)(path[i].Y * scaleBy));
                //p.userData = path[i].UserData;
                clipPath.Add(p);
            }

            return clipPath;
        }

        #endregion

        #region Converting from Paths to Polygons

        private static List<XPolygon> FromPolyTree(PolyNode results, PathOrientation subjectOrientation, double scaleBy = 1000)
        {
            List<XPolygon> polygons = new List<XPolygon>();

            foreach (PolyNode child in results.Childs)
            {
                PathOrientation orientation = Clipper.Orientation(child.Contour) == true ? PathOrientation.Anticlockwise : PathOrientation.Clockwise;
                XPolygon polygon = FromPath(child.Contour, scaleBy);
                polygons.Add(polygon);

                foreach (PolyNode childNode in child.Childs)
                {
                    //polygon.InnerPaths.Add(FromPath(childNode.Contour, scaleBy));
                    //List<SimplePolygon> childPolygons = FromPolyTree(childNode, subjectOrientation, scaleBy);
                    //polygons.AddRange(childPolygons);
                }
                if (orientation != subjectOrientation)
                    polygon.Reverse();
                //polygon.MakeLines();
            }

            return polygons;
        }

        private static XPolygon FromPath(Path clipPath, double scaleBy = 1)
        {
            XPolygon path = new XPolygon();
            foreach (IntPoint pt in clipPath)
            {
                XbimPoint3D p = new XbimPoint3D(pt.X / scaleBy, pt.Y / scaleBy, 0);
                //p.UserData = pt.userData;
                path.Add(p);
            }
            return path;
        }

        #endregion

        #endregion

        public string ToSVGPath()
        {
            // Temporary scale so I can see something in the browser!
            double scale = 1;

            string svgPath = "<path id=\"" + Name + "\" d=";

            svgPath += "\"M " + this[0].X * scale + " " + this[0].Y * scale;
            XbimPoint3D lastPoint = this[0];
            for (int i = 1; i <= Count; i++)
            {
                XbimVector3D pt = XbimPoint3D.Subtract(this[(i) % Count], this[i - 1]);
                svgPath += " l " + pt.X * scale + " " + pt.Y * scale;
            }
            svgPath += " z \"";
            svgPath += " stroke=\"" + LineColor + "\"";
            svgPath += " fill=\"" + FillColor + "\"";
            svgPath += " />" + Environment.NewLine;

            return svgPath;
        }
    }
}
