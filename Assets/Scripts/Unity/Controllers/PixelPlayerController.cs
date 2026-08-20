using ElonLifeSim.Unity.Characters;
using UnityEngine;

namespace ElonLifeSim.Unity.Controllers
{
    /// <summary>
    /// Top-down pixel character controller with idle / walk sprite animation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float walkFps = 8f;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Vector2 _input;
        private Sprite[] _walkFrames;
        private Sprite _idle;
        private float _animTime;
        private int _frame;

        public void SetSprites(Sprite idle, Sprite[] walkFrames)
        {
            _idle = idle;
            _walkFrames = walkFrames != null && walkFrames.Length > 0
                ? walkFrames
                : (idle != null ? new[] { idle } : null);
            if (_sr == null)
                _sr = GetComponent<SpriteRenderer>();
            if (_sr != null && _idle != null)
                _sr.sprite = _idle;
            _animTime = 0f;
            _frame = 0;
        }

        /// <summary>Load and apply idle + walk for a location (and optional reserved act hook).</summary>
        public void ApplyLocation(string locationId, string actId = null)
        {
            var idle = ElonSpriteCatalog.LoadIdle(locationId, actId);
            var walk = ElonSpriteCatalog.LoadWalkCycle(locationId, actId);
            SetSprites(idle, walk);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
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

            if (_sr != null && _input.x != 0f)
                _sr.flipX = _input.x < 0f;

            Animate();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = _input * moveSpeed;
        }

        private void Animate()
        {
            if (_sr == null)
                return;

            bool moving = _input.sqrMagnitude > 0.01f;
            if (!moving || _walkFrames == null || _walkFrames.Length == 0)
            {
                if (_idle != null)
                    _sr.sprite = _idle;
                _animTime = 0f;
                _frame = 0;
                return;
            }

            _animTime += Time.deltaTime * walkFps;
            if (_animTime >= 1f)
            {
                _animTime -= 1f;
                _frame = (_frame + 1) % _walkFrames.Length;
            }

            if (_walkFrames[_frame] != null)
                _sr.sprite = _walkFrames[_frame];
        }
    }
}
