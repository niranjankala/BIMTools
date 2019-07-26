using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xbim.Common;
using Xbim.Common.Step21;
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
    class XPreviewSpace : XPreviewElement
    {
        public string LongName { get; set; }
        public IfcSpace Space
        {
            get => Product as IfcSpace;
            set => Product = value;
        }

        public XPreviewSpace()
            : base()
        {

        }

        public XPreviewSpace(IfcSpace ifcSpace)
            : base(ifcSpace)
        {
            Product = ifcSpace;
            Name = Space.Name;
            LongName = Space.LongName;
        }

        protected override void DoCommitAll(IfcStore model)
        {
            Space = model.Instances.New<IfcSpace>();
            Space.LongName = LongName;
            Space.Description = Color;
            Space.CompositionType = IfcElementCompositionEnum.ELEMENT;
            base.DoCommitAll(model);
        }

        public override string GetSVGstring()
        {
            XPolygon profile = ProfilePath.Transformed(GlobalTransform);
            profile.Name = Name;
            profile.FillColor = Color;
            profile.LineColor = "black";

            string svg = profile.ToSVGPath();

            return svg;
        }

        protected override string GetObjMaterial()
        {
            if (LongName == null) return "usemtl corridor";
            else return "usemtl " + LongName.Replace(' ', '_');
        }

    }
}
