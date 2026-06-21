using UnityEngine;
using UnityEngine.UIElements;

namespace RYZECHo.UI
{
    /// <summary>
    /// System.Drawing.Color → UnityEngine.Color 変換ヘルパー
    /// </summary>
    public static class ColorAdapter
    {
        /// <summary>
        /// ARGB (int) を UnityEngine.Color に変換
        /// </summary>
        public static Color FromArgb(int argb)
        {
            return new Color(
                (argb >> 16 & 0xFF) / 255f,
                (argb >> 8 & 0xFF) / 255f,
                (argb & 0xFF) / 255f,
                (argb >> 24 & 0xFF) / 255f);
        }

        /// <summary>
        /// RGB (int) + alpha を UnityEngine.Color に変換
        /// </summary>
        public static Color FromArgb(int rgb, int alpha)
        {
            return new Color(
                (rgb >> 16 & 0xFF) / 255f,
                (rgb >> 8 & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                alpha / 255f);
        }

        /// <summary>
        /// Color (System.Drawing) → UnityEngine.Color (int overload)
        /// </summary>
        public static Color ToColor(int argb) => FromArgb(argb);

        /// <summary>
        /// Color (System.Drawing) → UnityEngine.Color (float overload)
        /// </summary>
        public static Color ToColor(float r, float g, float b, float a)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}
