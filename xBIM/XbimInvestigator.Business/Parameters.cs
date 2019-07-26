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
    public enum LayoutMode
    {
        Corridor,
        Perimeter
    }

    public enum CorridorMode
    {
        BothSides,
        Left,
        Right
    }

    public enum StructureType
    {
        Residential,
        Garage,
        Podium
    }

    public enum LevelOfDetail
    {
        /// <summary>
        /// Creates a model with spaces and building elements
        /// </summary>
        FullDetail,
        /// <summary>
        /// Creates a model with spaces only
        /// </summary>
        SpacesOnly
    }

    public enum UnitDistributionType
    {
        Percentage,
        Quantity
    }

    public enum BaseUnits
    {
        Feet,
        Inches,
        Meters,
        Millimeters
    }

    /// <summary>
    /// The parameters for making the building
    /// </summary>
    public class XParameters
    {
        private static double Feet(double value) => value * 304.8;

        /// <summary>
        /// 
        /// </summary>
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// The reference line defining the center of the corridor
        /// </summary>
        public XLine ReferenceLine { get; set; }
        /// <summary>
        /// The perimeter of the building
        /// </summary>
        public XPolygon Perimeter { get; set; }
        /// <summary>
        /// Units all the length measures are in
        /// </summary>
        public BaseUnits BaseUnit { get; set; } = BaseUnits.Meters;
        /// <summary>
        /// Calculation method for unit distribution
        /// </summary>
        public UnitDistributionType UnitDistribution { get; set; } = UnitDistributionType.Percentage;
        /// <summary>
        /// 
        /// </summary>
        public LayoutMode LayoutMode { get; set; } = LayoutMode.Corridor;
        /// <summary>
        /// 
        /// </summary>
        public CorridorMode CorridorMode { get; set; } = CorridorMode.BothSides;
        /// <summary>
        /// The type of structure to create
        /// </summary>
        public StructureType StructureType { get; set; } = StructureType.Residential;
        /// <summary>
        /// The name of the building being created
        /// </summary>
        public string BuildingName { get; set; } = "Building";
        /// <summary>
        /// Interior is just space layout (false) or contains internal walls and doors (true)
        /// </summary>
        public LevelOfDetail LevelOfDetail { get; set; } =  LevelOfDetail.FullDetail;
        /// <summary>
        /// The number of floors to create
        /// </summary>
        public int NumberOfFloors { get; set; }
        /// <summary>
        /// The floor to floor height (distance from top of slab to top of slab)
        /// </summary>
        public double FloorToFloor { get; set; }
        public double ExteriorWallThickness { get; set; } = 150;
        public double InteriorWallThickness { get; set; } = 100;
        public double RoofSlabThickness { get; set; } = 100;
        public double GroundSlabThickness { get; set; } = 150;
        public double InterzoneSlabThickness { get; set; } = 100;
        public double CorridorWidth { get; set; } = Feet(8);
        public List<UnitParameters> UnitDefinitions { get; set; }
        /// <summary>
        /// Elevation of the underside of the slab above
        /// </summary>
        public double CeilingElevation => FloorToFloor - InterzoneSlabThickness;

        public XParameters()
        {
            //BuildingName = "Building 1";

            //NumberOfFloors = 5;
            //FloorToFloor = 9 * 304.8 + InterzoneSlabThickness;

            //UnitDefinitions = new List<UnitParameters>();
            //UnitParameters unit1 = new UnitParameters
            //{
            //    UnitType = "Studio",
            //    UnitWidth = Feet(18),
            //    UnitDepth = Feet(30),
            //    Color = "#71aacc"
            //};
            //unit1.CreateProfile(InteriorWallThickness);
            //UnitDefinitions.Add(unit1);

            //UnitParameters unit2 = new UnitParameters
            //{
            //    UnitType = "One Bedroom Type A",
            //    UnitWidth = Feet(22),
            //    UnitDepth = Feet(30),
            //    Color = "#c5cc71"
            //};
            //unit2.CreateProfile(InteriorWallThickness);
            //UnitDefinitions.Add(unit2);

            //UnitParameters unit3 = new UnitParameters
            //{
            //    UnitType = "One Bedroom Type B",
            //    UnitWidth = Feet(24),
            //    UnitDepth = Feet(30),
            //    Color = "#74a96f"
            //};
            //unit3.CreateProfile(InteriorWallThickness);
            //UnitDefinitions.Add(unit3);

            //UnitParameters unit4 = new UnitParameters
            //{
            //    UnitType = "Two Bedroom Type A",
            //    UnitWidth = Feet(27),
            //    UnitDepth = Feet(32),
            //    Color = "#a971cc"
            //};
            //unit4.CreateProfile(InteriorWallThickness);
            //UnitDefinitions.Add(unit4);
        }

    }

    /// <summary>
    /// Unit
    /// </summary>
    public class UnitParameters
    {
        public string UnitType { get; set; }
        public XPolygon Profile { get; set; }
        public double UnitWidth { get; set; }
        public double UnitDepth { get; set; }
        public string Color { get; set; }
        public string ImageFile { get; set; }
        public List<WindowParameters> Windows { get; set; } = new List<WindowParameters>();
        public List<DoorParameters> Doors { get; set; } = new List<DoorParameters>();

        /// <summary>
        /// How to distrinute the units 
        /// </summary>
        public double Distribution { get; set; } = 50;

        /// <summary>
        /// Makes the unit profile
        /// </summary>
        /// <remarks>
        /// The profile is centered at the mid point of width and depth, starts top left corner and
        /// is oriented clockwise. This is important as the first edge is the external wall edge, the
        /// second and fourth are internal edges and the third is the edge adjacent to the corridor.
        /// The profile is adjusted for the interior wall thickness if we are creating interior walls
        /// the two internal edges and the edge adjacent to the corridor are adjusted. The other edge
        /// is not as it is adjacent to an external wall
        /// </remarks>
        /// <param name="wallThickness">Optional parameter indicating the partition wall thickness</param>
        public void CreateProfile(double wallThickness = 0)
        {
            Profile = CreateProfileP(wallThickness);
        }

        public XPolygon CreateFullSizeProfile()
        {
            return CreateProfileP(0);
        }

        private XPolygon CreateProfileP(double wallThickness)
        {
            double unitWidth = UnitWidth - wallThickness;
            double unitDepth = UnitDepth - wallThickness;
            XPolygon profile = new XPolygon();
            profile.Add(new XbimPoint3D(-unitWidth / 2, UnitDepth / 2, 0));
            profile.Add(new XbimPoint3D(unitWidth / 2, UnitDepth / 2, 0));
            profile.Add(new XbimPoint3D(unitWidth / 2, -unitDepth / 2, 0));
            profile.Add(new XbimPoint3D(-unitWidth / 2, -unitDepth / 2, 0));
            return profile;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(UnitType)) return UnitType;
            else return base.ToString();
        }
    }

    public class WindowParameters
    {
        public double Offset { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double TopElevation { get; set; }

        public WindowParameters()
        {

        }

        public WindowParameters(double offset, double width, double height, double topElevation)
        {
            Offset = offset;
            Width = width;
            Height = height;
            TopElevation = topElevation;
        }
    }

    enum DoorSwingType
    {
        Left,
        Right
    }

    public class DoorParameters
    {
        public double Offset { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public DoorParameters()
        {

        }
        public DoorParameters(double offset, double width, double height)
        {
            Offset = offset;
            Width = width;
            Height = height;
        }
    }
}
