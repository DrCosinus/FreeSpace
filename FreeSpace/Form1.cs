using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Collections;

namespace FreeSpace
{
    public partial class FreeSpace : Form
    {
        public FreeSpace()
        {
            InitializeComponent();
            WorkThread.Init(tvDisk, rtbLogger, splitContainer1);
        }

        private void Form1_Load(object sender, EventArgs e)
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

                cbDiskList.Items.Add(String.Format("{1} ({0})", Drive.Name, VolumeLabel));
            }
            cbDiskList.SelectedIndex = 0;
            tvDisk.TreeViewNodeSorter = new NodeSorter();
        }
        public class NodeSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;
                if (ty.Tag == null)
                    return -1;
                if (tx.Tag == null)
                    return 1;
                if (((NodeTag)ty.Tag).Data > ((NodeTag)tx.Tag).Data)
                    return 1;
                else
                    return -1;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            DriveInfo Drive = DriveInfo.GetDrives()[cbDiskList.SelectedIndex];

            if (Drive.IsReady)
            {
                DirectoryInfo diRoot = Drive.RootDirectory;
                //DirectoryInfo diRoot = new DirectoryInfo(@"c:\Program Files\");
                if (!WorkThread.isRunning)
                    WorkThread.StartDump(Drive.Name, diRoot);
            }
        }

        // Create a Font object for the node tags.
        Font tagFont = new Font("Helvetica", 9, FontStyle.Bold | FontStyle.Italic);

        private void tvDisk_DrawNode(object sender, DrawTreeNodeEventArgs e)
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
                int v = (int)(Math.Log((double)((NodeTag)e.Node.Tag).Data + 1) / Math.Log((double)WorkThread.BigestSize) * 128.0f);
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

        private void tvDisk_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode clickedNode = tvDisk.GetNodeAt(e.X, e.Y);
            if (clickedNode != null && NodeBounds(clickedNode).Contains(e.X, e.Y))
            {
                tvDisk.SelectedNode = clickedNode;
            }
        }

        private void FreeSpace_FormClosing(object sender, FormClosingEventArgs e)
        {
            WorkThread.AbortDump();
        }
    }
}