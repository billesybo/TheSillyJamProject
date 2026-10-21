using UnityEngine;

namespace Unity.FPS.Game
{
    // Cycles through a set of sprites on a mesh renderer's material, starting from a random one
    public class SpriteRotation : MonoBehaviour
    {
        [Tooltip("Sprites to cycle through")]
        public Sprite[] Sprites;

        [Tooltip("Mesh renderer whose material instance shows the sprite. If empty, it is searched for on this object and its children")]
        public MeshRenderer MeshRenderer;

        [Tooltip("Name of the texture property to replace on the material (URP Lit/Unlit use _BaseMap)")]
        public string BaseMapProperty = "_BaseMap";

        [Tooltip("Seconds before switching to the next sprite")]
        public float TimeBetweenSprites = 0.2f;

        public int CurrentIndex { get; private set; }

        Material m_Material;
        int m_BaseMapId;
        float m_Timer;

        void Awake()
        {
            if (MeshRenderer == null)
                MeshRenderer = GetComponentInChildren<MeshRenderer>();
            DebugUtility.HandleErrorIfNullGetComponent<MeshRenderer, SpriteRotation>(MeshRenderer, this, gameObject);

            // Accessing .material returns this renderer's own instance (creating one if needed),
            // so changing it won't affect other objects sharing the same material asset
            m_Material = MeshRenderer.material;
            m_BaseMapId = Shader.PropertyToID(BaseMapProperty);
        }

        void Start()
        {
            if (Sprites == null || Sprites.Length == 0)
                return;

            CurrentIndex = Random.Range(0, Sprites.Length);
            SetSprite(Sprites[CurrentIndex]);
        }

        void OnDestroy()
        {
            // Clean up the material instance created by MeshRenderer.material
            if (m_Material != null)
                Destroy(m_Material);
        }

        void Update()
        {
            if (Sprites == null || Sprites.Length == 0 || TimeBetweenSprites <= 0f)
                return;

            m_Timer += Time.deltaTime;
            if (m_Timer < TimeBetweenSprites)
                return;

            m_Timer -= TimeBetweenSprites;
            CurrentIndex = (CurrentIndex + 1) % Sprites.Length;
            SetSprite(Sprites[CurrentIndex]);
        }

        void SetSprite(Sprite sprite)
        {
            if (sprite == null || m_Material == null)
                return;

            Texture2D texture = sprite.texture;
            m_Material.SetTexture(m_BaseMapId, texture);

            // Crop to the sprite's rect, so sprites from a sprite sheet/atlas show correctly
            Rect rect = sprite.textureRect;
            Vector2 scale = new Vector2(rect.width / texture.width, rect.height / texture.height);
            Vector2 offset = new Vector2(rect.x / texture.width, rect.y / texture.height);
            m_Material.SetTextureScale(m_BaseMapId, scale);
            m_Material.SetTextureOffset(m_BaseMapId, offset);
        }
    }
}
