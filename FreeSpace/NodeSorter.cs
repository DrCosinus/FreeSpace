using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using static FreeSpace.WorkThread;

namespace FreeSpace
{
    internal sealed class NodeSorter : IComparer
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

    internal sealed class FolderTreeSizeSorter : IComparer<FolderTree>
    {
        public int Compare(FolderTree lhs, FolderTree rhs)
        {
            if (rhs == null)
                return lhs == null ? 0 : -1;
            if (lhs == null)
                return 1;
            if (rhs.size > lhs.size)
                return 1;
            else if (rhs.size < lhs.size)
                return -1;
            else
                return lhs.text.CompareTo(rhs.text);
        }
    }
}