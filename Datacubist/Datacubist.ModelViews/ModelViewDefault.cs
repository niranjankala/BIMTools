using System;
using System.Collections.Generic;
using System.Text;
using IfcIdLib = Datacubist.simplebim.Developer.Core.Libraries.DC.Classifications;
using Datacubist.simplebim.Developer.Core.DataCube.DataModelView;
using System.Diagnostics;
using Datacubist.Common;

namespace ModelViews
{
    public class ModelViewDefault : Datacubist.simplebim.Developer.Core.DataCube.DataModelView.ModelView, Datacubist.simplebim.Developer.Desktop.IModelViewModule
    {
        private System.Guid _defaultWorkspaceGuid;
        private Datacubist.simplebim.Developer.Core.DataCube.Identification.SemanticClassification _allowedRoleClassification;

        public ModelViewDefault()
        {
            _defaultWorkspaceGuid = Workspace_Keys.WorkspaceDashboard;
        }

        #region "IModelViewModule implementation"

        public System.Guid Guid
        {
            get { return new System.Guid(ApplicationConstants.Guids.ModelViews.Default); }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public string Name
        {
            get { return "Model View"; }
        }

        public string Description
        {
            get { return "Model View for data"; }
        }

        public System.Drawing.Bitmap Image
        {
            get { return null; }
        }

        public System.Guid DefaultWorkspaceGuid
        {
            get { return _defaultWorkspaceGuid; }

        }

        public Datacubist.simplebim.Developer.Core.DataCube.DataModelView.ModelView Model1
        {
            get { return this; }
        }

        public void SetPlayer(Datacubist.simplebim.Developer.Desktop.Players.SingleModelPlayer player)
        {
            // Nothing - the reference to a model player is not needed in this model view
        }

        Datacubist.simplebim.Developer.Core.DataCube.DataModelView.ModelView Datacubist.simplebim.Developer.Core.Runtime.IModelViewModule.ModelView
        {
            get { return Model1; }
        }

        #endregion

        #region "Model view overrides"

        protected override void BeforeCreateTables()
        {
            _allowedRoleClassification = IfcIdLib.GetClassificationIdentity(IfcIdLib.KEY_ALLOWED, this.Model);

        }

        protected override Datacubist.simplebim.Developer.Core.DataCube.DataModelView.TableView GetNewTable(Datacubist.simplebim.Developer.Core.DataCube.DataModel.Table modelTable)
        {

            // No special filtering        
            return base.GetNewTable(modelTable);
            //return new TableView(this, modelTable);

        }

        protected override void AfterCreateTables(Datacubist.simplebim.Developer.Core.DataCube.DataModelView.ModelViewInscopeTableCollection inFocusTables)
        {
            // Set the 3D allowed role classification to all tables in scope
            foreach (Datacubist.simplebim.Developer.Core.DataCube.DataModelView.TableView table in inFocusTables)
            {
                this.TableClassifications.Add(_allowedRoleClassification, table);
                //Debug.WriteLine(table.Name);
            }
        }

        public override Datacubist.simplebim.Developer.Core.DataCube.Query.PickSolverCollection PickSolvers
        {
            get
            {

                return null;
            }
        }


        public override Datacubist.simplebim.Developer.Core.DataCube.Query.DragSolver DefaultDragSolver
        {
            get
            {

                Datacubist.simplebim.Developer.Core.DataCube.Query.TableClassificationQueryItem classificationQuery = default(Datacubist.simplebim.Developer.Core.DataCube.Query.TableClassificationQueryItem);

                // Create the classification query
                classificationQuery = new Datacubist.simplebim.Developer.Core.DataCube.Query.TableClassificationQueryItem(_allowedRoleClassification);


                return new Datacubist.simplebim.Developer.Core.DataCube.Query.DragSolver(classificationQuery);
            }
        }

        #endregion

    }
}
