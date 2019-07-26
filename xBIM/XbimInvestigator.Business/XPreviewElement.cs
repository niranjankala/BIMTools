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
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;
using Xbim.Common.XbimExtensions;

namespace XbimInvestigator.Business
{
    /// <summary>
    /// 
    /// </summary>
    class XPreviewElement : XPreviewBase
    {
        /// <summary>
        /// Fill color
        /// </summary>
        public string Color { get; set; } = "#afafaf";

        public XPreviewElement()
            : base()
        {

        }

        public XPreviewElement(IfcProduct ifcEntity)
            : base(ifcEntity)
        {
            Color = ifcEntity.Description;
            ProfilePath = LoadProfilePath();

            XPolygon LoadProfilePath()
            {
                XPolygon profile = new XPolygon();
                foreach (var rep in ifcEntity.Representation.Representations)
                {
                    if (rep.RepresentationIdentifier == "Plan" && rep.RepresentationType == "Curve2D")
                    {
                        foreach (var item in rep.Items)
                        {
                            if (item is IfcPolyline polyline)
                            {
                                foreach (var point in polyline.Points)
                                {
                                    profile.Add(new XbimPoint3D(point.X, point.Y, point.Z));
                                }
                            }
                        }
                    }
                }

                return profile;
            }
        }

        public IfcElement Element
        {
            get => Product as IfcElement;
            set => Product = value;
        }

        public XPreviewContainer Container
        {
            get => container;
            set
            {
                if (container != null) container.Elements.Remove(this);
                container = value;
                if (container != null) container.Elements.Add(this);
            }

        }

        public override XbimMatrix3D GlobalTransform
        {
            get
            {
                if (container != null) return XbimMatrix3D.Multiply(LocalTransform, container.GlobalTransform);
                else return LocalTransform;
            }
        }

        /// <summary>
        /// In simergy everything has a profile and the profile is saved but in xbim we don't have profiles saved
        /// oh unless the 3d geometry is a swept area then we do - aha! 
        /// </summary>
        public XPolygon ProfilePath { get; set; }
        
        /// <summary>
        /// The height of the extrusion - can be the same as the height of an element (wall or space) or could be different
        /// for eaxmple we extrude slabs downwards
        /// </summary>
        public double Height { get; set; }

        protected override IfcProductDefinitionShape CreateRepresentations(IfcStore model)
        {
            // Use the profile path to create swept area - simplest form of a 3d
            var polyline = model.Instances.New<IfcPolyline>();
            ProfilePath.ForEach(p => polyline.Points.Add(model.Instances.New<IfcCartesianPoint>(cp => cp.SetXYZ(p.X, p.Y, p.Z))));
            polyline.Points.Add(model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(ProfilePath.First().X, ProfilePath.First().Y, ProfilePath.First().Z)));
            var profile = model.Instances.New<IfcArbitraryClosedProfileDef>();
            profile.OuterCurve = polyline;
            profile.ProfileType = IfcProfileTypeEnum.AREA;
            var origin = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));

            var path = model.Instances.New<IfcPolyline>();
            ProfilePath.ForEach(p => path.Points.Add(model.Instances.New<IfcCartesianPoint>(cp => cp.SetXYZ(p.X, p.Y, p.Z))));

            // Body representation
            var body = model.Instances.New<IfcExtrudedAreaSolid>();
            body.Depth = Math.Abs(Height);
            body.ExtrudedDirection = model.Instances.New<IfcDirection>(d => d.SetXYZ(0, 0, 1*Math.Sign(Height)));
            body.SweptArea = profile;
            body.Position = model.Instances.New<IfcAxis2Placement3D>(p => p.Location = origin);
            body.Position.Axis = model.Instances.New<IfcDirection>(d => d.SetXYZ(0, 0, 1));
            body.Position.RefDirection = model.Instances.New<IfcDirection>(d => d.SetXYZ(1, 0, 0));

            // Shape representation
            var bodyShape = model.Instances.New<IfcShapeRepresentation>();
            bodyShape.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            bodyShape.RepresentationType = "SweptSolid";
            bodyShape.RepresentationIdentifier = "Body";
            bodyShape.Items.Add(body);

            var planShape = model.Instances.New<IfcShapeRepresentation>();
            planShape.ContextOfItems = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            planShape.RepresentationType = "Curve2D";
            planShape.RepresentationIdentifier = "Plan";
            planShape.Items.Add(path);

            // Product definition
            var rep = model.Instances.New<IfcProductDefinitionShape>();
            rep.Representations.Add(bodyShape);
            rep.Representations.Add(planShape);
            return rep;
        }

        protected override void DoCommitAll(IfcStore model)
        {
            Product.Representation = CreateRepresentations(model);
            base.DoCommitAll(model);
        }

        #region OBJ file generarion

        public static int vertexCount = 1;

        public virtual bool TryGetPreviewObjGroup(Xbim3DModelContext context, out string group)
        {
            try
            {
                group = GetPreviewObjGroup(context);
                return true;
            }
            catch (Exception e)
            {
                group = string.Empty;
                return false;
            }
        }

        public virtual string GetPreviewObjGroup(Xbim3DModelContext context)
        {
            //if (Name != "009") return "";
            double scale = 1;
            string group = string.Empty;

            using (var geomReader = Product.Model.GeometryStore.BeginRead())
            {
                XbimShapeInstance xxx = context.ShapeInstances().ToList().Find(gg => gg.IfcProductLabel == Product.EntityLabel);
                if (xxx != null)
                {
                    var geometry = context.ShapeGeometry(xxx);
                    var data = ((IXbimShapeGeometryData)geometry).ShapeData;
                    using (var stream = new System.IO.MemoryStream(data))
                    {
                        using (var reader = new System.IO.BinaryReader(stream))
                        {
                            XbimShapeTriangulation tris = reader.ReadShapeTriangulation();
                            //if (tris.Faces.Count != 6) return "";

                            group += "g " + Name + ":" + Product.GlobalId + Environment.NewLine;
                            group += "s 1" + Environment.NewLine;

                            // Vertices first - we will do a unique set of vertices per face as that
                            // produces better rendering results (I think)
                            List<TPoint> allPoints = new List<TPoint>();          // All the vertices (for all faces)
                            List<List<int>> faceIndices = new List<List<int>>();    // Indices by face
                            List<TPoint> vertices = new List<TPoint>();

                            foreach (var v in tris.Vertices)
                            {
                                vertices.Add(new TPoint(v.X, v.Y, v.Z));
                            }

                            foreach (XbimFaceTriangulation f in tris.Faces)
                            {
                                XbimVector3D normal = new XbimVector3D(f.Normals.First().Normal.X, f.Normals.First().Normal.Y, f.Normals.First().Normal.Z);
                                List<int> indices = new List<int>();        //
                                List<TPoint> points = new List<TPoint>();
                                for (int i = 0; i < f.Indices.Count; i++)
                                {
                                    XbimPoint3D xp = tris.Vertices[f.Indices[i]];
                                    TPoint existing = points.Find(p => p.X == xp.X && p.Y == xp.Y && p.Z == xp.Z);
                                    if (existing != null) indices.Add(allPoints.LastIndexOf(existing));
                                    else
                                    {
                                        TPoint p = new TPoint(xp.X, xp.Y, xp.Z);
                                        p.UserData = normal;
                                        allPoints.Add(p);
                                        points.Add(p);
                                        indices.Add(allPoints.Count - 1);
                                    }
                                }
                                faceIndices.Add(indices);
                            }

                            allPoints.ForEach(p =>
                            {
                                p.Scale(scale);
                                TPoint pt = p.Transformed(GlobalTransform);
                                group += "v " + Math.Round(pt.X, 3) + " " + Math.Round(pt.Y, 3) + " " + Math.Round(pt.Z, 3) + Environment.NewLine;
                            });

                            allPoints.ForEach(p =>
                            {
                                XbimVector3D normal = (XbimVector3D)p.UserData;
                                XbimVector3D vt = GlobalTransform.Transform(normal);
                                group += "vn " + Math.Round(vt.X, 3) + " " + Math.Round(vt.Y, 3) + " " + Math.Round(vt.Z, 3) + Environment.NewLine;
                            });

                            group += GetObjMaterial() + Environment.NewLine;

                            foreach (List<int> indices in faceIndices)
                            {
                                for (int i = 0; i < indices.Count; i += 3)
                                {
                                    group += "f " + (indices[i] + vertexCount).ToString() + "//" + (indices[i] + vertexCount).ToString() + " " +
                                        (indices[i + 1] + vertexCount).ToString() + "//" + (indices[i + 1] + vertexCount).ToString() + " " +
                                        (indices[i + 2] + vertexCount).ToString() + "//" + (indices[i + 2] + vertexCount).ToString() + Environment.NewLine;
                                }
                            }

                            vertexCount += allPoints.Count;
                        }
                    }
                }
            }
            return group;
        }

        protected virtual string GetObjMaterial()
        {
            return "";
        }

        #endregion OBJ file generation

        #region SVG file generation

        public  virtual bool TryGetSVGstring(out string svg)
        {
            try
            {
                svg = GetSVGstring();
                return true;
            }
            catch(Exception e)
            {
                svg = string.Empty;
                return false;
            }
        }

        public virtual string GetSVGstring()
        {
            XPolygon profile = ProfilePath.Transformed(GlobalTransform);
            profile.Name = Name;
            string svg = profile.ToSVGPath();

            return svg;
        }

        public virtual XTreeNode GetModelTree()
        {
            XTreeNode node = new XTreeNode(Name, GUID);
            return node;
        }

        #endregion

        #region TPoint

        /// <summary>
        /// SImplep point class because XbimPoint3D is a struct and therefore is not nullable and that just makes life awkward
        /// </summary>
        class TPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public object UserData { get; set; }

            public TPoint(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public void Transform(XbimMatrix3D transform)
            {
                XbimPoint3D temp = new XbimPoint3D(X, Y, Z);
                temp = transform.Transform(temp);
                X = temp.X;
                Y = temp.Y;
                Z = temp.Z;
            }

            public TPoint Transformed(XbimMatrix3D transform)
            {
                TPoint temp = new TPoint(X, Y, Z);
                temp.Transform(transform);
                return temp;
            }

            public void Scale(double scale)
            {
                X *= scale;
                Y *= scale;
                Z *= scale;
            }
        }

        #endregion TPoint
    }
}
