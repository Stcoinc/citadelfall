using System.Collections;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class MergeInteractionFeedback : MonoBehaviour
    {
        private const int SampleRate = 44100;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _mergeClip;
        [SerializeField] private AudioClip _invalidClip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.7f;

        private static Sprite _burstSprite;

        private void Awake()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource == null)
            {
                Debug.LogError($"{nameof(MergeInteractionFeedback)} requires an authored AudioSource on {name}.", this);
                enabled = false;
                return;
            }

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _mergeClip ??= CreateTone("Merge Chime", 0.22f, true);
            _invalidClip ??= CreateTone("Invalid Merge", 0.16f, false);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }
#endif

        public void PlayMerge(Transform mergedHero)
        {
            Play(_mergeClip);
            if (mergedHero != null)
            {
                StartCoroutine(PunchScale(mergedHero));
                StartCoroutine(ShowBurst(mergedHero.position));
            }
        }

        public void PlayInvalid()
        {
            Play(_invalidClip);
        }

        private void Play(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip, _volume);
            }
        }

        private static IEnumerator PunchScale(Transform subject)
        {
            var baseScale = subject.localScale;
            var peakScale = baseScale * 1.28f;
            const float duration = 0.28f;
            var elapsed = 0f;

            while (subject != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                var pulse = Mathf.Sin(progress * Mathf.PI);
                subject.localScale = Vector3.LerpUnclamped(baseScale, peakScale, pulse);
                yield return null;
            }

            if (subject != null)
            {
                subject.localScale = baseScale;
            }
        }

        private static IEnumerator ShowBurst(Vector3 position)
        {
            var burst = new GameObject("Merge Burst");
            burst.transform.position = new Vector3(position.x, position.y, -2f);
            var renderer = burst.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBurstSprite();
            renderer.sortingOrder = 500;
            var gold = new Color(1f, 0.76f, 0.18f, 0.9f);
            const float duration = 0.34f;
            var elapsed = 0f;

            while (burst != null && elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                burst.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.65f, 1f - Mathf.Pow(1f - progress, 3f));
                renderer.color = new Color(gold.r, gold.g, gold.b, gold.a * (1f - progress));
                yield return null;
            }

            if (burst != null)
            {
                Destroy(burst);
            }
        }

        private static Sprite GetBurstSprite()
        {
            if (_burstSprite != null)
            {
                return _burstSprite;
            }

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime Merge Ring",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var radius = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    var alpha = Mathf.Clamp01(1f - Mathf.Abs(radius - 0.72f) / 0.11f);
                    pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _burstSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            _burstSprite.name = "Runtime Merge Ring";
            return _burstSprite;
        }

        private static AudioClip CreateTone(string clipName, float duration, bool success)
        {
            var sampleCount = Mathf.CeilToInt(duration * SampleRate);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var time = i / (float)SampleRate;
                var progress = i / (float)sampleCount;
                var envelope = Mathf.Sin(progress * Mathf.PI) * (1f - (progress * 0.35f));
                var frequency = success
                    ? (progress < 0.5f ? 660f : 880f)
                    : Mathf.Lerp(190f, 115f, progress);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * (success ? 0.22f : 0.3f);
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
