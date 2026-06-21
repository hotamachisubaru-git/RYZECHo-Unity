using RYZECHo.Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace RYZECHo;

public sealed class RyzechoGameController : MonoBehaviour
{
    private static RyzechoGameController? _instance;

    private GameModel? _game;
    private AudioManager? _audioManager;
    private AudioRippleSystem? _audioRipples;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<RyzechoGameController>() != null)
        {
            return;
        }

        var gameObject = new GameObject(nameof(RyzechoGameController));
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<RyzechoGameController>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _game = new GameModel();
        _audioManager = new AudioManager();
        _audioManager.MasterVolume = 0.9f;
        _audioManager.BgmVolume = 0.55f;
        _audioManager.SfxVolume = 0.8f;
        _audioManager.PreloadEffects(SoundEffectCatalog.All);
        _audioManager.PlayMusic(SoundEffectCatalog.BGM_HoloTheme);

        _audioRipples = new AudioRippleSystem(_audioManager);
        _game.AudioCueEmitted += HandleAudioCueEmitted;
    }

    private void Update()
    {
        if (_game is null)
        {
            return;
        }

        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard is null || mouse is null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            _game.IsPaused = !_game.IsPaused;
        }

        if (_game.IsPaused)
        {
            return;
        }

        var mouseValue = mouse.position.ReadValue();
        var mousePosition = new Point(
            Mathf.RoundToInt(mouseValue.x),
            Mathf.RoundToInt(Screen.height - mouseValue.y));

        if (mouse.leftButton.wasPressedThisFrame)
        {
            _game.HandleLeftClick(mousePosition);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            _game.HandleRightClick(mousePosition);
        }

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            _game.CycleBuildTool();
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            _game.ToggleBriefing();
        }

        var input = new InputSnapshot(
            keyboard.wKey.isPressed,
            keyboard.aKey.isPressed,
            keyboard.sKey.isPressed,
            keyboard.dKey.isPressed,
            keyboard.aKey.wasPressedThisFrame,
            keyboard.dKey.wasPressedThisFrame,
            keyboard.enterKey.wasPressedThisFrame,
            NumberPressed(keyboard.digit1Key, keyboard.numpad1Key),
            NumberPressed(keyboard.digit2Key, keyboard.numpad2Key),
            NumberPressed(keyboard.digit3Key, keyboard.numpad3Key),
            NumberPressed(keyboard.digit4Key, keyboard.numpad4Key),
            NumberPressed(keyboard.digit5Key, keyboard.numpad5Key),
            NumberPressed(keyboard.digit6Key, keyboard.numpad6Key),
            keyboard.qKey.wasPressedThisFrame,
            keyboard.eKey.wasPressedThisFrame,
            keyboard.rKey.wasPressedThisFrame,
            keyboard.tKey.wasPressedThisFrame,
            mouse.leftButton.isPressed,
            keyboard.fKey.isPressed,
            mousePosition);

        _game.Update(Mathf.Clamp(Time.deltaTime, 0.001f, 0.05f), input);
    }

    private void OnDestroy()
    {
        if (_game != null)
        {
            _game.AudioCueEmitted -= HandleAudioCueEmitted;
        }

        _audioManager?.Dispose();
        _audioManager = null;
        _audioRipples = null;

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void HandleAudioCueEmitted(RippleKind kind, Vector2 sourcePosition, float strength)
    {
        if (_audioRipples is null || _game is null)
        {
            return;
        }

        var listenerPosition = _game.AudioListenerPosition;
        var dx = sourcePosition.X - listenerPosition.X;
        var dy = sourcePosition.Y - listenerPosition.Y;
        var distance = MathF.Sqrt((dx * dx) + (dy * dy));
        var maxDistance = MathF.Max(1f, GameSettings.SoundMaxDistance * GameLayout.CellSize);
        var attenuation = MathF.Pow(1f - Math.Clamp(distance / maxDistance, 0f, 1f), 0.65f);
        var pan = Math.Clamp(dx / (GameLayout.CellSize * 8f), -1f, 1f);
        _audioRipples.Play(kind, strength * attenuation, pan);
    }

    public GameModel GetGameModel() => _game;

    private static bool NumberPressed(KeyControl topRow, KeyControl numberPad) =>
        topRow.wasPressedThisFrame || numberPad.wasPressedThisFrame;
}
