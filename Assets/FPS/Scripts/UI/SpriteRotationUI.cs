using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    // Cycles through a set of sprites on a UI image, starting from a random one.
    // The cycling can be overridden for a while with ForceShowSprite
    public class SpriteRotationUI : MonoBehaviour
    {
        [Tooltip("Sprites to cycle through")]
        public Sprite[] Sprites;

        [Tooltip("UI image that shows the sprite. If empty, it is searched for on this object and its children")]
        public Image Image;

        [Tooltip("Seconds before switching to the next sprite")]
        public float TimeBetweenSprites = 0.2f;

        public int CurrentIndex { get; private set; }
        public bool IsForcingSprite => m_ForcedTimeLeft > 0f;

        float m_Timer;
        float m_ForcedTimeLeft;

        void Awake()
        {
            if (Image == null)
                Image = GetComponentInChildren<Image>();
            DebugUtility.HandleErrorIfNullGetComponent<Image, SpriteRotationUI>(Image, this, gameObject);
        }

        void Start()
        {
            if (Sprites == null || Sprites.Length == 0)
                return;

            CurrentIndex = Random.Range(0, Sprites.Length);
            if (!IsForcingSprite)
                SetSprite(Sprites[CurrentIndex]);
        }

        void Update()
        {
            if (IsForcingSprite)
            {
                m_ForcedTimeLeft -= Time.deltaTime;
                if (m_ForcedTimeLeft > 0f)
                    return;

                // Forced time is over, go back to the normal rotation where it left off
                m_ForcedTimeLeft = 0f;
                m_Timer = 0f;
                if (Sprites != null && Sprites.Length > 0)
                    SetSprite(Sprites[CurrentIndex]);
                return;
            }

            if (Sprites == null || Sprites.Length == 0 || TimeBetweenSprites <= 0f)
                return;

            m_Timer += Time.deltaTime;
            if (m_Timer < TimeBetweenSprites)
                return;

            m_Timer -= TimeBetweenSprites;
            CurrentIndex = (CurrentIndex + 1) % Sprites.Length;
            SetSprite(Sprites[CurrentIndex]);
        }

        // Shows the given sprite for a number of seconds, pausing the normal rotation.
        // Calling it again while a sprite is being forced replaces it and restarts the time
        public void ForceShowSprite(Sprite sprite, float time)
        {
            if (sprite == null || time <= 0f)
                return;

            m_ForcedTimeLeft = time;
            SetSprite(sprite);
        }

        void SetSprite(Sprite sprite)
        {
            if (sprite == null || Image == null)
                return;

            Image.sprite = sprite;
        }
    }
}
