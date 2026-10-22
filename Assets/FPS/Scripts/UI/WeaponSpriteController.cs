using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    // Shows the active player weapon as a 2D sprite on the HUD (Doom/Wolfenstein style), and plays the
    // weapon's optional walk, shoot and reload animation clips on it.
    // Weapons without a HudSprite use their 3D model instead, and the HUD image is hidden.
    public class WeaponSpriteController : WeaponSpriteDisplay
    {
        enum SpriteState
        {
            Hidden,
            Idle,
            Walk,
            Shoot,
            Reload,
        }

        [Tooltip("UI image the weapon sprite is shown on. Animation clips must be recorded on this image's object")]
        public Image WeaponImage;

        [Tooltip("Horizontal speed the player must move at for the walk animation to play")]
        public float WalkSpeedThreshold = 0.5f;

        WeaponController m_CurrentWeapon;
        CharacterController m_OwnerCharacterController;
        SpriteState m_State = SpriteState.Hidden;
        float m_StateEndTime;

        PlayableGraph m_Graph;
        AnimationPlayableOutput m_Output;
        AnimationClipPlayable m_ClipPlayable;

        void Awake()
        {
            DebugUtility.HandleErrorIfNullGetComponent<Image, WeaponSpriteController>(WeaponImage, this, gameObject);
            if (WeaponImage == null)
                return;

            // Clips are played straight from code through a playable graph, so the Animator needs no controller
            Animator animator = WeaponImage.GetComponent<Animator>();
            if (animator == null)
                animator = WeaponImage.gameObject.AddComponent<Animator>();

            m_Graph = PlayableGraph.Create("WeaponSpriteController");
            m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            m_Output = AnimationPlayableOutput.Create(m_Graph, "WeaponSprite", animator);

            WeaponImage.enabled = false;
        }

        void OnDestroy()
        {
            if (m_Graph.IsValid())
                m_Graph.Destroy();
        }

        public override void ShowWeapon(WeaponController weapon)
        {
            if (WeaponImage == null)
                return;

            m_CurrentWeapon = weapon;
            m_OwnerCharacterController = weapon.Owner ? weapon.Owner.GetComponent<CharacterController>() : null;

            if (!weapon.HasHudSprite)
            {
                SetHidden();
                return;
            }

            WeaponImage.enabled = true;
            m_State = SpriteState.Hidden; // force the idle/walk state to be applied
            UpdateLocomotionState();
        }

        public override void HideWeapon(WeaponController weapon)
        {
            if (WeaponImage == null || weapon != m_CurrentWeapon)
                return;

            m_CurrentWeapon = null;
            m_OwnerCharacterController = null;
            SetHidden();
        }

        public override void PlayShoot(WeaponController weapon)
        {
            if (!IsShowing(weapon) || m_State == SpriteState.Reload || weapon.ShootAnimation == null)
                return;

            m_State = SpriteState.Shoot;
            m_StateEndTime = Time.time + weapon.ShootAnimation.length;
            PlayClip(weapon.ShootAnimation, 1f);
        }

        public override void PlayReload(WeaponController weapon, float duration)
        {
            if (!IsShowing(weapon) || weapon.ReloadAnimation == null)
                return;

            m_State = SpriteState.Reload;
            m_StateEndTime = Time.time + duration;

            // Stretch the clip so it lasts exactly as long as the reload
            float speed = duration > 0f ? weapon.ReloadAnimation.length / duration : 1f;
            PlayClip(weapon.ReloadAnimation, speed);
        }

        void Update()
        {
            if (m_State == SpriteState.Hidden || m_CurrentWeapon == null)
                return;

            if (m_State == SpriteState.Shoot || m_State == SpriteState.Reload)
            {
                if (Time.time >= m_StateEndTime)
                {
                    m_State = SpriteState.Hidden; // force the idle/walk state to be applied
                    UpdateLocomotionState();
                }

                return;
            }

            UpdateLocomotionState();

            // Loop the walk animation, whether or not the clip itself is set to loop
            if (m_State == SpriteState.Walk && m_ClipPlayable.IsValid())
            {
                float length = m_CurrentWeapon.WalkAnimation.length;
                if (length > 0f && m_ClipPlayable.GetTime() >= length)
                    m_ClipPlayable.SetTime(m_ClipPlayable.GetTime() % length);
            }
        }

        bool IsShowing(WeaponController weapon) => weapon == m_CurrentWeapon && m_State != SpriteState.Hidden;

        bool IsOwnerWalking()
        {
            if (m_OwnerCharacterController == null)
                return false;

            Vector3 velocity = m_OwnerCharacterController.velocity;
            velocity.y = 0f;
            return m_OwnerCharacterController.isGrounded && velocity.sqrMagnitude >= WalkSpeedThreshold * WalkSpeedThreshold;
        }

        // Switches between the idle sprite and the walk animation
        void UpdateLocomotionState()
        {
            SpriteState newState = IsOwnerWalking() && m_CurrentWeapon.WalkAnimation != null
                ? SpriteState.Walk
                : SpriteState.Idle;

            if (newState == m_State)
                return;

            m_State = newState;
            if (m_State == SpriteState.Walk)
            {
                PlayClip(m_CurrentWeapon.WalkAnimation, 1f);
            }
            else
            {
                StopClip();
                WeaponImage.sprite = m_CurrentWeapon.HudSprite;
            }
        }

        void SetHidden()
        {
            m_State = SpriteState.Hidden;
            StopClip();
            WeaponImage.enabled = false;
        }

        void PlayClip(AnimationClip clip, float speed)
        {
            StopClip();

            m_ClipPlayable = AnimationClipPlayable.Create(m_Graph, clip);
            m_ClipPlayable.SetSpeed(speed);
            m_Output.SetSourcePlayable(m_ClipPlayable);
            m_Graph.Play();
        }

        void StopClip()
        {
            if (m_Graph.IsValid())
                m_Graph.Stop();

            if (m_ClipPlayable.IsValid())
                m_ClipPlayable.Destroy();
        }
    }
}
