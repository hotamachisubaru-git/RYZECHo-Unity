using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using RYZECHo.UI;

namespace RYZECHo
{
    /// <summary>
    /// GameModel の状態を GameUI ビューに反映させるマネージャー。
    /// ドメインロジック (GameModel) とビュー層 (GameUI) を仲介する。
    /// </summary>
    public sealed class GameUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private GameUIConfig _uiConfig;

        private GameUI _gameUI;
        private GameModel _gameModel;

        private readonly Dictionary<string, Color> _actorColorCache = new();

        public void Initialize(GameModel gameModel)
        {
            _gameModel = gameModel;

            if (_uiDocument == null || _uiConfig == null)
            {
                Debug.LogError("[GameUIManager] UIDocument or GameUIConfig not assigned.");
                return;
            }

            _gameUI = new GameUI(_uiDocument, _uiConfig);

            // 初期状態を反映
            UpdateAll();
        }

        /// <summary>
        /// フレーム毎に呼び出して、GameModel の最新状態を UI に反映させる。
        /// </summary>
        public void UpdateAll()
        {
            if (_gameUI == null || _gameModel == null) return;

            // ===== HUD =====
            _gameUI.UpdateScore(_gameModel.Score);
            _gameUI.UpdateTurn(_gameModel.CurrentTurn);
            _gameUI.UpdateTime(_gameModel.RoundTime);

            // ===== Shop =====
            if (_gameModel.CurrentPhase == GamePhase.Construct)
            {
                _gameUI.ShowShopPanel(true);
                UpdateShopPanel();
            }
            else
            {
                _gameUI.ShowShopPanel(false);
            }

            // ===== Pause =====
            _gameUI.ShowPauseOverlay(_gameModel.IsPaused);

            // ===== Briefing =====
            _gameUI.ShowBriefingOverlay(_gameModel.ShowBriefing, _gameModel.BriefingText);

            // ===== Status Effects =====
            UpdateStatusEffects();

            // ===== Actor Labels =====
            UpdateActorLabels();
        }

        private void UpdateShopPanel()
        {
            _gameUI.ClearShopItems();

            if (_gameModel.CurrentPhase != GamePhase.Construct) return;

            var catalog = _gameModel.SelectedLoadoutFocus == Simulation.LoadoutFocus.Primary
                ? GameModel.PrimaryWeaponSelectionOrder()
                : GameModel.SidearmSelectionOrder();

            var selected = _gameModel.SelectedLoadoutWeapon();
            var selectedIndex = System.Array.IndexOf(catalog, selected);
            if (selectedIndex < 0) selectedIndex = 0;

            var cardHeight = 42;
            var visible = Mathf.Clamp((400 - 18) / (cardHeight + 8), 1, Mathf.Min(3, catalog.Length));
            var cardWidth = 320f;

            // ショップタイトル
            var shopTitle = _gameModel.SelectedLoadoutFocus == Simulation.LoadoutFocus.Primary
                ? "メインショップ" : "サブショップ";

            for (var i = 0; i < visible; i++)
            {
                var idx = selectedIndex - visible / 2 + i;
                idx = Mathf.Clamp(idx, 0, catalog.Length - 1);
                var weapon = catalog[idx];
                if (weapon == null) continue;

                var boundsTop = 18 + i * (cardHeight + 8);
                var card = new VisualElement { name = "shop-item" };
                card.style.flexDirection = FlexDirection.Row;
                card.style.paddingLeft = 10f;
                card.style.paddingTop = 6f;
                card.style.paddingBottom = 6f;
                card.style.marginBottom = 4f;
                card.style.backgroundColor = _gameUI != null
                    ? ColorAdapter.FromArgb(120, _gameModel.WeaponAccent(weapon.WeaponType))
                    : Color.FromArgb(70, 16, 24, 30);
                card.style.borderRadius = 6f;

                // Key Label
                var keyLabel = new Label((i + 1).ToString()) { name = "key-label" };
                keyLabel.style.fontSize = 14f;
                keyLabel.style.color = ColorAdapter.FromArgb(255, 245, 220, 155);
                keyLabel.style.width = 24f;
                keyLabel.style.alignSelf = Align.Center;
                card.Add(keyLabel);

                // Title
                var titleLabel = new Label(weapon.Label) { name = "item-title" };
                titleLabel.style.fontSize = 10f;
                titleLabel.style.color = ColorAdapter.FromArgb(240, 238, 244, 248);
                titleLabel.style.marginLeft = 8f;
                card.Add(titleLabel);

                // Subtitle
                var subtitleLabel = new Label($"{weapon.Cost}c / {weapon.MagazineAmmo}+{weapon.ReserveAmmo} / {weapon.VisionClass}視界")
                    { name = "item-subtitle" };
                subtitleLabel.style.fontSize = 9f;
                subtitleLabel.style.color = ColorAdapter.FromArgb(225, 204, 218, 226);
                subtitleLabel.style.marginLeft = 8f;
                card.Add(subtitleLabel);

                _gameUI.AddShopItem((i + 1).ToString(), weapon.Label,
                    $"{weapon.Cost}c / {weapon.MagazineAmmo}+{weapon.ReserveAmmo} / {weapon.VisionClass}視界",
                    idx == selectedIndex, _gameModel.WeaponAccent(weapon.WeaponType));
            }
        }

        private void UpdateStatusEffects()
        {
            if (_gameUI == null) return;
            _gameUI.ClearStatusEffects();

            var effects = _gameModel.GetStatusEffects();
            foreach (var effect in effects)
            {
                _gameUI.AddStatusEffect(effect.Label, effect.Color);
            }
        }

        private void UpdateActorLabels()
        {
            if (_gameUI == null) return;
            _gameUI.ClearActorLabels();

            var labels = _gameModel.GetActorLabels();
            foreach (var label in labels)
            {
                _gameUI.AddActorLabel(label.Text, label.Position, label.Color);
            }
        }

        private void OnDestroy()
        {
            _gameUI?.Dispose();
        }
    }
}
