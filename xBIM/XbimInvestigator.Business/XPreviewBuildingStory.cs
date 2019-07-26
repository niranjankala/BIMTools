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
    class XPreviewBuildingStory:XPreviewContainer
    {
        public int StoryNumber { get; set; }

        public XPreviewBuildingStory()
            : base()
        {

        }

        public XPreviewBuildingStory(IfcBuildingStorey ifcBuildingStory)
            : base(ifcBuildingStory)
        {
            Name = ifcBuildingStory.Name;
            foreach (IfcSpace ifcSpace in ifcBuildingStory.Spaces)
            {
                XPreviewSpace space = new XPreviewSpace(ifcSpace);
                space.Container = this;
            }
        }

        public IfcBuildingStorey BuildingStory
        {
            get => Product as IfcBuildingStorey;
            set => Product = value;
        }

        protected override void DoCommitAll(IfcStore model)
        {
            BuildingStory = model.Instances.New<IfcBuildingStorey>();
            BuildingStory.CompositionType = IfcElementCompositionEnum.ELEMENT;
            BuildingStory.Elevation = Location.Z;
            base.DoCommitAll(model);

            // Find the building
            var building = model.Instances.OfType<IfcBuilding>().Where(b => b.Name == container.Name).FirstOrDefault();
            if (building != null) building.AddToSpatialDecomposition(BuildingStory);

            foreach (XPreviewElement element in Elements)
            {
                if (element is XPreviewSpace space)
                {
                    space.CommitAll(model);
                    BuildingStory.AddToSpatialDecomposition(space.Space);
                }
                else if (element is XPreviewSlab || element is XPreviewWall)
                {
                    element.CommitAll(model);
                    BuildingStory.AddElement(element.Product);
                }
            }
        }

        public XTreeNode GetModelTree()
        {
            XTreeNode treeNode = new XTreeNode(Name, GUID);
            XTreeNode unitsNode = new XTreeNode("Units");
            treeNode.ChildNodes.Add(unitsNode);
            unitsNode.ChildNodes.AddRange(base.GetModelTreeNodes());
            return treeNode;
        }
    }
}
