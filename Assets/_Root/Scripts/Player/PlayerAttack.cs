using UnityEngine;
using UnityEngine.InputSystem;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField]
        private float attackRange = 1f;

        [SerializeField]
        private int attackDamage = 25;

        [SerializeField]
        private LayerMask enemyLayer;

        private HealthBehaviour health;
        private SpriteAnimator spriteAnimator;

        private void Awake()
        {
            health = GetComponent<HealthBehaviour>();
            spriteAnimator = GetComponent<SpriteAnimator>();
        }

        private void Update()
        {
            if (health.CurrentHealth <= 0) return;
            if (spriteAnimator.IsAttacking) return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame) Attack();
        }

        private void Attack()
        {
            spriteAnimator.TriggerAttack();

            var hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
            foreach (var hit in hits)
            {
                var enemyHealth = hit.GetComponent<HealthBehaviour>();
                if (enemyHealth != null) enemyHealth.TakeDamage(attackDamage);
            }
        }
    }
}
