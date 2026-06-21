using UnityEngine;
using UnityEngine.UIElements;

namespace RYZECHo.UI
{
    /// <summary>
    /// GameUI 全体のビュー層。UIDocument 経由で UXML/USS を操作する。
    /// ドメインロジック (GameModel) との橋渡し役。
    /// </summary>
    public sealed class GameUI : IDisposable
    {
        private readonly UIDocument _document;
        private readonly GameUIConfig _config;

        // ===== HUD =====
        private Label _scoreLabel;
        private Label _turnLabel;
        private Label _timeLabel;

        // ===== Shop =====
        private VisualElement _shopPanel;
        private VisualElement _shopItems;

        // ===== Overlays =====
        private VisualElement _pauseOverlay;
        private VisualElement _briefingOverlay;

        // ===== Status Effects =====
        private VisualElement _statusEffects;

        // ===== Actor Labels =====
        private VisualElement _actorLabels;

        public GameUI(UIDocument document, GameUIConfig config)
        {
            _document = document;
            _config = config;

            var root = document.rootVisualElement;

            // ===== 参照を取得 =====
            _scoreLabel = root.Q<Label>("score-display");
            _turnLabel = root.Q<Label>("turn-display");
            _timeLabel = root.Q<Label>("time-display");

            _shopPanel = root.Q<VisualElement>("shop-panel");
            _shopItems = _shopPanel.Q<VisualElement>("shop-items");

            _pauseOverlay = root.Q<VisualElement>("pause-overlay");
            _briefingOverlay = root.Q<VisualElement>("briefing-overlay");

            _statusEffects = root.Q<VisualElement>("status-effects");
            _actorLabels = root.Q<VisualElement>("actor-labels");

            // ===== デフォルト状態を適用 =====
            ApplyConfigToElements(root);
        }

        private void ApplyConfigToElements(VisualElement root)
        {
            // HUD
            if (_scoreLabel != null)
            {
                _scoreLabel.style.color = _config.scoreColor;
                _scoreLabel.style.fontSize = _config.hudFontSize;
            }
            if (_turnLabel != null)
            {
                _turnLabel.style.color = _config.turnColor;
                _turnLabel.style.fontSize = _config.hudFontSize - 2f;
            }
            if (_timeLabel != null)
            {
                _timeLabel.style.color = _config.timeColor;
                _timeLabel.style.fontSize = _config.hudFontSize - 2f;
            }

            // Shop
            if (_shopPanel != null)
            {
                _shopPanel.style.backgroundColor = _config.shopBgColor;
                _shopPanel.style.borderColor = new StyleColor(_config.shopBorderColor);
            }

            // Pause
            if (_pauseOverlay != null)
            {
                _pauseOverlay.style.backgroundColor = _config.pauseOverlayColor;
            }

            // Briefing
            if (_briefingOverlay != null)
            {
                _briefingOverlay.style.backgroundColor = _config.briefingOverlayColor;
            }
        }

        // ===== HUD Update =====
        public void UpdateScore(int score)
        {
            _scoreLabel?.SetText($"SCORE: {score}");
        }

        public void UpdateTurn(int turn)
        {
            _turnLabel?.SetText($"TURN: {turn}");
        }

        public void UpdateTime(float seconds)
        {
            var min = (int)seconds / 60;
            var sec = (int)seconds % 60;
            _timeLabel?.SetText($"TIME: {min:D2}:{sec:D2}");
        }

        // ===== Shop =====
        public void ShowShopPanel(bool visible)
        {
            if (_shopPanel != null)
            {
                _shopPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void ClearShopItems()
        {
            _shopItems?.Clear();
        }

        public void AddShopItem(string key, string title, string subtitle, bool selected, Color accent)
        {
            if (_shopItems == null) return;

            var card = new VisualElement { name = "shop-item" };
            card.style.flexDirection = FlexDirection.Row;
            card.style.paddingLeft = 10f;
            card.style.paddingRight = 10f;
            card.style.paddingTop = 6f;
            card.style.paddingBottom = 6f;
            card.style.marginBottom = 4f;
            card.style.backgroundColor = selected ? ColorAdapter.FromArgb(120, accent) : ColorAdapter.FromArgb(70, 16, 24, 30);
            card.style.borderRadius = 6f;
            card.style.unityFontStyleAndWeight = selected ? FontStyle.Bold : FontStyle.Normal;

            // Key Label
            var keyLabel = new Label(key) { name = "key-label" };
            keyLabel.style.fontSize = 14f;
            keyLabel.style.color = ColorAdapter.FromArgb(255, 245, 220, 155);
            keyLabel.style.width = 24f;
            keyLabel.style.alignSelf = Align.Center;
            card.Add(keyLabel);

            // Title
            var titleLabel = new Label(title) { name = "item-title" };
            titleLabel.style.fontSize = _config.shopItemTitleFontSize;
            titleLabel.style.color = ColorAdapter.FromArgb(240, 238, 244, 248);
            titleLabel.style.marginLeft = 8f;
            card.Add(titleLabel);

            // Subtitle
            var subtitleLabel = new Label(subtitle) { name = "item-subtitle" };
            subtitleLabel.style.fontSize = _config.shopItemSubtitleFontSize;
            subtitleLabel.style.color = ColorAdapter.FromArgb(225, 204, 218, 226);
            subtitleLabel.style.marginLeft = 8f;
            card.Add(subtitleLabel);

            _shopItems.Add(card);
        }

        // ===== Pause =====
        public void ShowPauseOverlay(bool visible)
        {
            if (_pauseOverlay != null)
            {
                _pauseOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        // ===== Briefing =====
        public void ShowBriefingOverlay(bool visible, string text)
        {
            if (_briefingOverlay != null)
            {
                _briefingOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                var label = _briefingOverlay.Q<Label>("briefing-text");
                label?.SetText(text);
            }
        }

        // ===== Status Effects =====
        public void AddStatusEffect(string label, Color color)
        {
            if (_statusEffects == null) return;
            var effect = new Label(label) { name = "status-effect" };
            effect.style.fontSize = 12f;
            effect.style.color = color;
            effect.style.marginBottom = 2f;
            _statusEffects.Add(effect);
        }

        public void ClearStatusEffects()
        {
            _statusEffects?.Clear();
        }

        // ===== Actor Labels =====
        public void AddActorLabel(string label, Vector2 position, Color color)
        {
            if (_actorLabels == null) return;
            var actorLabel = new Label(label) { name = "actor-label" };
            actorLabel.style.fontSize = 12f;
            actorLabel.style.color = color;
            actorLabel.style.position = Position.Absolute;
            actorLabel.style.left = position.x;
            actorLabel.style.top = position.y;
            _actorLabels.Add(actorLabel);
        }

        public void ClearActorLabels()
        {
            _actorLabels?.Clear();
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
