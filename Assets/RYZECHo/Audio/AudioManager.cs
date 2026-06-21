using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RYZECHo.Audio
{
    /// <summary>
    /// ゲーム全体のサウンド管理を行うマネージャー。
    /// Assets/Audio 配下の WAV を AudioClip としてロードします。
    /// </summary>
    public sealed class AudioManager : IDisposable
    {
        private readonly string _assetRoot;
        private readonly Dictionary<string, AudioClip> _loadedEffects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> _lastPlayedTicks = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _missingEffects = new(StringComparer.OrdinalIgnoreCase);
        private AudioClip _currentMusicClip;
        private AudioSource? _musicSource;
        private string? _currentMusicKey;
        private float _masterVolume = 1.0f;
        private float _sfxVolume = 1.0f;
        private float _bgmVolume = 1.0f;
        private bool _disposed;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
                ApplyMusicVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp(value, 0.0f, 1.0f);
                ApplyMusicVolume();
            }
        }

        public AudioManager(string? assetRoot = null)
        {
            _assetRoot = ResolveAssetRoot(assetRoot);
            _currentMusicClip = null!;
        }

        /// <summary>
        /// 効果音をロードします。失敗したキーは次回以降サイレントにスキップします。
        /// </summary>
        public bool LoadEffect(string key)
        {
            return TryGetEffect(key) is not null;
        }

        public void PreloadEffects(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                LoadEffect(key);
            }
        }

        /// <summary>
        /// 指定されたキーの効果を再生します。
        /// </summary>
        public void PlayEffect(
            string key,
            float volume = 1.0f,
            float pitch = 0.0f,
            float pan = 0.0f,
            float minIntervalSeconds = 0.0f)
        {
            if (_disposed || IsThrottled(key, minIntervalSeconds))
            {
                return;
            }

            var effect = TryGetEffect(key);
            if (effect is null)
            {
                return;
            }

            var effectiveVolume = Mathf.Clamp(volume * _masterVolume * _sfxVolume, 0.0f, 1.0f);
            if (effectiveVolume <= 0.001f)
            {
                return;
            }

            try
            {
                var source = new GameObject($"SFX_{key}").AddComponent<AudioSource>();
                source.clip = effect;
                source.volume = effectiveVolume;
                source.pitch = 1.0f + pitch;
                source.panStereo = Mathf.Clamp(pan, -1.0f, 1.0f);
                source.Play();
                GameObject.Destroy(source, effect.length);
                _lastPlayedTicks[key] = DateTime.UtcNow.Ticks;
            }
            catch
            {
                _missingEffects.Add(key);
            }
        }

        public void PlayMusic(string key, bool loop = true)
        {
            if (_disposed || _currentMusicKey == key && _currentMusicClip != null)
            {
                return;
            }

            StopMusic();

            var effect = TryGetEffect(key);
            if (effect is null)
            {
                return;
            }

            try
            {
                _currentMusicClip = effect;
                _currentMusicKey = key;
                _musicSource = new GameObject("RYZECHo_BGM").AddComponent<AudioSource>();
                _musicSource.clip = effect;
                _musicSource.loop = loop;
                ApplyMusicVolume();
                _musicSource.Play();
            }
            catch
            {
                StopMusic();
                _missingEffects.Add(key);
            }
        }

        /// <summary>
        /// 全ての音を停止します。
        /// </summary>
        public void StopAll()
        {
            StopMusic();
            _lastPlayedTicks.Clear();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
                UnityEngine.Object.Destroy(_musicSource.gameObject);
                _musicSource = null;
            }

            _currentMusicKey = null;
            _currentMusicClip = null!;
        }

        private AudioClip? TryGetEffect(string key)
        {
            if (_loadedEffects.TryGetValue(key, out var loaded))
            {
                return loaded;
            }

            if (_missingEffects.Contains(key))
            {
                return null;
            }

            var path = ResolveEffectPath(key);
            if (!File.Exists(path))
            {
                _missingEffects.Add(key);
                return null;
            }

            try
            {
                var extension = Path.GetExtension(path).ToLowerInvariant();
                var clip = extension == ".wav"
                    ? LoadPcmWave(path, key)
                    : Resources.Load<AudioClip>("Audio/" + key);

                if (clip != null)
                {
                    _loadedEffects[key] = clip;
                }
                return clip;
            }
            catch
            {
                _missingEffects.Add(key);
                return null;
            }
        }

        private bool IsThrottled(string key, float minIntervalSeconds)
        {
            if (minIntervalSeconds <= 0.0f || !_lastPlayedTicks.TryGetValue(key, out var lastTicks))
            {
                return false;
            }

            var elapsedSeconds = (DateTime.UtcNow.Ticks - lastTicks) / (float)TimeSpan.TicksPerSecond;
            return elapsedSeconds < minIntervalSeconds;
        }

        private void ApplyMusicVolume()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = Mathf.Clamp(_masterVolume * _bgmVolume, 0f, 1f);
            }
        }

        private static AudioClip? LoadPcmWave(string path, string key)
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            if (new string(reader.ReadChars(4)) != "RIFF")
            {
                return null;
            }

            reader.ReadInt32();
            if (new string(reader.ReadChars(4)) != "WAVE")
            {
                return null;
            }

            ushort format = 0;
            ushort channels = 0;
            var sampleRate = 0;
            ushort bitsPerSample = 0;
            byte[]? audioData = null;

            while (stream.Position + 8 <= stream.Length)
            {
                var chunkId = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();
                if (chunkSize < 0 || stream.Position + chunkSize > stream.Length)
                {
                    return null;
                }

                if (chunkId == "fmt ")
                {
                    format = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    bitsPerSample = reader.ReadUInt16();
                    stream.Position += chunkSize - 16;
                }
                else if (chunkId == "data")
                {
                    audioData = reader.ReadBytes(chunkSize);
                }
                else
                {
                    stream.Position += chunkSize;
                }

                if ((chunkSize & 1) != 0 && stream.Position < stream.Length)
                {
                    stream.Position++;
                }
            }

            if (audioData is null || channels == 0 || sampleRate <= 0)
            {
                return null;
            }

            float[] samples;
            if (format == 1 && bitsPerSample == 16)
            {
                samples = new float[audioData.Length / 2];
                for (var index = 0; index < samples.Length; index++)
                {
                    samples[index] = BitConverter.ToInt16(audioData, index * 2) / 32768f;
                }
            }
            else if (format == 1 && bitsPerSample == 8)
            {
                samples = new float[audioData.Length];
                for (var index = 0; index < samples.Length; index++)
                {
                    samples[index] = (audioData[index] - 128) / 128f;
                }
            }
            else if (format == 3 && bitsPerSample == 32)
            {
                samples = new float[audioData.Length / 4];
                Buffer.BlockCopy(audioData, 0, samples, 0, audioData.Length);
            }
            else
            {
                return null;
            }

            var sampleFrames = samples.Length / channels;
            var clip = AudioClip.Create("loaded_" + key, sampleFrames, channels, sampleRate, false);
            return clip.SetData(samples, 0) ? clip : null;
        }

        private string ResolveEffectPath(string key)
        {
            var normalizedKey = key
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            if (!Path.HasExtension(normalizedKey))
            {
                normalizedKey += ".wav";
            }

            return Path.IsPathRooted(normalizedKey)
                ? normalizedKey
                : Path.Combine(_assetRoot, normalizedKey);
        }

        private static string ResolveAssetRoot(string? assetRoot)
        {
            var baseDirectoryRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Audio");
            var workingDirectoryRoot = Path.Combine(Environment.CurrentDirectory, "Assets", "Audio");

            var candidates = string.IsNullOrWhiteSpace(assetRoot)
                ? new[] { baseDirectoryRoot, workingDirectoryRoot }
                : new[] { assetRoot, baseDirectoryRoot, workingDirectoryRoot };

            return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            StopMusic();

            foreach (var effect in _loadedEffects.Values)
            {
                // Unity の AudioClip は GC 任せ
            }

            _loadedEffects.Clear();
        }
    }
}
