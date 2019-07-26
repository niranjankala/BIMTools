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
using Xbim.ModelGeometry.Scene;
using ClipperLib;
using Newtonsoft.Json;
using XbimInvestigator.Business.Utils;

namespace XbimInvestigator.Business
{

    public interface IXModelCreator
    {
        /// <summary>
        /// Creates a project
        /// </summary>
        /// <param name="name">The name of the project</param>
        /// <param name="units">The units the model will be defined in</param>
        /// <returns>The project identifier</returns>
        string CreateProject(string name, BaseUnits units);
        /// <summary>
        /// Creates a building using the input parameters
        /// </summary>
        /// <param name="parameters"></param>
        void CreateBuilding(XParameters parameters);
        /// <summary>
        /// Creates a building using the input parameters
        /// </summary>
        /// <param name="parameters"></param>
        void CreateBuilding(XParameters parameters, string projectId);
        /// <summary>
        /// Creates a 3D OBJ file and returns a path to the file
        /// </summary>
        /// <returns></returns>
        string CreateOBJFile(string projectId);
        string CreateOBJFile(IfcStore model, string projectId);
        /// <summary>
        /// Creates an IFC file of the model
        /// </summary>
        /// <returns></returns>
        string CreateIFCFile();
        /// <summary>
        /// Creates an SVG file
        /// </summary>
        /// <returns></returns>
        string CreateSVGFile(string projectId);
        /// <summary>
        /// Creates project tree hierarchy
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        XTreeNode GetModelTree(string projectId, bool serialize = false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        List<XTreeNode> CreateProjectTree(string projectId);
        /// <summary>
        /// Sets project storage folder path
        /// </summary>
        /// <param name="storagePath"></param>
        string StoragePath { get; set; }

    }

    public class XModelCreator : IXModelCreator
    {
        /// <summary>
        /// COnverts any number to base unit
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        double BaseUnit(double value) => value / scaleFactor;
        /// <summary>
        /// Scale factor
        /// </summary>
        /// <remarks>
        /// All input at the moment is in mm as it is from simergy. The scale factor here converts to the
        /// model unit we want to use, 1000 converts to meters, 304.8 converts to feet and 1 keeps everything in mm
        ///(the input numbers are dividied by scale because 1/304.8 is a long number!)
        /// </remarks>
        private readonly double scaleFactor = 1;
        private IfcStore model;
        private XPreviewBuilding building;
        private XParameters parameters;

        /// <summary>
        /// Contains the unit definition of all the units that will be created
        /// </summary>
        private List<UnitParameters> unitsToCreate;

        public string StoragePath { get; set; }
        /// <summary>
        /// Creates the building
        /// </summary>
        public void CreateBuilding(string jsonstring)
        {
            XParameters parameters = JsonConvert.DeserializeObject<XParameters>(jsonstring);
            CreateBuildingP(parameters);
        }

        public string CreateProject(string name, BaseUnits units)
        {
            Guid guid = Guid.NewGuid();

            model = InitializeModel(units);
            string path = GetFolderPath(guid.ToString()) + guid + ".ifc";
            model.SaveAs(path, StorageType.Ifc);
            model.Close();
            return guid.ToString();
        }

        public void CreateBuilding(XParameters parameters)
        {
            string json = JsonConvert.SerializeObject(parameters);
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\pegasus.json";
            System.IO.File.WriteAllText(path, json);

            CreateBuildingP(parameters);
        }

        public void CreateBuilding(XParameters parameters, string projectId)
        {
            string path = GetFolderPath(projectId) + projectId + ".ifc";
            model = IfcStore.Open(path);
            CreateBuildingP(parameters);
            model.SaveAs(path, StorageType.Ifc);
            model.Close();
        }

        public string CreateOBJFile(string projectId)
        {
            string modelPath = GetFolderPath(projectId) + projectId + ".ifc";
            model = IfcStore.Open(modelPath);
            return CreateOBJFile(model, projectId);

            
        }
        public string CreateOBJFile(IfcStore model, string projectId)
        {
            this.model = model;
            IfcBuilding ifcBuilding = model.Instances.OfType<IfcBuilding>().FirstOrDefault();
            if (ifcBuilding != null)
            {
                building = new XPreviewBuilding(ifcBuilding);

                XPreviewElement.vertexCount = 1;
                var context = new Xbim3DModelContext(model);
                context.CreateContext();
                string objfile = building.GetPreviewObjGroup(context);
                string path = GetFolderPath(projectId) + "pegasus.obj";
                System.IO.File.WriteAllText(path, objfile);

                // Create the material template library by scanning for unit colors
                string mtlfile = string.Empty;
                foreach (UnitParameters unit in parameters.UnitDefinitions)
                {
                    string unitType = unit.UnitType.Replace(' ', '_');
                    mtlfile += "newmtl " + unit.UnitType.Replace(' ', '_') + Environment.NewLine;
                    System.Drawing.Color color1 = System.Drawing.ColorTranslator.FromHtml(unit.Color);
                    mtlfile += "kd " + color1.R / 255.0 + ", " + color1.G / 255.0 + ", " + color1.B / 255.0 + Environment.NewLine;
                }
                mtlfile += "newmtl corridor" + Environment.NewLine;
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml("#afafaf");
                mtlfile += "kd " + color.R / 255.0 + ", " + color.G / 255.0 + ", " + color.B / 255.0 + Environment.NewLine;
                System.IO.File.WriteAllText(GetFolderPath(projectId) + "pegasus.mtl", mtlfile);

                model.Close();
                return path;
            }
            else return string.Empty;
        }

        public string CreateIFCFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\pegasus.ifc";
            model.SaveAs(path, StorageType.Ifc);
            return path;
        }

        public string CreateSVGFile(string projectId)
        {
            string modelPath = GetFolderPath(projectId) + projectId + ".ifc";
            model = IfcStore.Open(modelPath);
            IfcBuilding ifcBuilding = model.Instances.OfType<IfcBuilding>().FirstOrDefault();
            if (ifcBuilding != null)
            {
                building = new XPreviewBuilding(ifcBuilding);
                string svg = building.GetSVGstring();
                string path = GetFolderPath(projectId) + "pegasus.svg";
                System.IO.File.WriteAllText(path, svg);

                model.Close();

                return path;
            }
            else return string.Empty;
        }

        public XTreeNode GetModelTree(string projectId, bool serialize = false)
        {
            string modelPath = GetFolderPath(projectId) + projectId + ".ifc";
            model = IfcStore.Open(modelPath);

            XTreeNode rootNode = new XTreeNode("Project");
            XTreeNode buildingsNode = new XTreeNode("Buildings");
            rootNode.ChildNodes.Add(buildingsNode);

            foreach (IfcBuilding ifcBuilding in model.Instances.OfType<IfcBuilding>())
            {
                building = new XPreviewBuilding(ifcBuilding);

                buildingsNode.ChildNodes.Add(building.GetModelTree());
            }

            model.Close();

            if (serialize)
            {
                string json = JsonConvert.SerializeObject(rootNode, Formatting.Indented);
                string treePath = GetFolderPath(projectId) + "pegasustree.json";
                System.IO.File.WriteAllText(treePath, json);

            }
            return rootNode;
        }


        public List<XTreeNode> CreateProjectTree(string projectId)
        {
            List<XTreeNode> projectStructure = new List<XTreeNode>();
            try
            {
                string modelPath = GetFolderPath(projectId) + projectId + ".ifc";
                model = IfcStore.Open(modelPath);
                IfcBuilding ifcBuilding = model.Instances.OfType<IfcBuilding>().FirstOrDefault();
                if (ifcBuilding != null)
                {
                    building = new XPreviewBuilding(ifcBuilding);
                    IfcProject project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                    if (project != null)
                    {
                        IfcUtils utils = new IfcUtils();
                        XTreeNode projectNode = utils.CreateProjectHierarchy(project);
                        if (projectNode != null)
                            projectStructure.Add(projectNode);

                    }
                    model.Close();

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return projectStructure;
        }

        private string GetFolderPath(string projectId)
        {
            if (string.IsNullOrWhiteSpace(StoragePath))
                StoragePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

#if true
            string folderPath = StoragePath + "\\" + projectId + "\\";
#else
           string folderPath = System.Web.Hosting.HostingEnvironment.MapPath($"~/Content/assets/files/models/{projectId}");
#endif
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);
            return folderPath;
        }

        private void CreateBuildingP(XParameters parameters)
        {
            this.parameters = parameters;

            parameters.ReferenceLine = new XLine(new XbimPoint3D(BaseUnit(parameters.ReferenceLine.sp.X), BaseUnit(parameters.ReferenceLine.sp.Y), BaseUnit(parameters.ReferenceLine.sp.Z)),
                new XbimPoint3D(BaseUnit(parameters.ReferenceLine.ep.X), BaseUnit(parameters.ReferenceLine.ep.Y), BaseUnit(parameters.ReferenceLine.ep.Z)));

            // Scale the import data (mm) to base units
            parameters.CorridorWidth = BaseUnit(parameters.CorridorWidth);
            parameters.FloorToFloor = BaseUnit(parameters.FloorToFloor);
            parameters.ExteriorWallThickness = BaseUnit(parameters.ExteriorWallThickness);
            parameters.GroundSlabThickness = BaseUnit(parameters.GroundSlabThickness);
            parameters.InteriorWallThickness = BaseUnit(parameters.InteriorWallThickness);
            parameters.InterzoneSlabThickness = BaseUnit(parameters.InterzoneSlabThickness);
            foreach (var unit in parameters.UnitDefinitions)
            {
                unit.UnitWidth = BaseUnit(unit.UnitWidth);
                unit.UnitDepth = BaseUnit(unit.UnitDepth);
                unit.CreateProfile(parameters.InteriorWallThickness);
                foreach (var window in unit.Windows)
                {
                    window.Offset = BaseUnit(window.Offset);
                    window.Width = BaseUnit(window.Width);
                    window.Height = BaseUnit(window.Height);
                    window.TopElevation = BaseUnit(window.TopElevation);
                }

                foreach (var door in unit.Doors)
                {
                    door.Offset = BaseUnit(door.Offset);
                    door.Width = BaseUnit(door.Width);
                    door.Height = BaseUnit(door.Height);
                }
            }

            string json = JsonConvert.SerializeObject(parameters);
            System.IO.File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\pegasus.json", json);
            XParameters test = JsonConvert.DeserializeObject<XParameters>(json);

            // Make the building
            CreateBuildingStructure();
            AdjustCorridorLength();
            CreateBuildingEnvelope();
            CreateBuildingInterior();
            building.CommitAll(model);
        }

        /// <summary>
        /// Make a model containing a project and a building
        /// </summary>
        /// <returns></returns>
        private IfcStore InitializeModel(BaseUnits units = BaseUnits.Meters)
        {
            var creds = new XbimEditorCredentials
            {
                ApplicationDevelopersName = "Digital Alchemy",
                ApplicationFullName = "Simergy/Pegasus",
                ApplicationIdentifier = "hello.exe",
                ApplicationVersion = "1.0",
            };

            IfcStore model = IfcStore.Create(creds, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
            CreateProject(model, units);
            return model;
        }

        private void CreateProject(IfcStore model, BaseUnits units = BaseUnits.Meters)
        {
            using (var txn = model.BeginTransaction("Initialising..."))
            {
                var project = model.Instances.New<IfcProject>();
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = "Pegasus";

                // By default the units are mm, m2 and m3
                // Change the length unit to meters (metres)
                // Note xbim doesn't have imperial units so we might have to add a complete set of imperial units here,
                // check out IfcProjectPartial.cs
                switch (units)
                {
                    case BaseUnits.Millimeters:
                        model.Instances.OfType<IfcSIUnit>().Where(u => u.UnitType == IfcUnitEnum.LENGTHUNIT).ToList().ForEach(u => u.Prefix = null);
                        break;
                    case BaseUnits.Meters:
                        break;
                    case BaseUnits.Feet:
                        {
                            IfcUnitAssignment unitAssignment = model.Instances.New<IfcUnitAssignment>();
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.LENGTHUNIT, ConversionBasedUnit.Foot);
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.AREAUNIT, ConversionBasedUnit.SquareFoot);
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.VOLUMEUNIT, ConversionBasedUnit.CubicFoot);
                        }
                        break;
                    case BaseUnits.Inches:
                        {
                            IfcUnitAssignment unitAssignment = model.Instances.New<IfcUnitAssignment>();
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.LENGTHUNIT, ConversionBasedUnit.Inch);
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.AREAUNIT, ConversionBasedUnit.SquareFoot);
                            unitAssignment.SetOrChangeConversionUnit(IfcUnitEnum.VOLUMEUNIT, ConversionBasedUnit.CubicFoot);
                        }
                        break;
                }

                txn.Commit();
            }
        }

        /// <summary>
        /// Creates the building and buildinh stories
        /// </summary>
        private void CreateBuildingStructure()
        {
            // Create the building
            building = new XPreviewBuilding()
            {
                Name = parameters.BuildingName,
                Location = new XbimPoint3D(parameters.ReferenceLine.sp.X, parameters.ReferenceLine.sp.Y, parameters.ReferenceLine.sp.Z),
                ReferenceDirection = parameters.ReferenceLine.NormalizedVector,
                Axis = new XbimVector3D(0, 0, 1)
            };

            // Building stories
            for (int i = 0; i < parameters.NumberOfFloors; i++)
            {
                XPreviewBuildingStory story = new XPreviewBuildingStory
                {
                    Name = "Floor " + (i + 1).ToString(),
                    Location = new XbimPoint3D(0, 0, i * parameters.FloorToFloor),
                    ReferenceDirection = new XbimVector3D(1, 0, 0),
                    Axis = new XbimVector3D(0, 0, 1),
                    Container = building,
                    StoryNumber = (i + 1),
                };
            }
        }

        /// <summary>
        /// Creates the xterior walls and slabs
        /// </summary>
        private void CreateBuildingEnvelope()
        {

        }

        /// <summary>
        /// Creates the building interior
        /// </summary>
        /// <remarks>
        /// We start with a reference line which is the corridr center line and create a corridor space and then units to
        /// the right and left of the line. The interior is created with or without interior walls
        /// </remarks>
        private void CreateBuildingInterior()
        {
            foreach (XPreviewBuildingStory story in building.Containers)
            {
                CreateCorridorSpace(story);
                CreateUnits(story);
                if (parameters.LevelOfDetail == LevelOfDetail.FullDetail)
                {
                    CreateSlabs(story);
                    CreateCorridorWalls(story);
                    CreatePartitionWalls(story);
                }
            }
        }

        /// <summary>
        /// Creates the corridor space
        /// </summary>
        /// <param name="story"></param>
        private void CreateCorridorSpace(XPreviewBuildingStory story)
        {
            // Transform the line to be in building coordinates;
            XLine refLine = parameters.ReferenceLine.Transformed(building.InverseGlobalTransform);

            // Craete the profile
            XPolygon profile = CreateCorridorSpaceProfile(parameters.InteriorWallThickness);
            // Location will be at the center of the reference line
            XbimPoint3D location = refLine.MidPoint;
            XPreviewSpace space = new XPreviewSpace
            {
                Name = "Corridor " + story.StoryNumber.ToString(),
                ProfilePath = profile,
                Height = parameters.CeilingElevation,
                Location = location,
                Container = story,
                Color = "#afafaf"
            };
        }

        private XPolygon CreateCorridorSpaceProfile(double wallThickness)
        {
            double width = parameters.CorridorWidth - wallThickness;

            // Transform the line to be in building coordinates;
            XLine refLine = parameters.ReferenceLine.Transformed(building.InverseGlobalTransform);
            // Create the profile. To be consistent the profile is centered at the mid point
            // starts upper left and is oriented clockwise
            XPolygon profile = new XPolygon
            {
                new XbimPoint3D(-refLine.Length / 2, width / 2, 0),
                new XbimPoint3D(refLine.Length / 2, width / 2, 0),
                new XbimPoint3D(refLine.Length / 2, -width / 2, 0),
                new XbimPoint3D(-refLine.Length / 2, -width / 2, 0)
            };
            return profile;
        }

        private void CreateCorridorWalls(XPreviewBuildingStory story)
        {
            XbimPoint3D p1 = new XbimPoint3D(0, parameters.CorridorWidth, 0);
            XbimPoint3D p2 = new XbimPoint3D(parameters.ReferenceLine.Length, parameters.CorridorWidth, 0);

            XPolygon wallProfile = CreateWallProfile(p1.Distance(p2));

            XPreviewWall wall = new XPreviewWall
            {
                Name = "Wall ",
                Location = new XbimPoint3D(0, -parameters.CorridorWidth / 2, 0),
                ProfilePath = wallProfile.Clone(),
                Thickness = parameters.InteriorWallThickness,
                Height = parameters.CeilingElevation,
                Container = story
            };

            wall = new XPreviewWall
            {
                Name = "Wall ",
                Location = new XbimPoint3D(0, parameters.CorridorWidth / 2, 0),
                ProfilePath = wallProfile.Clone(),
                Thickness = parameters.InteriorWallThickness,
                Height = parameters.CeilingElevation,
                Container = story
            };
        }

        /// <summary>
        /// Creates the partition walls between the units. Not surprisingly uses similar logic to
        /// creating units
        /// </summary>
        /// <param name="story"></param>
        private void CreatePartitionWalls(XPreviewBuildingStory story)
        {
            UnitParameters firstUnit = unitsToCreate.First();
            XLine refLine = parameters.ReferenceLine.Transformed(building.InverseGlobalTransform);
            XbimVector3D cross = refLine.NormalizedVector.CrossProduct(new XbimVector3D(0, 0, 1));
            // The reference point on the center line from which the insertion points will be calculated
            XbimPoint3D referencePoint = new XbimPoint3D(refLine.sp.X + (firstUnit.UnitWidth - parameters.InteriorWallThickness / 2), refLine.sp.Y, refLine.sp.Z);

            int unitCount = 0;
            int wallcount = 0;
            foreach (UnitParameters unit in unitsToCreate)
            {
                if (unitCount != unitsToCreate.Count - 1)
                {
                    // the length of the wall is going to be the minimum of the dwpth of the two units the wall is between
                    double length = Math.Min(unit.UnitDepth, unitsToCreate[unitCount + 1].UnitDepth);
                    if (parameters.CorridorMode != CorridorMode.Left)
                    {
                        // Right of the corridor
                        XbimPoint3D location = cross.GetPoint(referencePoint, parameters.CorridorWidth / 2 + parameters.InteriorWallThickness / 2);
                        XPreviewWall wall = new XPreviewWall
                        {
                            Name = "Wall " + (story.StoryNumber * 100 + wallcount).ToString(),
                            Location = location,
                            Container = story,
                            ProfilePath = CreateWallProfile(length),
                            Height = parameters.CeilingElevation,
                            ReferenceDirection = new XbimVector3D(0, -1, 0)
                        };
                        wallcount++;
                    }
                    if (parameters.CorridorMode != CorridorMode.Right)
                    {
                        // Left of the corridor
                        XbimPoint3D location = cross.GetPoint(referencePoint, -parameters.CorridorWidth / 2 - parameters.InteriorWallThickness / 2);
                        XPreviewWall wall = new XPreviewWall
                        {
                            Name = "Wall " + (story.StoryNumber * 100 + wallcount).ToString(),
                            Location = location,
                            Container = story,
                            ProfilePath = CreateWallProfile(length),
                            Height = parameters.CeilingElevation,
                            ReferenceDirection = new XbimVector3D(0, 1, 0),
                        };
                        wallcount++;
                    }

                    // Increment the location. Must use next unit width too as units may be different sizes
                    if (unitCount != unitsToCreate.Count - 1)
                    {
                        UnitParameters nextUnit = unitsToCreate[unitCount + 1];
                        XbimPoint3D delta = new XbimPoint3D(nextUnit.UnitWidth, 0, 0);
                        referencePoint = XbimPoint3D.Add(referencePoint, delta);
                    }
                }
                unitCount++;
            }

        }

        /// <summary>
        /// Creates the internal wall profile from two points on the cenbter line and a thickness
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private XPolygon CreateWallProfile(double length)
        {
            XPolygon wallProfile = new XPolygon
            {
                new XbimPoint3D(0, -parameters.InteriorWallThickness / 2, 0),
                new XbimPoint3D(length, -parameters.InteriorWallThickness / 2, 0),
                new XbimPoint3D(length, parameters.InteriorWallThickness / 2, 0),
                new XbimPoint3D(0, parameters.InteriorWallThickness / 2, 0)
            };

            return wallProfile;
        }

        /// <summary>
        /// Creates the units.
        /// </summary>
        /// <remarks>
        /// The units are inserted at their bounding box center, so what we need to do is calculate where that center should be.
        /// Take a point along the corridor center line, using the cross product vector get a point to the right and to the left
        /// The units profile is already adjusted for whether we are FullDetail or not.
        /// </remarks>
        /// <param name="story"></param>
        private void CreateUnits(XPreviewBuildingStory story)
        {
            UnitParameters firstUnit = unitsToCreate.First();
            XLine refLine = parameters.ReferenceLine.Transformed(building.InverseGlobalTransform);
            XbimVector3D cross = refLine.NormalizedVector.CrossProduct(new XbimVector3D(0, 0, 1));
            // The reference point on the center line from which the insertion points will be calculated
            XbimPoint3D referencePoint = new XbimPoint3D(refLine.sp.X + (firstUnit.UnitWidth - parameters.InteriorWallThickness) / 2, refLine.sp.Y, refLine.sp.Z);

            int unitNumber = 1;     // The sequence number used to generate the unit name (incremented for every unit)
            int unitCount = 0;      // The count of units to create, increment every two units (for both sides)
            foreach (UnitParameters unit in unitsToCreate)
            {
                if (parameters.CorridorMode != CorridorMode.Left)
                {
                    XbimPoint3D location = cross.GetPoint(referencePoint, (unit.UnitDepth + parameters.CorridorWidth) / 2);
                    // Create units to the right
                    XPreviewSpace space = new XPreviewSpace
                    {
                        Name = "Unit " + (story.StoryNumber * 100 + unitNumber).ToString(),  // "101, 102, 103" etc.
                        ProfilePath = unit.Profile.Clone(),
                        LongName = unit.UnitType,
                        Height = parameters.CeilingElevation,
                        Location = location,
                        ReferenceDirection = new XbimVector3D(-1, 0, 0),
                        Container = story,
                        Color = unit.Color,
                    };
                    unitNumber++;
                }
                if (parameters.CorridorMode != CorridorMode.Right)
                {
                    XbimPoint3D location = cross.GetPoint(referencePoint, -(unit.UnitDepth + parameters.CorridorWidth) / 2);
                    // Create units to the left
                    XPreviewSpace space = new XPreviewSpace
                    {
                        Name = "Unit " + (story.StoryNumber * 100 + unitNumber).ToString(),  // "101, 102, 103" etc.
                        ProfilePath = unit.Profile.Clone(),
                        LongName = unit.UnitType,
                        Height = parameters.CeilingElevation,
                        Location = location,
                        Container = story,
                        Color = unit.Color
                    };
                    unitNumber++;
                }

                // Increment the location. Must use next unit width too as units may be different sizes
                if (unitCount != unitsToCreate.Count - 1)
                {
                    UnitParameters nextUnit = unitsToCreate[unitCount + 1];
                    XbimPoint3D delta = new XbimPoint3D(unit.UnitWidth / 2 + nextUnit.UnitWidth / 2, 0, 0);
                    referencePoint = XbimPoint3D.Add(referencePoint, delta);
                }
                unitCount++;
            }
        }

        private void CreateSlabs(XPreviewBuildingStory story)
        {
            // For which we need the total footprint as defined by the units. But we can't boolean them together
            // because there are gaps between the spaces now - arrrghhhh!

            // Make the corridor polygon and then make each unit and boolean add them.
            XPolygon corridor = CreateCorridorSpaceProfile(0).Transformed(story.Elements.First().LocalTransform);
            List<XPolygon> results = new List<XPolygon>() { corridor };
            foreach (XPreviewSpace space in story.Elements.Where(elem => elem is XPreviewSpace space && space.LongName != null))
            {
                // Find the relating unit definition so we can create a polygon the extents of width and depth
                UnitParameters unitdef = unitsToCreate.Find(u => u.UnitType == space.LongName);
                if (unitdef != null)
                {
                    XPolygon fullProfile = unitdef.CreateFullSizeProfile().Transformed(space.LocalTransform);
                    results = XPolygon.Clip(ClipType.ctUnion, results, new List<XPolygon>() { fullProfile });
                }
            }

            // We will place the slab at 0,0,0 on the story
            XPreviewSlab slab = new XPreviewSlab
            {
                Name = "Slab " + story.StoryNumber,
                ProfilePath = results.First(),
                Thickness = parameters.InterzoneSlabThickness,
                Container = story,
                Height = -parameters.InterzoneSlabThickness
            };
        }

        #region Making the garage

        public void CreateGarage(XParameters parameters)
        {
            string path = GetFolderPath(parameters.ProjectId) + parameters.ProjectId + ".ifc";
            model = IfcStore.Open(path);

            CreateGarageP(parameters);

            model.SaveAs(path, StorageType.Ifc);
            model.Close();
        }

        private void CreateGarageP(XParameters parameters)
        {
            XPreviewBuilding garage = CreateGarageStructure(parameters);
            garage.CommitAll(model);
        }

        private XPreviewBuilding CreateGarageStructure(XParameters parameters)
        {
            XbimPoint3D location = parameters.Perimeter.First();
            XPolygon perimeter = parameters.Perimeter.Normalized(location);
            // Create the building
            XPreviewBuilding garage = new XPreviewBuilding()
            {
                Name = parameters.BuildingName,
                Location = location,
                ReferenceDirection = new XbimVector3D(1, 0, 0),
                Axis = new XbimVector3D(0, 0, 1)
            };

            // Building stories
            for (int i = 0; i < parameters.NumberOfFloors; i++)
            {
                XPreviewBuildingStory story = new XPreviewBuildingStory
                {
                    Name = "Floor " + (i + 1).ToString(),
                    Location = new XbimPoint3D(0, 0, i * parameters.FloorToFloor),
                    ReferenceDirection = new XbimVector3D(1, 0, 0),
                    Axis = new XbimVector3D(0, 0, 1),
                    Container = garage,
                    StoryNumber = (i + 1),
                };
            }

            return garage;
        }

        #endregion

        #region Miscellaneous functions

        private static double Feet(double value) => value * 304.8;

        /// <summary>
        /// Adjusts the corridor length to fit whole units 
        /// </summary>
        /// <remarks>
        /// The algorithm is a bit crude, it just extends to fit the last unit. but good enough for now
        /// </remarks>
        private void AdjustCorridorLength()
        {
            unitsToCreate = new List<UnitParameters>();

            double minimum = Feet(2);
            // The length of one set of each unit
            double unitPatternLength = parameters.UnitDefinitions.Sum(unit => unit.UnitWidth);
            // Number of repeating units
            int numRepeats = (int)(parameters.ReferenceLine.Length / unitPatternLength);
            // Whats left over
            double remainder = parameters.ReferenceLine.Length - numRepeats * unitPatternLength;

            for (int i = 0; i < numRepeats; i++) unitsToCreate.AddRange(parameters.UnitDefinitions);

            int nAdded = 0;
            do
            {
                if (remainder < minimum)
                {
                    // Less than minimum tolerance left so shrink the reference line  
                    parameters.ReferenceLine.ep = parameters.ReferenceLine.NormalizedVector.GetPoint(parameters.ReferenceLine.sp, parameters.ReferenceLine.Length - remainder);
                    remainder = 0;
                }
                else if (remainder < parameters.UnitDefinitions[nAdded].UnitWidth)
                {
                    // Not quite long enough to fit the next unit but can be extended to fit
                    parameters.ReferenceLine.ep = parameters.ReferenceLine.NormalizedVector.GetPoint(parameters.ReferenceLine.sp, parameters.ReferenceLine.Length + (parameters.UnitDefinitions[nAdded].UnitWidth) - remainder);
                    unitsToCreate.Add(parameters.UnitDefinitions[nAdded]);
                    remainder = 0;
                }
                else
                {
                    // plenty of room!
                    remainder = remainder - parameters.UnitDefinitions[nAdded].UnitWidth;
                    unitsToCreate.Add(parameters.UnitDefinitions[nAdded]);
                    nAdded++;
                }
            }
            while (remainder != 0);
        }

        #endregion
    }
}
