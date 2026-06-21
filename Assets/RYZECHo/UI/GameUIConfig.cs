using UnityEngine;

namespace RYZECHo.UI
{
    /// <summary>
    /// ScriptableObject for UI visual configuration.
    /// Edit in Inspector to control colors, font sizes, and layout constants.
    /// </summary>
    [CreateAssetMenu(fileName = "GameUIConfig", menuName = "RYZECHo/UI/GameUIConfig")]
    public sealed class GameUIConfig : ScriptableObject
    {
        [Header("HUD Colors")]
        public Color scoreColor = new Color(0.96f, 0.86f, 0.63f, 1f);
        public Color turnColor = new Color(0.93f, 0.96f, 0.97f, 1f);
        public Color timeColor = new Color(0.36f, 0.90f, 0.96f, 1f);

        [Header("Shop Colors")]
        public Color shopBgColor = new Color(0.06f, 0.09f, 0.12f, 0.95f);
        public Color shopBorderColor = new Color(0.30f, 0.43f, 0.49f, 1f);
        public Color shopTitleColor = new Color(0.88f, 0.88f, 0.94f, 1f);
        public Color shopSelectedAccent = new Color(0.30f, 0.43f, 0.49f, 1f);

        [Header("Overlay Colors")]
        public Color pauseOverlayColor = new Color(0.01f, 0.03f, 0.06f, 0.7f);
        public Color pauseTextColor = new Color(0.96f, 0.86f, 0.63f, 1f);
        public Color briefingOverlayColor = new Color(0.01f, 0.03f, 0.06f, 0.85f);
        public Color briefingTextColor = new Color(0.93f, 0.96f, 0.97f, 1f);

        [Header("Font Sizes")]
        public float hudFontSize = 18f;
        public float shopTitleFontSize = 16f;
        public float shopItemTitleFontSize = 10f;
        public float shopItemSubtitleFontSize = 9f;
        public float overlayTitleFontSize = 48f;
        public float overlayTextFontSize = 20f;

        [Header("Layout Constants")]
        public float hudHeight = 60f;
        public float shopPanelWidth = 320f;
        public float shopPanelHeight = 400f;
        public float shopCardHeight = 42f;
        public float shopCardSpacing = 8f;
        public float hudPaddingLeft = 20f;
        public float hudPaddingTop = 10f;
    }
}
