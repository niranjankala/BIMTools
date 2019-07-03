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
using Xbim.IO;

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
                XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Dispose();

            XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel
                 = IfcStore.Create(XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            if (XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel != null)
                XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel.Dispose();

            XbimInvestigator.Common.ApplicationManager.Instance.CurrentModel
                 = IfcStore.Create(XbimInvestigator.Common.ApplicationManager.Instance.ApplicationEditorCredentials, Xbim.Common.Step21.XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

        }
    }
}
