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
        static bool bRunning = false;
        static Thread dumpThread;
        static TreeNode treeRootNode;
        static TreeView tvDiskDrive;
        static DirectoryInfo directoryInfo;
        static long bigestSize = 1;
        static RichTextBox RTFLogger;
        static SplitContainer splitter;

        public static bool isRunning { get { return bRunning; } }
        public static long BigestSize { get { return bigestSize; } }

        public delegate void Void_String_Callback(string _str);
        public delegate void Void_Font_Color_Callback(Font _font, Color _col);
        public delegate TreeNode TreeNode_TreeNode_String_Callback(TreeNode _tn, string _str);
        public delegate void Void_Void_Callback();
        public delegate void Void_Bool_Callback(bool _b);

        #region DiskDrive TreeView methods
        static void ClearDiskTree(string _strRootName)
        {
            if (tvDiskDrive.InvokeRequired)
                tvDiskDrive.Invoke((Void_String_Callback)ClearDiskTree);
            else
            {
                tvDiskDrive.Nodes.Clear();
                treeRootNode = tvDiskDrive.Nodes.Add(_strRootName);
            }
        }

        static void ShowDiskTree(bool _bEnabled)
        {
            if (splitter.InvokeRequired)
            {
                splitter.Invoke((Void_Bool_Callback)ShowDiskTree, _bEnabled);
            }
            else
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
            }
        }

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

        #region Logger methods
        static void SetLogStyle(Font _font, Color _col)
        {
            if (RTFLogger.InvokeRequired)
                RTFLogger.Invoke((Void_Font_Color_Callback)SetLogStyle, _font, _col);
            else
            {
                RTFLogger.SelectionFont = _font;
                RTFLogger.SelectionColor = _col;
            }
        }

        static void LogAppendText(string _str)
        {
            if (RTFLogger.InvokeRequired)
                RTFLogger.Invoke((Void_String_Callback)LogAppendText, _str);
            else
            {
                RTFLogger.AppendText($"[{DateTime.Now}] {_str}");
                RTFLogger.ScrollToCaret();
            }
        }

        static void LogClear()
        {
            if (RTFLogger.InvokeRequired)
                RTFLogger.Invoke((Void_Void_Callback)LogClear);
            else
            {
                RTFLogger.Clear();
                RTFLogger.ScrollToCaret();
            }
        }

        static readonly Font MessageFont = new Font("Helvetica", 8.25f, FontStyle.Italic);
        static void LogMessage(string message)
        {
            lock (RTFLogger)
            {
                SetLogStyle(MessageFont, Color.Black);
                LogAppendText(message);
            }
        }

        static readonly Font WarningFont = new Font("Helvetica", 8.25f, FontStyle.Regular);
        static void LogWarning(string warning)
        {
            lock (RTFLogger)
            {
                SetLogStyle(WarningFont, Color.DarkBlue);
                LogAppendText(warning);
            }
        }

        static readonly Font ErrorFont = new Font("Helvetica", 8.25f, FontStyle.Bold);
        static void LogError(string error)
        {
            lock (RTFLogger)
            {
                SetLogStyle(ErrorFont, Color.DarkOrange);
                LogAppendText(error);
            }
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

            dumpThread = new Thread(WorkThread.WorkingThread);
            dumpThread.Name = "Dump Thread";
            dumpThread.Start();
        }

        public static void AbortDump()
        {
            LogMessage("ABORTING THREAD\n");
            if (dumpThread != null)
                dumpThread.Abort();
            dumpThread = null;
        }

        static FolderTree root;
        static void WorkingThread()
        {
            LogClear();
            LogMessage("START THREAD\n");
            root = new FolderTree(directoryInfo.Name);
            ShowDiskTree(false);
            DumpDirectory(root, directoryInfo);
            LogMessage("DUMP COMPLETED\n");
            SortFolderTree(root);
            LogMessage("SORT COMPLETED\n");
            FillTree(treeRootNode, root);
            LogMessage("TREE FILLED\n");
            bigestSize = ((NodeTag)treeRootNode.Tag).Data;
            bRunning = false;
            ShowDiskTree(true);
            LogMessage("FINISH THREAD\n");
            dumpThread.Abort();
            dumpThread = null;
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
