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

namespace XbimInvestigator.Business
{
    class XPreviewBuilding : XPreviewContainer
    {
        public IfcBuilding Building
        {
            get => Product as IfcBuilding;
            set => Product = value;
        }

        public XPreviewBuilding()
            : base()
        {

        }

        public XPreviewBuilding(IfcBuilding ifcBuilding)
            : base(ifcBuilding)
        {
            Product = ifcBuilding;
            Name = ifcBuilding.Name;
            foreach (IfcBuildingStorey buildingStory in ifcBuilding.BuildingStoreys)
            {
                XPreviewBuildingStory story = new XPreviewBuildingStory(buildingStory);
                story.Container = this;
            }
        }

        protected override void DoCommitAll(IfcStore model)
        {
            Building = model.Instances.New<IfcBuilding>();
            Building.CompositionType = IfcElementCompositionEnum.ELEMENT;
            base.DoCommitAll(model);

            // Add to the project
            var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
            if (project != null) project.AddBuilding(Building);

            foreach (XPreviewBuildingStory story in Containers)
            {
                story.CommitAll(model);
            }
        }

        public override string GetPreviewObjGroup(Xbim3DModelContext context)
        {
            string group = base.GetPreviewObjGroup(context);
            group = "# Model Created By Simergy" + Environment.NewLine + group;
            group = "mtllib pegasus.mtl" + Environment.NewLine + group;
            return group;
        }

        public override string GetSVGstring()
        {
            string svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink = \"http://www.w3.org/1999/xlink\">" + Environment.NewLine;
            svg += base.GetSVGstring();
            svg += "</svg>" + Environment.NewLine;
            return svg;
        }

        public XTreeNode GetModelTree()
        {
            XTreeNode treeNode = new XTreeNode(Name, GUID);
            XTreeNode floorNode = new XTreeNode("Floors");
            treeNode.ChildNodes.Add(floorNode);
            foreach (XPreviewBuildingStory story in Containers)
            {
                floorNode.ChildNodes.Add(story.GetModelTree());
            }
            return treeNode;
        }
    }
}
