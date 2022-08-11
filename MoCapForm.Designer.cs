namespace Optitracker
{
    partial class MoCapForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuProject = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTreadmill = new System.Windows.Forms.ToolStripMenuItem();
            this.randomWalkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.formVisualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuProject,
            this.formVisualToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(783, 24);
            this.menuStrip1.TabIndex = 23;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuProject
            // 
            this.mnuProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuTreadmill,
            this.randomWalkToolStripMenuItem});
            this.mnuProject.Name = "mnuProject";
            this.mnuProject.Size = new System.Drawing.Size(56, 20);
            this.mnuProject.Text = "Project";
            // 
            // mnuTreadmill
            // 
            this.mnuTreadmill.Name = "mnuTreadmill";
            this.mnuTreadmill.Size = new System.Drawing.Size(145, 22);
            this.mnuTreadmill.Text = "Treadmill";
            this.mnuTreadmill.Click += new System.EventHandler(this.mnuTreadmill_Click);
            // 
            // randomWalkToolStripMenuItem
            // 
            this.randomWalkToolStripMenuItem.Name = "randomWalkToolStripMenuItem";
            this.randomWalkToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.randomWalkToolStripMenuItem.Text = "RandomWalk";
            this.randomWalkToolStripMenuItem.Click += new System.EventHandler(this.randomWalkToolStripMenuItem_Click);
            // 
            // tabControl
            // 
            this.tabControl.Location = new System.Drawing.Point(12, 27);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(759, 559);
            this.tabControl.TabIndex = 26;
            // 
            // formVisualToolStripMenuItem
            // 
            this.formVisualToolStripMenuItem.Name = "formVisualToolStripMenuItem";
            this.formVisualToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.formVisualToolStripMenuItem.Text = "FormVisual";
            this.formVisualToolStripMenuItem.Click += new System.EventHandler(this.formVisualToolStripMenuItem_Click);
            // 
            // MoCapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 598);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MoCapForm";
            this.Text = "MoCapForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MoCapForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuProject;
        private System.Windows.Forms.ToolStripMenuItem mnuTreadmill;
        private System.Windows.Forms.ToolStripMenuItem randomWalkToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.ToolStripMenuItem formVisualToolStripMenuItem;
    }
}