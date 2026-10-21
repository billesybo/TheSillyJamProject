using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.UI
{
    public class PerLetterText : MonoBehaviour
    {
        [Tooltip("The TextMeshPro text to reveal one letter at a time")]
        public TMP_Text Text;

        [Tooltip("Seconds between each letter appearing")]
        public float DelayBetweenLetters = 0.05f;

        [Tooltip("Extra seconds to wait after punctuation like . , ! ?")]
        public float ExtraDelayAfterPunctuation = 0f;

        [Tooltip("Seconds to wait before the first letter appears")]
        public float StartDelay = 0f;

        [Tooltip("Start revealing the text automatically when this component is enabled")]
        public bool PlayOnEnable = true;

        [Tooltip("Use unscaled time, so the text still types out while the game is paused")]
        public bool UseUnscaledTime = true;

        [Header("Audio")]
        [Tooltip("Optional audio source to play a sound when each letter appears")]
        public AudioSource AudioSource;

        [Tooltip("Optional clip to play per letter. If empty, the audio source's own clip is played")]
        public AudioClip LetterSound;

        [Tooltip("Don't play the sound for spaces and line breaks")]
        public bool SkipSoundOnWhitespace = true;

        [Tooltip("Random pitch variation applied to each letter sound (0 = none)")]
        [Range(0f, 0.5f)]
        public float PitchVariation = 0f;

        [Tooltip("Invoked when the full text has been revealed")]
        public UnityEvent OnTextCompleted;

        public bool IsTyping => m_TypeRoutine != null;

        const int AudioFrequency = 3;

        Coroutine m_TypeRoutine;
        float m_BasePitch = 1f;

        void Awake()
        {
            if (Text == null)
                Text = GetComponent<TMP_Text>();

            if (AudioSource != null)
                m_BasePitch = AudioSource.pitch;
        }

        void OnEnable()
        {
            if (PlayOnEnable)
                Play();
        }

        void OnDisable()
        {
            StopTyping();
        }

        // Starts revealing the text currently set on the TextMeshPro component
        public void Play()
        {
            Play(Text.text);
        }

        // Sets new text and starts revealing it
        public void Play(string message)
        {
            StopTyping();
            Text.text = message;
            Text.maxVisibleCharacters = 0;
            m_TypeRoutine = StartCoroutine(TypeRoutine());
        }

        // Instantly reveals the rest of the text
        public void Skip()
        {
            if (!IsTyping)
                return;

            StopTyping();
            Text.maxVisibleCharacters = int.MaxValue;
            OnTextCompleted.Invoke();
        }

        void StopTyping()
        {
            if (m_TypeRoutine != null)
            {
                StopCoroutine(m_TypeRoutine);
                m_TypeRoutine = null;
            }
        }

        IEnumerator TypeRoutine()
        {
            // Make sure the text info is up to date so rich text tags aren't counted as letters
            Text.ForceMeshUpdate();
            TMP_TextInfo textInfo = Text.textInfo;
            int totalCharacters = textInfo.characterCount;

            if (StartDelay > 0f)
                yield return Wait(StartDelay);

            for (int i = 1; i <= totalCharacters; i++)
            {
                Text.maxVisibleCharacters = i;

                char character = textInfo.characterInfo[i - 1].character;

                if (i % AudioFrequency == 0)
                {
                    if (!(SkipSoundOnWhitespace && char.IsWhiteSpace(character)))
                        PlayLetterSound();
                }

                float delay = DelayBetweenLetters;
                if (IsPunctuation(character))
                    delay += ExtraDelayAfterPunctuation;

                if (delay > 0f)
                    yield return Wait(delay);
            }

            Text.maxVisibleCharacters = int.MaxValue;
            m_TypeRoutine = null;
            OnTextCompleted.Invoke();
        }

        void PlayLetterSound()
        {
            if (AudioSource == null)
                return;

            AudioClip clip = LetterSound != null ? LetterSound : AudioSource.clip;
            if (clip == null)
                return;

            if (PitchVariation > 0f)
                AudioSource.pitch = m_BasePitch + Random.Range(-PitchVariation, PitchVariation);

            AudioSource.PlayOneShot(clip);
        }

        object Wait(float seconds)
        {
            return UseUnscaledTime ? new WaitForSecondsRealtime(seconds) : new WaitForSeconds(seconds);
        }

        static bool IsPunctuation(char c)
        {
            return c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';';
        }
    }
}
