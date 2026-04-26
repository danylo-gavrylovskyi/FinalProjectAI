using CursedDungeon.CoreAI.CellularAutomata;
using CursedDungeon.GameLoop;
using UnityEngine;
using HealthBehaviour = global::CursedDungeon.Health.Health;

namespace CursedDungeon.Enemies
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField]
        private LevelGenerator levelGenerator;

        [SerializeField]
        private GameObject bossPrefab;

        private void Awake()
        {
            levelGenerator.OnLevelReady += SpawnBoss;
        }

        private void OnDestroy()
        {
            levelGenerator.OnLevelReady -= SpawnBoss;
        }

        private void SpawnBoss()
        {
            var go = Instantiate(bossPrefab, levelGenerator.BossSpawn, Quaternion.identity);

            var bossHealth = go.GetComponent<HealthBehaviour>();
            GameManager.Instance.RegisterBossHealth(bossHealth);
            GameManager.Instance.RegisterEnemy();
        }
    }
}
