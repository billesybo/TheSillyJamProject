using UnityEngine;

namespace Unity.FPS.Game
{
    // Spins the object around a random axis at a random speed, picked once when enabled
    public class RandomRotator : MonoBehaviour
    {
        [Tooltip("Minimum rotation angle per second")]
        public float MinRotationSpeed = 90f;

        [Tooltip("Maximum rotation angle per second")]
        public float MaxRotationSpeed = 360f;

        [Tooltip("Rotate around the random axis in world space instead of local space")]
        public bool UseWorldSpace = false;

        public Vector3 RotationAxis { get; private set; }
        public float RotationSpeed { get; private set; }

        void OnEnable()
        {
            Randomize();
        }

        // Picks a new random axis and speed
        public void Randomize()
        {
            RotationAxis = Random.onUnitSphere;
            RotationSpeed = Random.Range(MinRotationSpeed, MaxRotationSpeed);
        }

        void Update()
        {
            transform.Rotate(RotationAxis, RotationSpeed * Time.deltaTime, UseWorldSpace ? Space.World : Space.Self);
        }
    }
}
