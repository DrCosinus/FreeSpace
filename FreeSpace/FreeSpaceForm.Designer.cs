namespace FreeSpace
{
    partial class FreeSpace
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
            this.cbDiskList = new System.Windows.Forms.ComboBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.tvDisk = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbLogger = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbDiskList
            // 
            this.cbDiskList.BackColor = System.Drawing.SystemColors.Window;
            this.cbDiskList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDiskList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDiskList.FormattingEnabled = true;
            this.cbDiskList.Location = new System.Drawing.Point(0, 0);
            this.cbDiskList.Name = "cbDiskList";
            this.cbDiskList.Size = new System.Drawing.Size(545, 21);
            this.cbDiskList.TabIndex = 0;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnUpdate.Location = new System.Drawing.Point(545, 0);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(67, 22);
            this.btnUpdate.TabIndex = 1;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.OnButtonUpdateClick);
            // 
            // tvDisk
            // 
            this.tvDisk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDisk.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.tvDisk.Location = new System.Drawing.Point(0, 0);
            this.tvDisk.Name = "tvDisk";
            this.tvDisk.Size = new System.Drawing.Size(612, 264);
            this.tvDisk.TabIndex = 2;
            this.tvDisk.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.OnTreeviewDiskDrawNode);
            this.tvDisk.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnTreeviewDiskMouseDown);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbDiskList);
            this.panel1.Controls.Add(this.btnUpdate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(612, 22);
            this.panel1.TabIndex = 3;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(5, 27);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvDisk);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbLogger);
            this.splitContainer1.Size = new System.Drawing.Size(612, 412);
            this.splitContainer1.SplitterDistance = 264;
            this.splitContainer1.TabIndex = 4;
            // 
            // rtbLogger
            // 
            this.rtbLogger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLogger.Location = new System.Drawing.Point(0, 0);
            this.rtbLogger.Name = "rtbLogger";
            this.rtbLogger.ReadOnly = true;
            this.rtbLogger.Size = new System.Drawing.Size(612, 144);
            this.rtbLogger.TabIndex = 0;
            this.rtbLogger.Text = "";
            this.rtbLogger.WordWrap = false;
            // 
            // FreeSpace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 444);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "FreeSpace";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "FreeSpace 2023.05.31";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FreeSpace_FormClosing);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbDiskList;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.TreeView tvDisk;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox rtbLogger;
    }
}

