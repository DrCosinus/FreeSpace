using System;

namespace FreeSpace
{
    public class NodeTag : Object
    {
        public long Data;
        public NodeTag(long x)
        {
            Data = x;
        }
        public override string ToString()
        {
            long DirSize = Data;
            string SizeString;
            if (DirSize > 1073741824)
                SizeString = String.Format("({0} Go)", DirSize / 1073741824.0f);
            else if (DirSize > 1048576)
                SizeString = String.Format("({0} Mo)", DirSize / 1048576.0f);
            else if (DirSize > 1024)
                SizeString = String.Format("({0} ko)", DirSize / 1024.0f);
            else
                SizeString = String.Format("({0} o)", DirSize);
            return SizeString;
        }
    }
}
