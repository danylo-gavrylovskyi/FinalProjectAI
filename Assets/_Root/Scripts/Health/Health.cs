using System;
using UnityEngine;

namespace CursedDungeon.Health
{
    public class Health : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth = 100;

        private int currentHealth;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDied;

        private void Awake()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= amount;
            if (currentHealth < 0) currentHealth = 0;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0) OnDied?.Invoke();
        }

        public void Heal(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
