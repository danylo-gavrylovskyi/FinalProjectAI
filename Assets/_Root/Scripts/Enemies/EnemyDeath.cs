using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;
using CursedDungeon.GameLoop;

namespace CursedDungeon.Enemies
{
    public class EnemyDeath : MonoBehaviour
    {
        [SerializeField]
        private float destroyDelay = 1.2f;

        [SerializeField]
        private bool isBoss;

        private HealthBehaviour health;

        private void Awake()
        {
            health = GetComponent<HealthBehaviour>();
        }

        private void OnEnable()
        {
            health.OnDied += OnDied;
        }

        private void OnDisable()
        {
            health.OnDied -= OnDied;
        }

        private void OnDied()
        {
            if (isBoss) GameManager.Instance.OnBossKilled();
            else GameManager.Instance.OnEnemyKilled();

            Destroy(gameObject, destroyDelay);
        }
    }
}
