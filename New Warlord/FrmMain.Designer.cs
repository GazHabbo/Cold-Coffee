namespace Warlord
{
    partial class FrmMain
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
            this.TxtLog = new System.Windows.Forms.TextBox();
            this.TVClients = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PauseLog = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stufToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editMySQLDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtLog
            // 
            this.TxtLog.BackColor = System.Drawing.Color.White;
            this.TxtLog.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TxtLog.Location = new System.Drawing.Point(6, 19);
            this.TxtLog.Multiline = true;
            this.TxtLog.Name = "TxtLog";
            this.TxtLog.ReadOnly = true;
            this.TxtLog.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TxtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtLog.ShortcutsEnabled = false;
            this.TxtLog.Size = new System.Drawing.Size(538, 196);
            this.TxtLog.TabIndex = 0;
            this.TxtLog.WordWrap = false;
            // 
            // TVClients
            // 
            this.TVClients.Location = new System.Drawing.Point(6, 19);
            this.TVClients.Name = "TVClients";
            this.TVClients.Size = new System.Drawing.Size(265, 252);
            this.TVClients.TabIndex = 2;
            this.TVClients.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TVClients_NodeMouseClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 221);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 22);
            this.button1.TabIndex = 4;
            this.button1.Text = "&Copy selected";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.PauseLog);
            this.groupBox1.Controls.Add(this.TxtLog);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(12, 310);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(550, 249);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Packet Log";
            // 
            // PauseLog
            // 
            this.PauseLog.AutoSize = true;
            this.PauseLog.Location = new System.Drawing.Point(430, 225);
            this.PauseLog.Name = "PauseLog";
            this.PauseLog.Size = new System.Drawing.Size(114, 17);
            this.PauseLog.TabIndex = 5;
            this.PauseLog.Text = "Pause Packet Log";
            this.PauseLog.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TVClients);
            this.groupBox2.Location = new System.Drawing.Point(12, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(277, 277);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Connected Clients";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(574, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stufToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // stufToolStripMenuItem
            // 
            this.stufToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editMySQLDetailsToolStripMenuItem,
            this.editPortToolStripMenuItem});
            this.stufToolStripMenuItem.Name = "stufToolStripMenuItem";
            this.stufToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stufToolStripMenuItem.Text = "Server Options";
            // 
            // editMySQLDetailsToolStripMenuItem
            // 
            this.editMySQLDetailsToolStripMenuItem.Name = "editMySQLDetailsToolStripMenuItem";
            this.editMySQLDetailsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.editMySQLDetailsToolStripMenuItem.Text = "Edit MySQL Details";
            // 
            // editPortToolStripMenuItem
            // 
            this.editPortToolStripMenuItem.Name = "editPortToolStripMenuItem";
            this.editPortToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.editPortToolStripMenuItem.Text = "Edit Port";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 572);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.Text = "Server Control Center";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtLog;
        internal System.Windows.Forms.TreeView TVClients;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox PauseLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stufToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editMySQLDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editPortToolStripMenuItem;
    }
}

