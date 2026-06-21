using UnityEngine;
using UnityEngine.UIElements;
using RYZECHo.UI;

namespace RYZECHo
{
    /// <summary>
    /// SampleScene 用の初期化スクリプト。
    /// GameUIManager と GameModel を接続し、UI を有効化する。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class SceneSetup : MonoBehaviour
    {
        [SerializeField] private GameUIManager _gameUIManager;
        [SerializeField] private GameUIConfig _uiConfig;

        private void Awake()
        {
            // GameModel をシングルトンとして登録
            var gameController = FindObjectOfType<RyzechoGameController>();
            if (gameController == null)
            {
                Debug.LogWarning("[SceneSetup] RyzechoGameController not found in scene.");
                return;
            }

            // GameModel を取得して GameUIManager に渡す
            var gameModel = gameController.GetGameModel();
            if (gameModel == null)
            {
                Debug.LogWarning("[SceneSetup] GameModel not available.");
                return;
            }

            if (_gameUIManager != null)
            {
                _gameUIManager.Initialize(gameModel);
            }

            // GameUIConfig が未設定の場合はデフォルトを作成
            if (_uiConfig == null)
            {
                var config = ScriptableObject.CreateInstance<GameUIConfig>();
                config.name = "GameUIConfig";
                _uiConfig = config;

                if (_gameUIManager != null)
                {
                    var field = typeof(GameUIManager).GetField("_uiConfig",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(_gameUIManager, _uiConfig);
                }
            }

            // UI を可視化
            var root = Camera.main?.GetComponent<UIDocument>()?.rootVisualElement;
            if (root != null)
            {
                root.style.display = DisplayStyle.Flex;
            }
        }
    }
}
