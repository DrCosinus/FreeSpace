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
        internal readonly static Font tagFont = new Font("Microsoft Sans", 10.0f, FontStyle.Bold);
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
        private static readonly Font MessageFont = new Font("Helvetica", 8.25f, FontStyle.Italic);
        private static readonly Color MessageColor = Color.Black;
        private static readonly Font WarningFont = new Font("Helvetica", 8.25f, FontStyle.Regular);
        private static readonly Color WarningColor = Color.DarkBlue;
        private static readonly Font ErrorFont = new Font("Helvetica", 8.25f, FontStyle.Bold);
        private static readonly Color ErrorColor = Color.DarkOrange;

        internal void LogAppendText(string _str, Font _font, Color _col)
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

        internal void LogClear()
        {
            lock (LoggerBox)
                LoggerBox.Invoke(new Action(() =>
                {
                    LoggerBox.Clear();
                    LoggerBox.ScrollToCaret();
                }));
        }

        internal void LogMessage(string message)
        {
            LogAppendText(message, MessageFont, MessageColor);
        }

        internal void LogWarning(string warning)
        {
            LogAppendText(warning, WarningFont, WarningColor);
        }

        internal void LogError(string error)
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
                    backgroundWorker.RunWorkerAsync(new BackgroundWorkerParameters { driveName = Drive.Name, localRoot = diRoot });
            }
        }

        private static float ComputeLogarithmicProgress(long min, long max, long value)
        {
            return (float)(Math.Log(value - min + 1.0) / Math.Log(max - min + 1.0));
        }

        private static float ComputeLinearProgress(long min, long max, long value)
        {
            return (float)(value - min) / (max - min);
        }

        private static readonly Color emptyFolderColor = Color.FromArgb(95, 255, 95);
        private static readonly Color biggestFolderColor = Color.FromArgb(255, 127, 63);
        private void OnTreeviewDiskDrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            TreeNode node = e.Node;

            if (!node.IsVisible)
                return;

            e.DrawDefault = true;

            if (node.Tag is NodeTag nodeTag)
            {
                float progress = ComputeLogarithmicProgress(0, BigestSize, nodeTag.Size);

                using (SolidBrush textBrush = new SolidBrush(emptyFolderColor.Lerp(biggestFolderColor, progress)))
                {
                    e.Graphics.DrawString(nodeTag.ToString(), tagFont, Brushes.Black, e.Bounds.Right + 3, e.Bounds.Top - 1);
                    e.Graphics.DrawString(nodeTag.ToString(), tagFont, textBrush, e.Bounds.Right + 2, e.Bounds.Top - 2);
                }
            }
        }

        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
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

        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnUpdate.Enabled = true;
        }
    }
}