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
    class XPreviewSlab : XPreviewElement
    {
        public double Thickness { get; set; }

        public IfcSlab Slab
        {
            get => Product as IfcSlab;
            set => Product = value;
        }

        protected override void DoCommitAll(IfcStore model)
        {
            Slab = model.Instances.New<IfcSlab>();
            base.DoCommitAll(model);
        }
    }
}
