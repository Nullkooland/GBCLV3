using System;
using System.Windows.Media;

namespace GBCLV3.Utils
{
    static class ColorUtil
    {
        public static Color LuminanceGamma(Color color, double gamma)
        {
            float Y = 0.299f * color.ScR + 0.587f * color.ScG + 0.114f * color.ScB;
            float U = -0.147f * color.ScR - 0.289f * color.ScG + 0.436f * color.ScB;
            float V = 0.615f * color.ScR - 0.515f * color.ScG - 0.100f * color.ScB;

            Y = (float)Math.Pow(Y, gamma);

            color.ScR = Y + 1.140f * V;
            color.ScG = Y - 0.395f * U - 0.581f * V;
            color.ScB = Y + 2.032f * U;
            color.Clamp();

            return color;
        }
    }
}
