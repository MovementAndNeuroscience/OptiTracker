namespace Optitracker
{
    partial class Treadmill
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.chtPhaseHeight = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblInterval = new System.Windows.Forms.Label();
            this.nudFrequencyMin = new System.Windows.Forms.NumericUpDown();
            this.cmdStart = new System.Windows.Forms.Button();
            this.grpCalibration = new System.Windows.Forms.GroupBox();
            this.chkMedianFilter = new System.Windows.Forms.CheckBox();
            this.grpAutoCalib = new System.Windows.Forms.GroupBox();
            this.lblCalibTime = new System.Windows.Forms.Label();
            this.nudCalibTime = new System.Windows.Forms.NumericUpDown();
            this.lblCalibDelay = new System.Windows.Forms.Label();
            this.nudCalibDelay = new System.Windows.Forms.NumericUpDown();
            this.chkShowMid = new System.Windows.Forms.CheckBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.nudHeight = new System.Windows.Forms.NumericUpDown();
            this.cmdCalibrate = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.txtPostfix = new System.Windows.Forms.TextBox();
            this.lblPostfix = new System.Windows.Forms.Label();
            this.trbLeftRight = new System.Windows.Forms.TrackBar();
            this.lblLeft = new System.Windows.Forms.Label();
            this.lblRight = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nudTimeout = new System.Windows.Forms.NumericUpDown();
            this.lblTimeout = new System.Windows.Forms.Label();
            this.tmrTimeout = new System.Windows.Forms.Timer(this.components);
            this.lstTargets = new System.Windows.Forms.ListBox();
            this.pnlTarget = new System.Windows.Forms.Panel();
            this.cmdAdd = new System.Windows.Forms.Button();
            this.grpTargets = new System.Windows.Forms.GroupBox();
            this.cmdGlobalTrigger = new System.Windows.Forms.Button();
            this.grpStart = new System.Windows.Forms.GroupBox();
            this.chkGroupRandom = new System.Windows.Forms.CheckBox();
            this.chkStartTrigger = new System.Windows.Forms.CheckBox();
            this.chkFixSpawn = new System.Windows.Forms.CheckBox();
            this.nudFrequencyMax = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.chtPhaseHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrequencyMin)).BeginInit();
            this.grpCalibration.SuspendLayout();
            this.grpAutoCalib.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalibTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalibDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbLeftRight)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeout)).BeginInit();
            this.grpTargets.SuspendLayout();
            this.grpStart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrequencyMax)).BeginInit();
            this.SuspendLayout();
            // 
            // chtPhaseHeight
            // 
            chartArea1.Name = "ChartArea1";
            this.chtPhaseHeight.ChartAreas.Add(chartArea1);
            this.chtPhaseHeight.Location = new System.Drawing.Point(541, 27);
            this.chtPhaseHeight.Name = "chtPhaseHeight";
            this.chtPhaseHeight.Size = new System.Drawing.Size(445, 594);
            this.chtPhaseHeight.TabIndex = 4;
            this.chtPhaseHeight.Text = "chart1";
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(8, 281);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(111, 13);
            this.lblInterval.TabIndex = 8;
            this.lblInterval.Text = "Spawn every x\'th step";
            // 
            // nudFrequencyMin
            // 
            this.nudFrequencyMin.Location = new System.Drawing.Point(150, 279);
            this.nudFrequencyMin.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrequencyMin.Name = "nudFrequencyMin";
            this.nudFrequencyMin.Size = new System.Drawing.Size(62, 20);
            this.nudFrequencyMin.TabIndex = 24;
            this.nudFrequencyMin.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudFrequencyMin.ValueChanged += new System.EventHandler(this.nudFrequencyMin_ValueChanged);
            // 
            // cmdStart
            // 
            this.cmdStart.BackColor = System.Drawing.Color.Lime;
            this.cmdStart.Location = new System.Drawing.Point(420, 37);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(101, 40);
            this.cmdStart.TabIndex = 26;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = false;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // grpCalibration
            // 
            this.grpCalibration.Controls.Add(this.chkMedianFilter);
            this.grpCalibration.Controls.Add(this.grpAutoCalib);
            this.grpCalibration.Controls.Add(this.chkShowMid);
            this.grpCalibration.Controls.Add(this.lblHeight);
            this.grpCalibration.Controls.Add(this.nudHeight);
            this.grpCalibration.Controls.Add(this.cmdCalibrate);
            this.grpCalibration.Controls.Add(this.lblStats);
            this.grpCalibration.Location = new System.Drawing.Point(12, 27);
            this.grpCalibration.Name = "grpCalibration";
            this.grpCalibration.Size = new System.Drawing.Size(523, 200);
            this.grpCalibration.TabIndex = 27;
            this.grpCalibration.TabStop = false;
            this.grpCalibration.Text = "Calibration";
            // 
            // chkMedianFilter
            // 
            this.chkMedianFilter.AutoSize = true;
            this.chkMedianFilter.Location = new System.Drawing.Point(18, 173);
            this.chkMedianFilter.Name = "chkMedianFilter";
            this.chkMedianFilter.Size = new System.Drawing.Size(111, 17);
            this.chkMedianFilter.TabIndex = 60;
            this.chkMedianFilter.Text = "Apply median filter";
            this.chkMedianFilter.UseVisualStyleBackColor = true;
            this.chkMedianFilter.CheckedChanged += new System.EventHandler(this.chkMedianFilter_CheckedChanged);
            // 
            // grpAutoCalib
            // 
            this.grpAutoCalib.Controls.Add(this.lblCalibTime);
            this.grpAutoCalib.Controls.Add(this.nudCalibTime);
            this.grpAutoCalib.Controls.Add(this.lblCalibDelay);
            this.grpAutoCalib.Controls.Add(this.nudCalibDelay);
            this.grpAutoCalib.Location = new System.Drawing.Point(344, 68);
            this.grpAutoCalib.Name = "grpAutoCalib";
            this.grpAutoCalib.Size = new System.Drawing.Size(173, 73);
            this.grpAutoCalib.TabIndex = 59;
            this.grpAutoCalib.TabStop = false;
            this.grpAutoCalib.Text = "Automatic (Seconds)";
            // 
            // lblCalibTime
            // 
            this.lblCalibTime.AutoSize = true;
            this.lblCalibTime.Location = new System.Drawing.Point(10, 44);
            this.lblCalibTime.Name = "lblCalibTime";
            this.lblCalibTime.Size = new System.Drawing.Size(29, 13);
            this.lblCalibTime.TabIndex = 58;
            this.lblCalibTime.Text = "Stop";
            // 
            // nudCalibTime
            // 
            this.nudCalibTime.Location = new System.Drawing.Point(45, 44);
            this.nudCalibTime.Name = "nudCalibTime";
            this.nudCalibTime.Size = new System.Drawing.Size(120, 20);
            this.nudCalibTime.TabIndex = 56;
            // 
            // lblCalibDelay
            // 
            this.lblCalibDelay.AutoSize = true;
            this.lblCalibDelay.Location = new System.Drawing.Point(10, 16);
            this.lblCalibDelay.Name = "lblCalibDelay";
            this.lblCalibDelay.Size = new System.Drawing.Size(29, 13);
            this.lblCalibDelay.TabIndex = 57;
            this.lblCalibDelay.Text = "Start";
            // 
            // nudCalibDelay
            // 
            this.nudCalibDelay.Location = new System.Drawing.Point(45, 14);
            this.nudCalibDelay.Name = "nudCalibDelay";
            this.nudCalibDelay.Size = new System.Drawing.Size(120, 20);
            this.nudCalibDelay.TabIndex = 55;
            // 
            // chkShowMid
            // 
            this.chkShowMid.AutoSize = true;
            this.chkShowMid.Checked = true;
            this.chkShowMid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowMid.Location = new System.Drawing.Point(427, 147);
            this.chkShowMid.Name = "chkShowMid";
            this.chkShowMid.Size = new System.Drawing.Size(73, 17);
            this.chkShowMid.TabIndex = 54;
            this.chkShowMid.Text = "Show Mid";
            this.chkShowMid.UseVisualStyleBackColor = true;
            this.chkShowMid.CheckedChanged += new System.EventHandler(this.chkShowMid_CheckedChanged);
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(15, 128);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(124, 13);
            this.lblHeight.TabIndex = 53;
            this.lblHeight.Text = "Height Threshold (in CM)";
            // 
            // nudHeight
            // 
            this.nudHeight.Location = new System.Drawing.Point(18, 147);
            this.nudHeight.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.nudHeight.Name = "nudHeight";
            this.nudHeight.Size = new System.Drawing.Size(77, 20);
            this.nudHeight.TabIndex = 52;
            this.nudHeight.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudHeight.ValueChanged += new System.EventHandler(this.nudHeight_ValueChanged);
            // 
            // cmdCalibrate
            // 
            this.cmdCalibrate.BackColor = System.Drawing.Color.Lime;
            this.cmdCalibrate.Location = new System.Drawing.Point(427, 170);
            this.cmdCalibrate.Name = "cmdCalibrate";
            this.cmdCalibrate.Size = new System.Drawing.Size(90, 23);
            this.cmdCalibrate.TabIndex = 1;
            this.cmdCalibrate.Text = "Calibrate";
            this.cmdCalibrate.UseVisualStyleBackColor = false;
            this.cmdCalibrate.Click += new System.EventHandler(this.cmdCalibrate_Click);
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Location = new System.Drawing.Point(15, 16);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(0, 13);
            this.lblStats.TabIndex = 0;
            // 
            // txtPostfix
            // 
            this.txtPostfix.Location = new System.Drawing.Point(114, 13);
            this.txtPostfix.Name = "txtPostfix";
            this.txtPostfix.Size = new System.Drawing.Size(121, 20);
            this.txtPostfix.TabIndex = 45;
            // 
            // lblPostfix
            // 
            this.lblPostfix.AutoSize = true;
            this.lblPostfix.Location = new System.Drawing.Point(6, 16);
            this.lblPostfix.Name = "lblPostfix";
            this.lblPostfix.Size = new System.Drawing.Size(102, 13);
            this.lblPostfix.TabIndex = 46;
            this.lblPostfix.Text = "File postfix (optional)";
            // 
            // trbLeftRight
            // 
            this.trbLeftRight.Location = new System.Drawing.Point(75, 233);
            this.trbLeftRight.Maximum = 100;
            this.trbLeftRight.Name = "trbLeftRight";
            this.trbLeftRight.Size = new System.Drawing.Size(390, 45);
            this.trbLeftRight.TabIndex = 54;
            this.trbLeftRight.Value = 50;
            this.trbLeftRight.Scroll += new System.EventHandler(this.trbLeftRight_Scroll);
            // 
            // lblLeft
            // 
            this.lblLeft.AutoSize = true;
            this.lblLeft.Location = new System.Drawing.Point(12, 245);
            this.lblLeft.Name = "lblLeft";
            this.lblLeft.Size = new System.Drawing.Size(51, 13);
            this.lblLeft.TabIndex = 55;
            this.lblLeft.Text = "Left: 50%";
            // 
            // lblRight
            // 
            this.lblRight.AutoSize = true;
            this.lblRight.Location = new System.Drawing.Point(471, 245);
            this.lblRight.Name = "lblRight";
            this.lblRight.Size = new System.Drawing.Size(58, 13);
            this.lblRight.TabIndex = 56;
            this.lblRight.Text = "Right: 50%";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(998, 24);
            this.menuStrip1.TabIndex = 57;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // nudTimeout
            // 
            this.nudTimeout.Location = new System.Drawing.Point(114, 57);
            this.nudTimeout.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.nudTimeout.Name = "nudTimeout";
            this.nudTimeout.Size = new System.Drawing.Size(121, 20);
            this.nudTimeout.TabIndex = 58;
            this.nudTimeout.ValueChanged += new System.EventHandler(this.nudTimeout_ValueChanged);
            // 
            // lblTimeout
            // 
            this.lblTimeout.AutoSize = true;
            this.lblTimeout.Location = new System.Drawing.Point(8, 51);
            this.lblTimeout.Name = "lblTimeout";
            this.lblTimeout.Size = new System.Drawing.Size(106, 26);
            this.lblTimeout.TabIndex = 59;
            this.lblTimeout.Text = "Stop after X seconds\r\n(0 = never)";
            // 
            // tmrTimeout
            // 
            this.tmrTimeout.Interval = 1000;
            this.tmrTimeout.Tick += new System.EventHandler(this.tmrTimeout_Tick);
            // 
            // lstTargets
            // 
            this.lstTargets.FormattingEnabled = true;
            this.lstTargets.Location = new System.Drawing.Point(6, 52);
            this.lstTargets.Name = "lstTargets";
            this.lstTargets.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstTargets.Size = new System.Drawing.Size(149, 147);
            this.lstTargets.TabIndex = 60;
            this.lstTargets.SelectedIndexChanged += new System.EventHandler(this.lstTargets_SelectedIndexChanged);
            this.lstTargets.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lstTargets_KeyUp);
            // 
            // pnlTarget
            // 
            this.pnlTarget.AutoScroll = true;
            this.pnlTarget.Location = new System.Drawing.Point(161, 18);
            this.pnlTarget.Name = "pnlTarget";
            this.pnlTarget.Size = new System.Drawing.Size(357, 186);
            this.pnlTarget.TabIndex = 61;
            // 
            // cmdAdd
            // 
            this.cmdAdd.Location = new System.Drawing.Point(46, 18);
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(71, 28);
            this.cmdAdd.TabIndex = 62;
            this.cmdAdd.Text = "+";
            this.cmdAdd.UseVisualStyleBackColor = true;
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // grpTargets
            // 
            this.grpTargets.Controls.Add(this.lstTargets);
            this.grpTargets.Controls.Add(this.cmdAdd);
            this.grpTargets.Controls.Add(this.pnlTarget);
            this.grpTargets.Location = new System.Drawing.Point(11, 305);
            this.grpTargets.Name = "grpTargets";
            this.grpTargets.Size = new System.Drawing.Size(524, 210);
            this.grpTargets.TabIndex = 63;
            this.grpTargets.TabStop = false;
            this.grpTargets.Text = "Targets";
            this.grpTargets.Resize += new System.EventHandler(this.grpTargets_Resize);
            // 
            // cmdGlobalTrigger
            // 
            this.cmdGlobalTrigger.Location = new System.Drawing.Point(322, 37);
            this.cmdGlobalTrigger.Name = "cmdGlobalTrigger";
            this.cmdGlobalTrigger.Size = new System.Drawing.Size(92, 40);
            this.cmdGlobalTrigger.TabIndex = 64;
            this.cmdGlobalTrigger.Text = "Globaltrigger";
            this.cmdGlobalTrigger.UseVisualStyleBackColor = true;
            this.cmdGlobalTrigger.Click += new System.EventHandler(this.cmdGlobalTrigger_Click);
            // 
            // grpStart
            // 
            this.grpStart.Controls.Add(this.chkGroupRandom);
            this.grpStart.Controls.Add(this.chkStartTrigger);
            this.grpStart.Controls.Add(this.cmdGlobalTrigger);
            this.grpStart.Controls.Add(this.lblPostfix);
            this.grpStart.Controls.Add(this.cmdStart);
            this.grpStart.Controls.Add(this.lblTimeout);
            this.grpStart.Controls.Add(this.txtPostfix);
            this.grpStart.Controls.Add(this.nudTimeout);
            this.grpStart.Location = new System.Drawing.Point(11, 521);
            this.grpStart.Name = "grpStart";
            this.grpStart.Size = new System.Drawing.Size(524, 100);
            this.grpStart.TabIndex = 56;
            this.grpStart.TabStop = false;
            this.grpStart.Text = "Start";
            // 
            // chkGroupRandom
            // 
            this.chkGroupRandom.AutoSize = true;
            this.chkGroupRandom.Location = new System.Drawing.Point(310, 14);
            this.chkGroupRandom.Name = "chkGroupRandom";
            this.chkGroupRandom.Size = new System.Drawing.Size(103, 17);
            this.chkGroupRandom.TabIndex = 66;
            this.chkGroupRandom.Text = "Random Groups";
            this.chkGroupRandom.UseVisualStyleBackColor = true;
            this.chkGroupRandom.CheckedChanged += new System.EventHandler(this.chkGroupRandom_CheckedChanged);
            // 
            // chkStartTrigger
            // 
            this.chkStartTrigger.AutoSize = true;
            this.chkStartTrigger.Location = new System.Drawing.Point(419, 14);
            this.chkStartTrigger.Name = "chkStartTrigger";
            this.chkStartTrigger.Size = new System.Drawing.Size(99, 17);
            this.chkStartTrigger.TabIndex = 65;
            this.chkStartTrigger.Text = "Trigger on Start";
            this.chkStartTrigger.UseVisualStyleBackColor = true;
            this.chkStartTrigger.CheckedChanged += new System.EventHandler(this.chkStartTrigger_CheckedChanged);
            // 
            // chkFixSpawn
            // 
            this.chkFixSpawn.AutoSize = true;
            this.chkFixSpawn.Location = new System.Drawing.Point(439, 277);
            this.chkFixSpawn.Name = "chkFixSpawn";
            this.chkFixSpawn.Size = new System.Drawing.Size(87, 17);
            this.chkFixSpawn.TabIndex = 60;
            this.chkFixSpawn.Text = "Fixed Spawn";
            this.chkFixSpawn.UseVisualStyleBackColor = true;
            this.chkFixSpawn.CheckedChanged += new System.EventHandler(this.chkFixSpawn_CheckedChanged);
            // 
            // nudFrequencyMax
            // 
            this.nudFrequencyMax.Location = new System.Drawing.Point(218, 279);
            this.nudFrequencyMax.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudFrequencyMax.Name = "nudFrequencyMax";
            this.nudFrequencyMax.Size = new System.Drawing.Size(62, 20);
            this.nudFrequencyMax.TabIndex = 64;
            this.nudFrequencyMax.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudFrequencyMax.ValueChanged += new System.EventHandler(this.nudFrequencyMax_ValueChanged);
            // 
            // Treadmill
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 629);
            this.Controls.Add(this.nudFrequencyMax);
            this.Controls.Add(this.chkFixSpawn);
            this.Controls.Add(this.grpStart);
            this.Controls.Add(this.grpTargets);
            this.Controls.Add(this.lblRight);
            this.Controls.Add(this.lblLeft);
            this.Controls.Add(this.trbLeftRight);
            this.Controls.Add(this.grpCalibration);
            this.Controls.Add(this.nudFrequencyMin);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.chtPhaseHeight);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Treadmill";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Treadmill_FormClosing);
            this.Load += new System.EventHandler(this.Treadmill_Load);
            this.Resize += new System.EventHandler(this.Treadmill_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.chtPhaseHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrequencyMin)).EndInit();
            this.grpCalibration.ResumeLayout(false);
            this.grpCalibration.PerformLayout();
            this.grpAutoCalib.ResumeLayout(false);
            this.grpAutoCalib.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalibTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCalibDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbLeftRight)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeout)).EndInit();
            this.grpTargets.ResumeLayout(false);
            this.grpStart.ResumeLayout(false);
            this.grpStart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudFrequencyMax)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataVisualization.Charting.Chart chtPhaseHeight;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.NumericUpDown nudFrequencyMin;
        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.GroupBox grpCalibration;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.Button cmdCalibrate;
        private System.Windows.Forms.TextBox txtPostfix;
        private System.Windows.Forms.Label lblPostfix;
        private System.Windows.Forms.NumericUpDown nudHeight;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.CheckBox chkShowMid;
        private System.Windows.Forms.TrackBar trbLeftRight;
        private System.Windows.Forms.Label lblLeft;
        private System.Windows.Forms.Label lblRight;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown nudTimeout;
        private System.Windows.Forms.Label lblTimeout;
        private System.Windows.Forms.Timer tmrTimeout;
        private System.Windows.Forms.ListBox lstTargets;
        private System.Windows.Forms.Panel pnlTarget;
        private System.Windows.Forms.Button cmdAdd;
        private System.Windows.Forms.GroupBox grpTargets;
        private System.Windows.Forms.Button cmdGlobalTrigger;
        private System.Windows.Forms.GroupBox grpStart;
        private System.Windows.Forms.NumericUpDown nudCalibTime;
        private System.Windows.Forms.NumericUpDown nudCalibDelay;
        private System.Windows.Forms.Label lblCalibDelay;
        private System.Windows.Forms.Label lblCalibTime;
        private System.Windows.Forms.GroupBox grpAutoCalib;
        private System.Windows.Forms.CheckBox chkStartTrigger;
        private System.Windows.Forms.CheckBox chkGroupRandom;
        private System.Windows.Forms.CheckBox chkFixSpawn;
        private System.Windows.Forms.NumericUpDown nudFrequencyMax;
        private System.Windows.Forms.CheckBox chkMedianFilter;
    }
}

