using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xbim.Ifc;
using Xbim.Ifc2x3.Interfaces;
using Xbim.IO;
using XbimInvestigator.Business;

namespace XbimInvestigator
{
    public partial class IFCAnalyser : Form
    {
        public IFCAnalyser()
        {
            InitializeComponent();
            //Set these from user preferences
            XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials.EditorsFamilyName = "Singh";
            XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials.EditorsGivenName = "Niranjan";
            XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials.EditorsOrganisationName = "Independent Architecture";
            XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel
                 = IfcStore.Create(XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);


        }

        private void btnCreateNewProject_Click(object sender, EventArgs e)
        {
            if (XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel != null)
            {
                XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Close();
                XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Dispose();

            }

            XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel
                 = IfcStore.Create(XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
            PopulateControls();
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "IFC files (*.ifc)|*.ifc";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel != null)
                    {
                        XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Close();
                        XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Dispose();

                    }

                    XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel
                         = IfcStore.Open(openFileDialog.FileName);
                    PopulateControls();
                    IXModelCreator creator = new XModelCreator();
                    creator.CreateOBJFile(XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel,System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                }
            }


        }

        private void PopulateControls()
        {
            txtObjDetails.Text = "";
            treeView1.Nodes.Clear();
            //Load Tree
            var project = XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Instances.FirstOrDefault<IIfcProject>();
            if (project != null)
            {
                TreeNode projectNode = CreateNode(project);
                projectNode.Tag = project.GlobalId;
                PrintHierarchy(project, projectNode);
                treeView1.Nodes.Add(projectNode);
                treeView1.ExpandAll();
                projectNode.EnsureVisible();
            }

        }

        TreeNode CreateNode(IIfcObjectDefinition project)
        {
            TreeNode projectNode = new TreeNode(project.Name);
            projectNode.Tag = project.GlobalId;
            return projectNode;

        }

        private void PrintHierarchy(IIfcObjectDefinition o, TreeNode parentNode)
        {
            //textBox1.Text += ($"{GetIndent(level)}{o.Name} [{o.GetType().Name}]");
            var spatialElement = o as IIfcSpatialStructureElement;
            if (spatialElement != null)
            {
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                {
                    parentNode.Nodes.Add(CreateNode(element));
                    //textBox1.Text += ($"{GetIndent(level)}    ->{element.Name} [{element.GetType().Name}]");
                }
            }

            foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
            {
                spatialElement = item as IIfcSpatialStructureElement;
                if (spatialElement != null)
                {
                    TreeNode childNode = CreateNode(item);
                    parentNode.Nodes.Add(childNode);
                    PrintHierarchy(item, childNode);
                }
            }
        }

        private void btnSaveProject_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "IFC files (*.ifc)|*.ifc|IFC Zip files (*.ifczip)|*.ifczip|IFC XML files (*.ifcxml)|*.ifcxml|Xbim files (*.xbim)|*.xbim";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel != null)
                    {
                        XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.SaveAs(saveFileDialog.FileName);
                    }
                }
            }
        }
        //private void PrintHierarchy(IIfcObjectDefinition o, int level)
        //{
        //    textBox1.Text += ($"{GetIndent(level)}{o.Name} [{o.GetType().Name}]");

        //    var spatialElement = o as IIfcSpatialStructureElement;
        //    if (spatialElement != null)
        //    {
        //        var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
        //        foreach (var element in containedElements)
        //            textBox1.Text += ($"{GetIndent(level)}    ->{element.Name} [{element.GetType().Name}]");
        //    }

        //    foreach (var item in o.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
        //        PrintHierarchy(item, level + 1);
        //}

        //private string GetIndent(int level)
        //{
        //    var indent = "";
        //    for (int i = 0; i < level; i++)
        //        indent += "  ";
        //    return indent;
        //}
    }
}
