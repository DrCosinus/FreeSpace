namespace FreeSpace
{
    public class NodeTag
    {
        public readonly long Size;

        public NodeTag(long x)
        {
            Size = x;
        }

        public override string ToString()
        {
            if (Size > 1073741824)
                return $"({Size / 1073741824.0f} Go)";
            if (Size > 1048576)
                return $"({Size / 1048576.0f} Mo)";
            if (Size > 1024)
                return $"({Size / 1024.0f} ko)";
            return $"({Size} o)";
        }
    }
}
