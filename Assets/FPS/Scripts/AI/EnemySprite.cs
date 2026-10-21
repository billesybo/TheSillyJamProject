using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemySprite : MonoBehaviour
    {
        public enum DamageState
        {
            UnDamaged,
            LightlyDamaged,
            Damaged,
            SeverelyDamaged,
        }

        [Tooltip("Health component to listen to. If empty, it is searched for on this object and its parents")]
        public Health Health;

        [Tooltip("Mesh renderer whose material instance shows the enemy sprite")]
        public MeshRenderer MeshRenderer;

        [Tooltip("Name of the texture property to replace on the material (URP Lit/Unlit use _BaseMap)")]
        public string BaseMapProperty = "_BaseMap";

        [Header("Sprites")]
        [Tooltip("Sprites used while undamaged. One is picked at random")]
        public Sprite[] UnDamaged;

        [Tooltip("Sprites used while lightly damaged. One is picked at random")]
        public Sprite[] LightlyDamaged;

        [Tooltip("Sprites used while damaged. One is picked at random")]
        public Sprite[] Damaged;

        [Tooltip("Sprites used while severely damaged. One is picked at random")]
        public Sprite[] SeverelyDamaged;

        [Header("Health thresholds")]
        [Tooltip("Health ratio at or below which the enemy counts as lightly damaged")]
        [Range(0f, 1f)]
        public float LightlyDamagedRatio = 0.99f;

        [Tooltip("Health ratio at or below which the enemy counts as damaged")]
        [Range(0f, 1f)]
        public float DamagedRatio = 0.6f;

        [Tooltip("Health ratio at or below which the enemy counts as severely damaged")]
        [Range(0f, 1f)]
        public float SeverelyDamagedRatio = 0.3f;

        public DamageState CurrentState { get; private set; }

        Material m_Material;
        int m_BaseMapId;

        void Awake()
        {
            if (Health == null)
                Health = GetComponentInParent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemySprite>(Health, this, gameObject);

            if (MeshRenderer == null)
                MeshRenderer = GetComponentInChildren<MeshRenderer>();
            DebugUtility.HandleErrorIfNullGetComponent<MeshRenderer, EnemySprite>(MeshRenderer, this, gameObject);

            // Accessing .material returns this renderer's own instance (creating one if needed),
            // so changing it won't affect other enemies sharing the same material asset
            m_Material = MeshRenderer.material;
            m_BaseMapId = Shader.PropertyToID(BaseMapProperty);
        }

        void OnEnable()
        {
            if (Health != null)
            {
                Health.OnDamaged += OnDamaged;
                Health.OnHealed += OnHealed;
            }
        }

        void OnDisable()
        {
            if (Health != null)
            {
                Health.OnDamaged -= OnDamaged;
                Health.OnHealed -= OnHealed;
            }
        }

        void Start()
        {
            CurrentState = DamageState.UnDamaged;
            ShowRandomSprite(CurrentState);
        }

        void OnDestroy()
        {
            // Clean up the material instance created by MeshRenderer.material
            if (m_Material != null)
                Destroy(m_Material);
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            UpdateState();
        }

        void OnHealed(float amount)
        {
            UpdateState();
        }

        void UpdateState()
        {
            DamageState newState = GetStateForRatio(Health.GetRatio());
            if (newState == CurrentState)
                return;

            CurrentState = newState;
            ShowRandomSprite(CurrentState);
        }

        DamageState GetStateForRatio(float ratio)
        {
            if (ratio <= SeverelyDamagedRatio)
                return DamageState.SeverelyDamaged;
            if (ratio <= DamagedRatio)
                return DamageState.Damaged;
            if (ratio <= LightlyDamagedRatio)
                return DamageState.LightlyDamaged;
            return DamageState.UnDamaged;
        }

        Sprite[] GetSprites(DamageState state)
        {
            switch (state)
            {
                case DamageState.LightlyDamaged: return LightlyDamaged;
                case DamageState.Damaged: return Damaged;
                case DamageState.SeverelyDamaged: return SeverelyDamaged;
                default: return UnDamaged;
            }
        }

        void ShowRandomSprite(DamageState state)
        {
            Sprite[] sprites = GetSprites(state);
            if (sprites == null || sprites.Length == 0)
                return;

            SetSprite(sprites[Random.Range(0, sprites.Length)]);
        }

        public void SetSprite(Sprite sprite)
        {
            if (sprite == null || m_Material == null)
                return;

            Texture2D texture = sprite.texture;
            // m_Material.SetTexture(m_BaseMapId, texture);
            m_Material.mainTexture = texture;

            // Crop to the sprite's rect, so sprites from a sprite sheet/atlas show correctly
            Rect rect = sprite.textureRect;
            Vector2 scale = new Vector2(rect.width / texture.width, rect.height / texture.height);
            Vector2 offset = new Vector2(rect.x / texture.width, rect.y / texture.height);
            m_Material.SetTextureScale(m_BaseMapId, scale);
            m_Material.SetTextureOffset(m_BaseMapId, offset);
        }
    }
}
