using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// Pensez à checker le site : https://learn.microsoft.com/en-us/archive/blogs/bclteam/long-paths-in-net-part-1-of-3-kim-hamilton

namespace FreeSpace
{
    internal class FolderTree : IEquatable<FolderTree>, IComparable<FolderTree>
    {
        public readonly string text;
        internal long size = 0;
        internal bool reparse_point = false;
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

        internal void Sort()
        {
            Parallel.ForEach(children, child =>
            {
                child.Sort();
            });

            children.Sort();
        }

        private void FillTreeView_R(TreeNode treeLocalRootNode)
        {
            treeLocalRootNode.Tag = new NodeTag(size);

            foreach (var child in children)
            {
                var child_node = treeLocalRootNode.Nodes.Add(child.text);
                child.FillTreeView_R(child_node);
            }
        }

        internal void FillTreeView(TreeNode treeRootNode)
        {
            treeRootNode.TreeView.Invoke((Action<TreeNode>)FillTreeView_R, treeRootNode);
        }

        internal long DumpDirectory(DirectoryInfo _di, FreeSpace form)
        {
            long DirSize = 0;
            object mutex = new object();
            try
            {
                var DirList = _di.EnumerateDirectories();

                Parallel.ForEach(DirList, di =>
                {
                    if ((di.Attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        //LogMessage($"Ignore Reparse point: {di.FullName}\n");
                        return;
                    }
                    FolderTree child;
                    lock (this)
                        child = AddChild(di.Name);
                    var subDirSize = child.DumpDirectory(di, form);
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
                    AddChild("/*** Files ***/").size = TotalFilesSize;
                    DirSize += TotalFilesSize;
                }
            }
            catch (PathTooLongException)
            {
                form.LogWarning($"Path Too Long: {_di.FullName}\n");
            }
            catch (UnauthorizedAccessException)
            {
                //if ((_di.Attributes & FileAttributes.ReparsePoint) != 0)
                //    LogError($"Unauthorized Access: {_di.FullName} [{_di.Attributes}]\n");
                //else
                //    LogWarning($"Unauthorized Access: {_di.FullName} [{_di.Attributes}]\n");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                form.LogWarning($"Unknown Exception: {_di.FullName}\n{e}\n");
            }
            finally
            {
                size = DirSize;
            }
            return DirSize;
        }
    }
}
