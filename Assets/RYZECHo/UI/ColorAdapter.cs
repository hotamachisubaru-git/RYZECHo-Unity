using UnityColor = UnityEngine.Color;

namespace RYZECHo.UI
{
    /// <summary>
    /// System.Drawing.Color → UnityEngine.Color 変換ヘルパー。
    /// 既存の描画ロジックで使われている RGB 値をそのまま Unity カラーに変換する。
    /// </summary>
    internal static class ColorAdapter
    {
        /// <summary>0-255 の RGB を Unity カラーに変換 (Alpha=255)</summary>
        public static UnityColor ToUnity(int r, int g, int b)
        {
            return new UnityColor(r / 255f, g / 255f, b / 255f, 1f);
        }

        /// <summary>0-255 の RGBA を Unity カラーに変換</summary>
        public static UnityColor ToUnity(int r, int g, int b, int a)
        {
            return new UnityColor(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        /// <summary>0-1 の RGB を Unity カラーに変換</summary>
        public static UnityColor ToUnity(float r, float g, float b)
        {
            return new UnityColor(r, g, b, 1f);
        }

        /// <summary>0-1 の RGBA を Unity カラーに変換</summary>
        public static UnityColor ToUnity(float r, float g, float b, float a)
        {
            return new UnityColor(r, g, b, a);
        }
    }
}
