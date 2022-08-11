namespace Optitracker.GUI
{
    partial class MoCap
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpBodymodel = new System.Windows.Forms.GroupBox();
            this.lblIgnoreZ = new System.Windows.Forms.Label();
            this.nudIgnoreZ = new System.Windows.Forms.NumericUpDown();
            this.chkTrack = new System.Windows.Forms.CheckBox();
            this.lblCalibration = new System.Windows.Forms.Label();
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.cmdCalibrateBody = new System.Windows.Forms.Button();
            this.grpMocapSystem = new System.Windows.Forms.GroupBox();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.cmdConnectSystem = new System.Windows.Forms.Button();
            this.lstKnownMarkers = new System.Windows.Forms.ListBox();
            this.lblKnownMarkers = new System.Windows.Forms.Label();
            this.scene = new MDOL.Scene.SceneControl();
            this.lstNMarkers = new System.Windows.Forms.ListBox();
            this.lblNMarkers = new System.Windows.Forms.Label();
            this.grpBodymodel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIgnoreZ)).BeginInit();
            this.grpMocapSystem.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBodymodel
            // 
            this.grpBodymodel.Controls.Add(this.lblIgnoreZ);
            this.grpBodymodel.Controls.Add(this.nudIgnoreZ);
            this.grpBodymodel.Controls.Add(this.chkTrack);
            this.grpBodymodel.Controls.Add(this.lblCalibration);
            this.grpBodymodel.Controls.Add(this.cmbMode);
            this.grpBodymodel.Controls.Add(this.lblMode);
            this.grpBodymodel.Controls.Add(this.cmdCalibrateBody);
            this.grpBodymodel.Location = new System.Drawing.Point(13, 8);
            this.grpBodymodel.Name = "grpBodymodel";
            this.grpBodymodel.Size = new System.Drawing.Size(200, 346);
            this.grpBodymodel.TabIndex = 27;
            this.grpBodymodel.TabStop = false;
            this.grpBodymodel.Text = "Bodymodel";
            // 
            // lblIgnoreZ
            // 
            this.lblIgnoreZ.AutoSize = true;
            this.lblIgnoreZ.Location = new System.Drawing.Point(6, 16);
            this.lblIgnoreZ.Name = "lblIgnoreZ";
            this.lblIgnoreZ.Size = new System.Drawing.Size(158, 13);
            this.lblIgnoreZ.TabIndex = 34;
            this.lblIgnoreZ.Text = "Ignore Above Z cm (0 = disable)";
            // 
            // nudIgnoreZ
            // 
            this.nudIgnoreZ.Location = new System.Drawing.Point(9, 32);
            this.nudIgnoreZ.Name = "nudIgnoreZ";
            this.nudIgnoreZ.Size = new System.Drawing.Size(120, 20);
            this.nudIgnoreZ.TabIndex = 28;
            // 
            // chkTrack
            // 
            this.chkTrack.AutoSize = true;
            this.chkTrack.Location = new System.Drawing.Point(9, 106);
            this.chkTrack.Name = "chkTrack";
            this.chkTrack.Size = new System.Drawing.Size(93, 17);
            this.chkTrack.TabIndex = 33;
            this.chkTrack.Text = "Track on error";
            this.chkTrack.UseVisualStyleBackColor = true;
            // 
            // lblCalibration
            // 
            this.lblCalibration.AutoSize = true;
            this.lblCalibration.Location = new System.Drawing.Point(6, 126);
            this.lblCalibration.Name = "lblCalibration";
            this.lblCalibration.Size = new System.Drawing.Size(56, 13);
            this.lblCalibration.TabIndex = 32;
            this.lblCalibration.Text = "Calibration";
            // 
            // cmbMode
            // 
            this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMode.FormattingEnabled = true;
            this.cmbMode.Location = new System.Drawing.Point(76, 79);
            this.cmbMode.Name = "cmbMode";
            this.cmbMode.Size = new System.Drawing.Size(102, 21);
            this.cmbMode.TabIndex = 31;
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(6, 82);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(34, 13);
            this.lblMode.TabIndex = 30;
            this.lblMode.Text = "Mode";
            // 
            // cmdCalibrateBody
            // 
            this.cmdCalibrateBody.BackColor = System.Drawing.Color.Lime;
            this.cmdCalibrateBody.Location = new System.Drawing.Point(119, 317);
            this.cmdCalibrateBody.Name = "cmdCalibrateBody";
            this.cmdCalibrateBody.Size = new System.Drawing.Size(75, 23);
            this.cmdCalibrateBody.TabIndex = 24;
            this.cmdCalibrateBody.Text = "Calibrate";
            this.cmdCalibrateBody.UseVisualStyleBackColor = false;
            // 
            // grpMocapSystem
            // 
            this.grpMocapSystem.Controls.Add(this.lblNMarkers);
            this.grpMocapSystem.Controls.Add(this.lstNMarkers);
            this.grpMocapSystem.Controls.Add(this.pnlSettings);
            this.grpMocapSystem.Controls.Add(this.cmdConnectSystem);
            this.grpMocapSystem.Controls.Add(this.lstKnownMarkers);
            this.grpMocapSystem.Controls.Add(this.lblKnownMarkers);
            this.grpMocapSystem.Controls.Add(this.scene);
            this.grpMocapSystem.Location = new System.Drawing.Point(219, 8);
            this.grpMocapSystem.Name = "grpMocapSystem";
            this.grpMocapSystem.Size = new System.Drawing.Size(627, 522);
            this.grpMocapSystem.TabIndex = 26;
            this.grpMocapSystem.TabStop = false;
            this.grpMocapSystem.Text = "MoCap System";
            // 
            // pnlSettings
            // 
            this.pnlSettings.AutoScroll = true;
            this.pnlSettings.Location = new System.Drawing.Point(16, 51);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(178, 113);
            this.pnlSettings.TabIndex = 22;
            // 
            // cmdConnectSystem
            // 
            this.cmdConnectSystem.Location = new System.Drawing.Point(16, 22);
            this.cmdConnectSystem.Name = "cmdConnectSystem";
            this.cmdConnectSystem.Size = new System.Drawing.Size(178, 23);
            this.cmdConnectSystem.TabIndex = 19;
            this.cmdConnectSystem.Text = "Connect";
            this.cmdConnectSystem.UseVisualStyleBackColor = true;
            // 
            // lstKnownMarkers
            // 
            this.lstKnownMarkers.FormattingEnabled = true;
            this.lstKnownMarkers.Location = new System.Drawing.Point(16, 192);
            this.lstKnownMarkers.Name = "lstKnownMarkers";
            this.lstKnownMarkers.Size = new System.Drawing.Size(178, 199);
            this.lstKnownMarkers.TabIndex = 13;
            // 
            // lblKnownMarkers
            // 
            this.lblKnownMarkers.AutoSize = true;
            this.lblKnownMarkers.Location = new System.Drawing.Point(13, 176);
            this.lblKnownMarkers.Name = "lblKnownMarkers";
            this.lblKnownMarkers.Size = new System.Drawing.Size(99, 13);
            this.lblKnownMarkers.TabIndex = 16;
            this.lblKnownMarkers.Text = "Predefined Markers";
            // 
            // scene
            // 
            this.scene.BackColor = System.Drawing.Color.Black;
            this.scene.Location = new System.Drawing.Point(200, 19);
            this.scene.Name = "scene";
            this.scene.Size = new System.Drawing.Size(413, 376);
            this.scene.TabIndex = 18;
            this.scene.VSync = false;
            // 
            // lstNMarkers
            // 
            this.lstNMarkers.FormattingEnabled = true;
            this.lstNMarkers.Location = new System.Drawing.Point(16, 415);
            this.lstNMarkers.Name = "lstNMarkers";
            this.lstNMarkers.Size = new System.Drawing.Size(178, 95);
            this.lstNMarkers.TabIndex = 23;
            // 
            // lblNMarkers
            // 
            this.lblNMarkers.AutoSize = true;
            this.lblNMarkers.Location = new System.Drawing.Point(13, 399);
            this.lblNMarkers.Name = "lblNMarkers";
            this.lblNMarkers.Size = new System.Drawing.Size(176, 13);
            this.lblNMarkers.TabIndex = 24;
            this.lblNMarkers.Text = "Number of markers (last 500 frames)";
            // 
            // MoCap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpBodymodel);
            this.Controls.Add(this.grpMocapSystem);
            this.Name = "MoCap";
            this.Size = new System.Drawing.Size(859, 545);
            this.grpBodymodel.ResumeLayout(false);
            this.grpBodymodel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudIgnoreZ)).EndInit();
            this.grpMocapSystem.ResumeLayout(false);
            this.grpMocapSystem.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpBodymodel;
        private System.Windows.Forms.CheckBox chkTrack;
        private System.Windows.Forms.Label lblCalibration;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Button cmdCalibrateBody;
        private System.Windows.Forms.GroupBox grpMocapSystem;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Button cmdConnectSystem;
        private System.Windows.Forms.ListBox lstKnownMarkers;
        private System.Windows.Forms.Label lblKnownMarkers;
        private MDOL.Scene.SceneControl scene;
        private System.Windows.Forms.Label lblIgnoreZ;
        private System.Windows.Forms.NumericUpDown nudIgnoreZ;
        private System.Windows.Forms.Label lblNMarkers;
        private System.Windows.Forms.ListBox lstNMarkers;
    }
}
