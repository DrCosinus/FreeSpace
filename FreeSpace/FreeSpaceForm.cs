using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

// Pensez à checker le site : https://learn.microsoft.com/en-us/archive/blogs/bclteam/long-paths-in-net-part-1-of-3-kim-hamilton

namespace FreeSpace
{
    public partial class FreeSpace : Form
    {
        // Create a Font object for the node tags.
        private readonly Font tagFont = new Font("Helvetica", 9, FontStyle.Bold | FontStyle.Italic);
        private long BigestSize = 1;

        private class BackgroundWorkerParameters
        {
            public string driveName;
            public DirectoryInfo localRoot;
        }

        public FreeSpace()
        {
            InitializeComponent();
            Text = $"FreeSpace { System.Reflection.Assembly.GetExecutingAssembly().GetName().Version }";
        }

        #region Logger invokable methods
        static readonly Font MessageFont = new Font("Helvetica", 8.25f, FontStyle.Italic);
        static readonly Color MessageColor = Color.Black;
        static readonly Font WarningFont = new Font("Helvetica", 8.25f, FontStyle.Regular);
        static readonly Color WarningColor = Color.DarkBlue;
        static readonly Font ErrorFont = new Font("Helvetica", 8.25f, FontStyle.Bold);
        static readonly Color ErrorColor = Color.DarkOrange;

        void LogAppendText(string _str, Font _font, Color _col)
        {
            lock (LoggerBox)
                LoggerBox.Invoke(new Action(() =>
                {
                    LoggerBox.SelectionFont = _font;
                    LoggerBox.SelectionColor = _col;
                    LoggerBox.AppendText($"[{DateTime.Now}] {_str}");
                    LoggerBox.ScrollToCaret();
                }));
        }

        void LogClear()
        {
            lock (LoggerBox)
                LoggerBox.Invoke(new Action(() =>
                {
                    LoggerBox.Clear();
                    LoggerBox.ScrollToCaret();
                }));
        }

        void LogMessage(string message)
        {
            LogAppendText(message, MessageFont, MessageColor);
        }

        internal void LogWarning(string warning)
        {
            LogAppendText(warning, WarningFont, WarningColor);
        }

        void LogError(string error)
        {
            LogAppendText(error, ErrorFont, ErrorColor);
        }
        #endregion

        #region DiskDrive TreeView invokable methods
        internal TreeNode ClearDiskTreeAndAddRootNode(string _strRootName)
        {
            return (TreeNode)tvDisk.Invoke(new Func<TreeNode>(() =>
            {
                tvDisk.Nodes.Clear();
                return tvDisk.Nodes.Add(_strRootName);
            }));
        }

        internal void ShowDiskTree(bool _bEnabled)
        {
            splitContainer1.Invoke(new Action(() =>
            {
                if (_bEnabled)
                {
                    splitContainer1.Panel1Collapsed = false;
                    tvDisk.Show();
                }
                else
                {
                    splitContainer1.Panel1Collapsed = true;
                    tvDisk.Hide();
                }
            }));
        }
        #endregion

        private void OnFormLoad(object sender, EventArgs e)
        {
            DriveInfo[] DriveList = DriveInfo.GetDrives();
            foreach (DriveInfo Drive in DriveList)
            {
                string VolumeLabel;

                if (Drive.IsReady)
                {
                    VolumeLabel = Drive.VolumeLabel;
                }
                else
                {
                    VolumeLabel = "";
                }
                if (VolumeLabel.Length == 0)
                {
                    VolumeLabel = Drive.DriveType.ToString();
                }

                cbDiskList.Items.Add($"{Drive.Name} ({VolumeLabel})");
            }
            cbDiskList.SelectedIndex = 0;
        }

        private void OnButtonUpdateClick(object sender, EventArgs e)
        {
            DriveInfo Drive = DriveInfo.GetDrives()[cbDiskList.SelectedIndex];
            btnUpdate.Enabled = false;
            if (Drive.IsReady)
            {
                DirectoryInfo diRoot = Drive.RootDirectory;
                //DirectoryInfo diRoot = new DirectoryInfo(@"c:\Program Files\");
                if (!backgroundWorker.IsBusy)
                    backgroundWorker.RunWorkerAsync(new BackgroundWorkerParameters { driveName = Drive.Name, localRoot = diRoot } );
            }
        }

        private void OnTreeviewDiskDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (!e.Node.IsVisible)
                return;

            // Draw the background and node text for a selected node.
            if ((e.State & (TreeNodeStates.Selected | TreeNodeStates.Grayed)) != 0)
            {
                // Draw the background of the selected node. The NodeBounds
                // method makes the highlight rectangle large enough to
                // include the text of a node tag, if one is present.
                e.Graphics.FillRectangle(Brushes.LightGray, NodeBounds(e.Node));

                // Retrieve the node font. If the node font has not been set,
                // use the TreeView font.
                Font nodeFont = e.Node.NodeFont;
                if (nodeFont == null) nodeFont = ((TreeView)sender).Font;

                // Draw the node text.
                Rectangle focusBounds = NodeBounds(e.Node);
                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.Gray,
                    focusBounds /*Rectangle.Inflate(e.Bounds, 2, 0)*/);
            }

            // Use the default background and node text.
            else
            {
                e.DrawDefault = true;
            }

            // If a node tag is present, draw its string representation 
            // to the right of the label text.
            if (e.Node.Tag != null)
            {
                // LOGARITHMIC
                // int v = (int)( Math.Log((double)((NodeTag)e.Node.Tag).Data) / Math.Log(2.0f) ) * 4;
                int v = (int)(Math.Log((double)((NodeTag)e.Node.Tag).Data + 1) / Math.Log(BigestSize) * 128.0f);
                // LINEAR
                //int v = (int)((double)((NodeTag)e.Node.Tag).Data / BigestSize * 255.0f);
                if ((uint)v > 128)
                    v = 128;
                SolidBrush brush = new SolidBrush(Color.FromArgb(255, v + 127, 255 - v, 127));

                e.Graphics.DrawString(e.Node.Tag.ToString(), tagFont,
                    Brushes.Black, e.Bounds.Right + 3, e.Bounds.Top + 1);
                e.Graphics.DrawString(e.Node.Tag.ToString(), tagFont,
                    brush /*Brushes.Blue*/, e.Bounds.Right + 2, e.Bounds.Top);
            }

            // If the node has focus, draw the focus rectangle large, making
            // it large enough to include the text of the node tag, if present.
            if ((e.State & TreeNodeStates.Focused) != 0)
            {
                using (Pen focusPen = new Pen(Color.Black))
                {
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    Rectangle focusBounds = NodeBounds(e.Node);
                    focusBounds.Size = new Size(focusBounds.Width - 1,
                    focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
            }
        }

        // Returns the bounds of the specified node, including the region 
        // occupied by the node label and any node tag displayed.
        private Rectangle NodeBounds(TreeNode node)
        {
            // Set the return value to the normal node bounds.
            Rectangle bounds = node.Bounds;
            if (node.Tag != null)
            {
                // Retrieve a Graphics object from the TreeView handle
                // and use it to calculate the display width of the tag.
                Graphics g = tvDisk.CreateGraphics();
                int tagWidth = (int)g.MeasureString
                    (node.Tag.ToString(), tagFont).Width + 6;

                // Adjust the node bounds using the calculated value.
                bounds.Offset(tagWidth / 2, 0);
                bounds = Rectangle.Inflate(bounds, tagWidth / 2, 0);
                g.Dispose();
            }

            return bounds;
        }

        private void OnTreeviewDiskMouseDown(object sender, MouseEventArgs e)
        {
            TreeNode clickedNode = tvDisk.GetNodeAt(e.X, e.Y);
            if (clickedNode != null && NodeBounds(clickedNode).Contains(e.X, e.Y))
            {
                tvDisk.SelectedNode = clickedNode;
            }
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var args = e.Argument as BackgroundWorkerParameters;
            var rootDirInfo = args.localRoot;

            FolderTree root;
            TreeNode treeviewRoot = ClearDiskTreeAndAddRootNode(rootDirInfo.Name);
            LogClear();
            LogMessage("START THREAD\n");
            root = new FolderTree(rootDirInfo.Name);
            ShowDiskTree(false);
            root.DumpDirectory(rootDirInfo, this);
            LogMessage("DUMP COMPLETED\n");
            root.Sort();
            LogMessage("SORT COMPLETED\n");
            root.FillTreeView(treeviewRoot);
            LogMessage("TREE FILLED\n");
            BigestSize = root.size;
            ShowDiskTree(true);
            LogMessage("FINISH THREAD\n");
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnUpdate.Enabled = true;
        }
    }
}