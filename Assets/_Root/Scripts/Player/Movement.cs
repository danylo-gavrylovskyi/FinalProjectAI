using UnityEngine;
using UnityEngine.InputSystem;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Player
{
    public class Movement : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 4f;

        private Rigidbody2D rb;
        private HealthBehaviour health;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthBehaviour>();
        }

        private void FixedUpdate()
        {
            if (health.CurrentHealth <= 0)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            var move = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) move.y += 1f;
            if (Keyboard.current.sKey.isPressed) move.y -= 1f;
            if (Keyboard.current.aKey.isPressed) move.x -= 1f;
            if (Keyboard.current.dKey.isPressed) move.x += 1f;

            // normalize so diagonal isnt faster
            var dir = move.sqrMagnitude > 1f ? move.normalized : move;
            rb.linearVelocity = dir * moveSpeed;
        }
    }
}
