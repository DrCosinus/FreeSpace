using System.Drawing;

namespace FreeSpace
{
    internal static class Extensions
    {
        internal static Color Lerp(this Color startColor, Color endColor, float t)
        {
            int r = (int)(startColor.R + (endColor.R - startColor.R) * t);
            int g = (int)(startColor.G + (endColor.G - startColor.G) * t);
            int b = (int)(startColor.B + (endColor.B - startColor.B) * t);

            return Color.FromArgb(r, g, b);
        }
    }
}
