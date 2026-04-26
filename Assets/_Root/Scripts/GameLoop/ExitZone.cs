using UnityEngine;

namespace CursedDungeon.GameLoop
{
    public class ExitZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (GameManager.Instance == null) return;
            GameManager.Instance.OnExitReached();
        }
    }
}
