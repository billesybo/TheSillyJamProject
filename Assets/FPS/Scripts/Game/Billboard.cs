using UnityEngine;

namespace Unity.FPS.Game
{
    // Keeps this object's z-axis (forward) pointing directly away from the main camera
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Only rotate around the world up axis, so the object stays upright (Doom style sprites)")]
        public bool LockToYAxis = false;

        Camera m_Camera;

        void LateUpdate()
        {
            // Re-fetch if the main camera changes or hasn't been found yet
            if (m_Camera == null || !m_Camera.isActiveAndEnabled)
                m_Camera = Camera.main;
            if (m_Camera == null)
                return;

            Vector3 awayFromCamera = transform.position - m_Camera.transform.position;
            if (LockToYAxis)
                awayFromCamera.y = 0f;

            if (awayFromCamera.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(awayFromCamera, Vector3.up);
        }
    }
}
