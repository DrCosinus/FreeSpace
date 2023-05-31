using System.Collections;
using System.Windows.Forms;

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
}