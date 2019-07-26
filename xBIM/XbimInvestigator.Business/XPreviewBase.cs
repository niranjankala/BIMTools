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
using Xbim.Ifc.Extensions;

using XbimInvestigator.Business.Utils;

namespace XbimInvestigator.Business
{
    /// <summary>
    /// Base class for Xbim preview elements - implements everything from IfcProduct down 
    /// </summary>
    class XPreviewBase
    {
        public string Name { get; set; }
        public IfcProduct Product { get; set; }
        public XbimPoint3D Location { get; set; }
        public XbimVector3D ReferenceDirection { get; set; }
        public XbimVector3D Axis { get; set; }
        public string GUID { get; set; }

        /// <summary>
        /// The container for this object
        /// </summary>
        protected XPreviewContainer container;

        public virtual XbimMatrix3D GlobalTransform
        {
            get
            {
                if (container != null) return XbimMatrix3D.Multiply(LocalTransform, container.GlobalTransform);
                else return LocalTransform;
            }
        }

        public XbimMatrix3D InverseGlobalTransform
        {
            get
            {
                XbimMatrix3D tr = GlobalTransform;
                tr.Invert();
                return tr;
            }
        }

        public XbimMatrix3D LocalTransform
        {
            get
            {
                var tr = XbimMatrix3D.Identity;
                if (Product != null)
                {
                    tr = (Product.ObjectPlacement as IIfcLocalPlacement).RelativePlacement.ToMatrix3D();
                }
                else
                {
                    // Took this from the extension method ToMatrix3D as used above
                    if (ReferenceDirection != null && Axis != null)
                    {
                        var cross = Axis.CrossProduct(ReferenceDirection);
                        tr = new XbimMatrix3D(ReferenceDirection.X, ReferenceDirection.Y, ReferenceDirection.Z, 0,
                            cross.X, cross.Y, cross.Z, 0,
                            Axis.X, Axis.Y, Axis.Z, 0,
                            Location.X, Location.Y, Location.Z, 1);
                    }
                    else
                        tr = new XbimMatrix3D(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, Location.X, Location.Y, Location.Z, 1);
                }
                return tr;
            }
        }

        public XPreviewBase()
        {
            Location = new XbimPoint3D(0, 0, 0);
            ReferenceDirection = new XbimVector3D(1, 0, 0);
            Axis = new XbimVector3D(0, 0, 1);
            // We don't set a guid, when we create the Ifc objects a guid will be created for us
            GUID = Guid.NewGuid().ToIfcGuid();
        }

        public XPreviewBase(IfcProduct product)
            : this()
        {
            Name = product.Name;
            Product = product;
            GUID = product.GlobalId;

            if (product.ObjectPlacement is IfcLocalPlacement localPlacement)
            {
                if (localPlacement.RelativePlacement is IfcAxis2Placement3D relativePlacement)
                {
                    Location = relativePlacement.Location.XbimPoint3D();
                    ReferenceDirection = relativePlacement.RefDirection.XbimVector3D();
                    Axis = relativePlacement.Axis.XbimVector3D();
                }
            }
        }


        protected IfcLocalPlacement CreatePlacement(IfcStore model)
        {
            // Relative placement
            var placement = model.Instances.New<IfcAxis2Placement3D>();
            placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(Location.X, Location.Y, Location.Z));
            placement.RefDirection = model.Instances.New<IfcDirection>(d => d.SetXYZ(ReferenceDirection.X, ReferenceDirection.Y, ReferenceDirection.Z));
            placement.Axis = model.Instances.New<IfcDirection>(d => d.SetXYZ(Axis.X, Axis.Y, Axis.Z));

            var localPlacement = model.Instances.New<IfcLocalPlacement>();
            localPlacement.RelativePlacement = placement;
            // Relative placement
            if (container != null) localPlacement.PlacementRelTo = container.Product.ObjectPlacement;
            return localPlacement;
        }

        #region Commit

        public void CommitAll(IfcStore model)
        {
            if (model.CurrentTransaction == null)
            {
                using (var txn = model.BeginTransaction("Commit All"))
                {
                    DoCommitAll(model);
                    txn.Commit();
                }
            }
            else
                DoCommitAll(model);
        }

        protected virtual void DoCommitAll(IfcStore model)
        {
            Product.Name = Name;
            Product.GlobalId = GUID;
            Product.ObjectPlacement = CreatePlacement(model);
        }

        #endregion

        protected virtual IfcProductDefinitionShape CreateRepresentations(IfcStore model)
        {
            return model.Instances.New<IfcProductDefinitionShape>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
