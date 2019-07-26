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

namespace XbimInvestigator.Business
{
    public class XLine
    {
        public XbimPoint3D sp { get; set; }
        public XbimPoint3D ep { get; set; }

        public XLine(XbimPoint3D sp, XbimPoint3D ep)
        {
            this.sp = sp;
            this.ep = ep;
        }

        public double Length => Math.Sqrt((sp.X - ep.X) * (sp.X - ep.X) + (sp.Y - ep.Y) * (sp.Y - ep.Y) + (sp.Z - ep.Z) * (sp.Z - ep.Z));

        public XbimVector3D Vector => new XbimVector3D(ep.X - sp.X, ep.Y - sp.Y, ep.Z - sp.Z);

        public XbimVector3D NormalizedVector => Vector.Normalized();

        public XLine Transformed(XbimMatrix3D transform)
        {
            XLine temp = new XLine(transform.Transform(sp), transform.Transform(ep));
            return temp;
        }

        public XbimPoint3D MidPoint => GetMidPoint();

        public XbimPoint3D GetMidPoint()
        {
            XbimPoint3D midPoint = new XbimPoint3D((sp.X + ep.X) / 2, (sp.Y + ep.Y) / 2, (sp.Z + ep.Z) / 2);
            return midPoint;
        }
    }

    public static class XbimExtensions
    {
        /// <summary>
        /// Gets a point a distance along the vector passing through the from point
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="fromPoint"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static XbimPoint3D GetPoint(this XbimVector3D vector, XbimPoint3D fromPoint, double distance)
        {
            return new XbimPoint3D(fromPoint.X + vector.X * distance, fromPoint.Y + vector.Y * distance, fromPoint.Z + vector.Z * distance);

        }

        public static double Distance(this XbimPoint3D p1, XbimPoint3D p2)
        {
            double a = p1.X - p2.X;
            double b = p1.Y - p2.Y;
            double c = p1.Z - p2.Z;

            return Math.Sqrt(a * a + b * b + c * c);

        }
    }
}
