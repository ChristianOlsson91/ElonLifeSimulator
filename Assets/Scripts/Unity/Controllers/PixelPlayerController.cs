using UnityEngine;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Minimal top-down pixel character controller (placeholder movement for SA scene).
    /// PLACEHOLDER: replace sprite / tune speed when real art arrives.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PixelPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        private Rigidbody2D _rb;
        private Vector2 _input;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            float x = 0f;
            float y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            _input = new Vector2(x, y).normalized;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _input * moveSpeed;
        }
    }
}
