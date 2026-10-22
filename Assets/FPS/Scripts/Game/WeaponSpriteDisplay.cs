using UnityEngine;

namespace Unity.FPS.Game
{
    // Base class for a HUD element that shows 2D weapon sprites for the player's weapons.
    // It lives in fps.Game so WeaponController can find it, while the implementation can live in fps.UI
    public abstract class WeaponSpriteDisplay : MonoBehaviour
    {
        // Called when a player weapon becomes the active weapon
        public abstract void ShowWeapon(WeaponController weapon);

        // Called when a player weapon is put away or destroyed. Only hides if it's the weapon being shown
        public abstract void HideWeapon(WeaponController weapon);

        // Called every time the weapon fires
        public abstract void PlayShoot(WeaponController weapon);

        // Called when the weapon starts reloading. The animation should last for the given duration
        public abstract void PlayReload(WeaponController weapon, float duration);
    }
}
