namespace XbimInvestigator
{
    partial class IFCAnalyser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnCreateNewProject = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnFilterObjects = new System.Windows.Forms.Button();
            this.btnOpenProject = new System.Windows.Forms.Button();
            this.txtObjDetails = new System.Windows.Forms.TextBox();
            this.gbxDescription = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btnSaveProject = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gbxDescription.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCreateNewProject
            // 
            this.btnCreateNewProject.Location = new System.Drawing.Point(12, 19);
            this.btnCreateNewProject.Name = "btnCreateNewProject";
            this.btnCreateNewProject.Size = new System.Drawing.Size(102, 23);
            this.btnCreateNewProject.TabIndex = 0;
            this.btnCreateNewProject.Text = "New Project";
            this.btnCreateNewProject.UseVisualStyleBackColor = true;
            this.btnCreateNewProject.Click += new System.EventHandler(this.btnCreateNewProject_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSaveProject);
            this.groupBox1.Controls.Add(this.btnFilterObjects);
            this.groupBox1.Controls.Add(this.btnOpenProject);
            this.groupBox1.Controls.Add(this.btnCreateNewProject);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 55);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IFC Operations";
            // 
            // btnFilterObjects
            // 
            this.btnFilterObjects.Location = new System.Drawing.Point(686, 19);
            this.btnFilterObjects.Name = "btnFilterObjects";
            this.btnFilterObjects.Size = new System.Drawing.Size(102, 23);
            this.btnFilterObjects.TabIndex = 2;
            this.btnFilterObjects.Text = "Filter Objects";
            this.btnFilterObjects.UseVisualStyleBackColor = true;
            // 
            // btnOpenProject
            // 
            this.btnOpenProject.Location = new System.Drawing.Point(120, 19);
            this.btnOpenProject.Name = "btnOpenProject";
            this.btnOpenProject.Size = new System.Drawing.Size(102, 23);
            this.btnOpenProject.TabIndex = 1;
            this.btnOpenProject.Text = "Open IFC Project";
            this.btnOpenProject.UseVisualStyleBackColor = true;
            this.btnOpenProject.Click += new System.EventHandler(this.btnOpenProject_Click);
            // 
            // txtObjDetails
            // 
            this.txtObjDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtObjDetails.Location = new System.Drawing.Point(3, 16);
            this.txtObjDetails.Multiline = true;
            this.txtObjDetails.Name = "txtObjDetails";
            this.txtObjDetails.Size = new System.Drawing.Size(526, 376);
            this.txtObjDetails.TabIndex = 0;
            // 
            // gbxDescription
            // 
            this.gbxDescription.Controls.Add(this.txtObjDetails);
            this.gbxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbxDescription.Location = new System.Drawing.Point(268, 55);
            this.gbxDescription.Name = "gbxDescription";
            this.gbxDescription.Size = new System.Drawing.Size(532, 395);
            this.gbxDescription.TabIndex = 4;
            this.gbxDescription.TabStop = false;
            this.gbxDescription.Text = "Object Details";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.treeView1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Location = new System.Drawing.Point(0, 55);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(268, 395);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Project Structure";
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 16);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(262, 376);
            this.treeView1.TabIndex = 3;
            // 
            // btnSaveProject
            // 
            this.btnSaveProject.Location = new System.Drawing.Point(240, 19);
            this.btnSaveProject.Name = "btnSaveProject";
            this.btnSaveProject.Size = new System.Drawing.Size(102, 23);
            this.btnSaveProject.TabIndex = 3;
            this.btnSaveProject.Text = "Save Project";
            this.btnSaveProject.UseVisualStyleBackColor = true;
            this.btnSaveProject.Click += new System.EventHandler(this.btnSaveProject_Click);
            // 
            // IFCAnalyser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.gbxDescription);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "IFCAnalyser";
            this.Text = "IFC Analyser";
            this.groupBox1.ResumeLayout(false);
            this.gbxDescription.ResumeLayout(false);
            this.gbxDescription.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateNewProject;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnOpenProject;
        private System.Windows.Forms.Button btnFilterObjects;
        private System.Windows.Forms.TextBox txtObjDetails;
        private System.Windows.Forms.GroupBox gbxDescription;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnSaveProject;
    }
}

