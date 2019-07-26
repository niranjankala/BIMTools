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
using Xbim.ModelGeometry.Scene;
using Xbim.Common.Geometry;

namespace XbimInvestigator.Business
{
    class XPreviewContainer : XPreviewBase
    {
        public List<XPreviewElement> Elements { get; set; }

        public List<XPreviewContainer> Containers { get; set; }

        protected XPreviewContainer() : base()
        {
            Elements = new List<XPreviewElement>();
            Containers = new List<XPreviewContainer>();
        }

        protected XPreviewContainer(IfcSpatialStructureElement ifcEntity)
            : base(ifcEntity)
        {
            Elements = new List<XPreviewElement>();
            Containers = new List<XPreviewContainer>();
        }

        public XPreviewContainer Container
        {
            get => container;
            set
            {
                container = value;
                container.Containers.Add(this);
            }
        }

        public override XbimMatrix3D GlobalTransform
        {
            get
            {
                if (Container != null) return XbimMatrix3D.Multiply(LocalTransform, Container.GlobalTransform);
                else return LocalTransform;
            }
        }

        public virtual string GetPreviewObjGroup(Xbim3DModelContext context)
        {
            string objGroup = string.Empty;
            foreach (XPreviewElement element in Elements)
            {
                if (element.TryGetPreviewObjGroup(context, out string elementGroup))
                {
                    objGroup += elementGroup;
                }
            }
            foreach (XPreviewContainer container in Containers)
            {
                objGroup += container.GetPreviewObjGroup(context);
            }
            return objGroup;
        }

        public virtual string GetSVGstring()
        {
            string svg = string.Empty;
            foreach (XPreviewElement element in Elements)
            {
                if (element.TryGetSVGstring(out string elementSVG))
                {
                    svg += elementSVG;
                }
            }

            foreach (XPreviewContainer container in Containers)
            {
                svg += container.GetSVGstring();
            }

            return svg;
        }

        public List<XTreeNode> GetModelTreeNodes()
        {
            List<XTreeNode> treeNodes = new List<XTreeNode>();
            foreach (XPreviewElement element in Elements.OfType<XPreviewSpace>())
            {
                treeNodes.Add(element.GetModelTree());
            }
            return treeNodes;
        }
    }
}
