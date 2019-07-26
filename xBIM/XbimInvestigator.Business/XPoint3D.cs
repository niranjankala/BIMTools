using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common.Geometry;

namespace XbimInvestigator.Business
{
    /// <summary>
    /// Could be XbimPoint3D
    /// </summary>
    public class XPoint3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public XPoint3D()
        {
            X = Y = Z = 0;
        }

        public XPoint3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public XbimPoint3D ToXbimPoint3D() => new XbimPoint3D(X, Y, Z);

    }
}
