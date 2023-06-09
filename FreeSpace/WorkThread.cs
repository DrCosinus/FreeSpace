using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Pensez à checker le site : https://learn.microsoft.com/en-us/archive/blogs/bclteam/long-paths-in-net-part-1-of-3-kim-hamilton

namespace FreeSpace
{
    class WorkThread
    {
        static readonly Font MessageFont = new Font("Helvetica", 8.25f, FontStyle.Italic);
        static readonly Color MessageColor = Color.Black;
        static readonly Font WarningFont = new Font("Helvetica", 8.25f, FontStyle.Regular);
        static readonly Color WarningColor = Color.DarkBlue;
        static readonly Font ErrorFont = new Font("Helvetica", 8.25f, FontStyle.Bold);
        static readonly Color ErrorColor = Color.DarkOrange;

        static bool bRunning = false;
        static TreeNode treeRootNode;
        static TreeView tvDiskDrive;
        static DirectoryInfo directoryInfo;
        static long bigestSize = 1;
        static RichTextBox RTFLogger;
        static SplitContainer splitter;

        public static bool isRunning { get { return bRunning; } }
        public static long BigestSize { get { return bigestSize; } }

        #region Virtual Folder Tree
        private static void FillTree_R(TreeNode treeLocalRootNode, FolderTree root)
        {
            treeLocalRootNode.Tag = new NodeTag(root.size);

            foreach (var child in root.children)
            {
                var child_node = treeLocalRootNode.Nodes.Add(child.text);
                FillTree_R(child_node, child);
            }
        }

        private static void FillTree(TreeNode treeRootNode, FolderTree root)
        {
            if (tvDiskDrive.InvokeRequired)
            {
                tvDiskDrive.Invoke((Action<TreeNode, FolderTree>)FillTree_R, treeRootNode, root);
                return;
            }
            FillTree_R(treeRootNode, root);
        }

        private static void SortFolderTree(FolderTree root)
        {
            Parallel.ForEach(root.children, child =>
            {
                SortFolderTree(child);
            });

            root.children.Sort();
        }

        internal class FolderTree : IEquatable<FolderTree>, IComparable<FolderTree>
        {
            public readonly string text;
            internal long size = 0;
            internal List<FolderTree> children = new List<FolderTree>();

            public FolderTree(string text)
            {
                this.text = text;
            }

            public int CompareTo(FolderTree other)
            {
                if (other == null) return -1;
                int result = -size.CompareTo(other.size);
                if (result == 0) result = text.CompareTo(other.text);
                return result;
            }

            public bool Equals(FolderTree other)
            {
                if (other == null) return false;
                return ReferenceEquals(other, this);
            }

            internal FolderTree AddChild(string name)
            {
                var child = new FolderTree(name);
                Debug.Assert(child != null);
                children.Add(child);
                return child;
            }
        }
        #endregion

        #region DiskDrive TreeView invokable methods
        static void ClearDiskTree(string _strRootName)
        {
            tvDiskDrive.Invoke(new Action(() =>
            {
                tvDiskDrive.Nodes.Clear();
                treeRootNode = tvDiskDrive.Nodes.Add(_strRootName);
            }));
        }

        static void ShowDiskTree(bool _bEnabled)
        {
            splitter.Invoke(new Action(() =>
            {
                if (_bEnabled)
                {
                    splitter.Panel1Collapsed = false;
                    tvDiskDrive.Show();
                }
                else
                {
                    splitter.Panel1Collapsed = true;
                    tvDiskDrive.Hide();
                }
            }));
        }
        #endregion

        #region Logger invokable methods
        static void LogAppendText(string _str, Font _font, Color _col)
        {
            lock (RTFLogger)
                RTFLogger.Invoke(new Action(() =>
                {
                    RTFLogger.SelectionFont = _font;
                    RTFLogger.SelectionColor = _col;
                    RTFLogger.AppendText($"[{DateTime.Now}] {_str}");
                    RTFLogger.ScrollToCaret();
                }));
        }

        static void LogClear()
        {
            lock (RTFLogger)
                RTFLogger.Invoke(new Action(() =>
                {
                    RTFLogger.Clear();
                    RTFLogger.ScrollToCaret();
                }));
        }

        static void LogMessage(string message)
        {
            LogAppendText(message, MessageFont, MessageColor);
        }

        static void LogWarning(string warning)
        {
            LogAppendText(warning, WarningFont, WarningColor);
        }

        static void LogError(string error)
        {
            LogAppendText(error, ErrorFont, ErrorColor);
        }
        #endregion

        public WorkThread()
        {
            throw new Exception("YOU MUST NOT CALL CONSTRUCTOR");
        }

        public static void Init(TreeView _tv, RichTextBox _rtbLogger, SplitContainer _splitter)
        {
            splitter = _splitter;
            tvDiskDrive = _tv;
            RTFLogger = _rtbLogger;
        }

        public static void StartDump(string _strDriveName, DirectoryInfo _di)
        {
            if (bRunning)
                return;

            bRunning = true;

            directoryInfo = _di;
            ClearDiskTree(_strDriveName);

            WorkingThread();
        }

        static FolderTree root;
        static void WorkingThread()
        {
            LogClear();                                     // UI Thread - Log: can be done in a task setup phase
            LogMessage("START THREAD\n");                   // UI Thread - Log
            root = new FolderTree(directoryInfo.Name);
            ShowDiskTree(false);                            // UI Thread - Splitter: can be done in a task setup phase
            DumpDirectory(root, directoryInfo);
            LogMessage("DUMP COMPLETED\n");                 // UI Thread - Log
            SortFolderTree(root);
            LogMessage("SORT COMPLETED\n");                 // UI Thread - Log
            FillTree(treeRootNode, root);                   // UI Thread
            LogMessage("TREE FILLED\n");                    // UI Thread - Log
            bigestSize = ((NodeTag)treeRootNode.Tag).Data;  // UI Thread - But size is a root.size too
            bRunning = false;
            ShowDiskTree(true);                             // UI Thread - Splitter: can be done in a task teardown phase
            LogMessage("FINISH THREAD\n");                  // UI Thread - Log
        }

        private static long DumpDirectory(FolderTree _localRoot, DirectoryInfo _di)
        {
            long DirSize = 0;
            object mutex = new object();
            try
            {
                var DirList = _di.EnumerateDirectories();

                Parallel.ForEach(DirList, di =>
                {
                    FolderTree child;
                    lock (_localRoot)
                        child = _localRoot.AddChild(di.Name);
                    var subDirSize = DumpDirectory(child, di);
                    lock (mutex)
                        DirSize += subDirSize;
                });

                var FileList = _di.EnumerateFiles();
                long TotalFilesSize = 0;
                bool isEmpty = true;
                Parallel.ForEach(FileList, fi =>
                {
                    isEmpty = false;

                    lock (mutex)
                        TotalFilesSize += fi.Length;
                });

                if (!isEmpty)
                {
                    _localRoot.AddChild("/*** Files ***/").size = TotalFilesSize;
                    DirSize += TotalFilesSize;
                }
            }
            catch (PathTooLongException)
            {
                LogWarning($"Path Too Long: {_di.FullName}\n");
            }
            catch (UnauthorizedAccessException)
            {
                //LogWarning($"Unauthorized Access: {_di.FullName}\n");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                LogWarning($"Unknown Exception: {_di.FullName}\n{e}\n");
            }
            finally
            {
                _localRoot.size = DirSize;
            }
            return DirSize;
        }
    }
}
