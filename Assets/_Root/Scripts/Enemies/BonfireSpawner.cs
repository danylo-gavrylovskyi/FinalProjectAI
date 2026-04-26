using CursedDungeon.CoreAI.CellularAutomata;
using UnityEngine;

namespace CursedDungeon.Enemies
{
    public class BonfireSpawner : MonoBehaviour
    {
        [SerializeField]
        private LevelGenerator levelGenerator;

        [SerializeField]
        private GameObject bonfirePrefab;

        private void Awake()
        {
            levelGenerator.OnLevelReady += SpawnBonfires;
        }

        private void OnDestroy()
        {
            levelGenerator.OnLevelReady -= SpawnBonfires;
        }

        private void SpawnBonfires()
        {
            foreach (var point in levelGenerator.BonfirePoints)
                Instantiate(bonfirePrefab, point, Quaternion.identity);
        }
    }
}
