using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Enemies
{
    public class ContactDamagePlayer : MonoBehaviour
    {
        [SerializeField]
        private int damage = 10;

        [SerializeField]
        private float damageCooldown = 1f;

        private float cooldownTimer;
        private HealthBehaviour ownHealth;

        private void Awake()
        {
            ownHealth = GetComponent<HealthBehaviour>();
        }

        private void Update()
        {
            if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (cooldownTimer > 0) return;
            if (ownHealth.CurrentHealth <= 0) return;
            if (!other.CompareTag("Player")) return;

            var playerHealth = other.GetComponent<HealthBehaviour>();
            if (playerHealth == null) return;

            playerHealth.TakeDamage(damage);
            cooldownTimer = damageCooldown;
        }
    }
}
