using CursedDungeon.CoreAI.CellularAutomata;
using CursedDungeon.GameLoop;
using UnityEngine;

namespace CursedDungeon.Enemies
{
    public class SkeletonSpawner : MonoBehaviour
    {
        [SerializeField]
        private LevelGenerator levelGenerator;

        [SerializeField]
        private GameObject skeletonPrefab;

        private void Awake()
        {
            levelGenerator.OnLevelReady += SpawnSkeletons;
        }

        private void OnDestroy()
        {
            levelGenerator.OnLevelReady -= SpawnSkeletons;
        }

        private void SpawnSkeletons()
        {
            foreach (var point in levelGenerator.SkeletonSpawnPoints)
            {
                Instantiate(skeletonPrefab, point, Quaternion.identity);
                GameManager.Instance.RegisterEnemy();
            }
        }
    }
}
