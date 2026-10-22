using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    // Makes a SpriteRotationUI briefly show a reaction sprite when the player takes damage or an enemy is killed
    public class SpriteRotationUIReactions : MonoBehaviour
    {
        [Tooltip("The sprite rotation to send the reaction sprites to. If empty, it is searched for on this object and its children")]
        public SpriteRotationUI SpriteRotation;

        [Header("Player damaged")]
        [Tooltip("Sprites shown when the player takes damage. One is picked at random each time")]
        public Sprite[] PlayerDamagedSprites;

        [Tooltip("Seconds the player damaged sprite is shown")]
        public float PlayerDamagedTime = 0.5f;

        [Header("Enemy killed")]
        [Tooltip("Sprites shown when an enemy is killed. One is picked at random each time")]
        public Sprite[] EnemyKilledSprites;

        [Tooltip("Seconds the enemy killed sprite is shown")]
        public float EnemyKilledTime = 1f;

        Health m_PlayerHealth;

        void Awake()
        {
            if (SpriteRotation == null)
                SpriteRotation = GetComponentInChildren<SpriteRotationUI>();
            DebugUtility.HandleErrorIfNullGetComponent<SpriteRotationUI, SpriteRotationUIReactions>(SpriteRotation,
                this, gameObject);

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
        }

        void Start()
        {
            PlayerCharacterController playerCharacterController = FindObjectOfType<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, SpriteRotationUIReactions>(
                playerCharacterController, this);
            if (playerCharacterController == null)
                return;

            m_PlayerHealth = playerCharacterController.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, SpriteRotationUIReactions>(m_PlayerHealth, this,
                playerCharacterController.gameObject);
            if (m_PlayerHealth != null)
                m_PlayerHealth.OnDamaged += OnPlayerDamaged;
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);

            if (m_PlayerHealth != null)
                m_PlayerHealth.OnDamaged -= OnPlayerDamaged;
        }

        void OnPlayerDamaged(float damage, GameObject damageSource)
        {
            ForceShowRandom(PlayerDamagedSprites, PlayerDamagedTime);
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            ForceShowRandom(EnemyKilledSprites, EnemyKilledTime);
        }

        void ForceShowRandom(Sprite[] sprites, float time)
        {
            if (SpriteRotation == null || sprites == null || sprites.Length == 0)
                return;

            SpriteRotation.ForceShowSprite(sprites[Random.Range(0, sprites.Length)], time);
        }
    }
}
