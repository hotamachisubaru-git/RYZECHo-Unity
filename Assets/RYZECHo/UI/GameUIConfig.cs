using UnityEngine;
using UnityColor = UnityEngine.Color;

namespace RYZECHo.UI;

[CreateAssetMenu(fileName = "GameUIConfig", menuName = "RYZECHo/UI/Game UI Config")]
public sealed class GameUIConfig : ScriptableObject
{
    [Header("Palette")]
    public UnityColor Background = new(0.025f, 0.04f, 0.065f, 0.97f);
    public UnityColor Panel = new(0.055f, 0.085f, 0.12f, 0.96f);
    public UnityColor PanelRaised = new(0.085f, 0.12f, 0.16f, 0.98f);
    public UnityColor Border = new(0.18f, 0.27f, 0.36f, 1f);
    public UnityColor Text = new(0.91f, 0.95f, 0.98f, 1f);
    public UnityColor MutedText = new(0.58f, 0.68f, 0.75f, 1f);
    public UnityColor Cyan = new(0.32f, 0.86f, 0.93f, 1f);
    public UnityColor Gold = new(0.95f, 0.77f, 0.28f, 1f);
    public UnityColor Green = new(0.35f, 0.86f, 0.58f, 1f);
    public UnityColor Red = new(0.94f, 0.34f, 0.31f, 1f);
    public UnityColor Purple = new(0.74f, 0.48f, 0.96f, 1f);

    [Header("Layout")]
    [Min(960)] public int ReferenceWidth = 1440;
    [Min(540)] public int ReferenceHeight = 810;
    [Range(0.7f, 1.5f)] public float UiScale = 1f;
}
