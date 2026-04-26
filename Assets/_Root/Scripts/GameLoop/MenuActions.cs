using UnityEngine;
using UnityEngine.SceneManagement;

namespace CursedDungeon.GameLoop
{
    public class MenuActions : MonoBehaviour
    {
        [SerializeField]
        private string gameSceneName = "Game";

        public void PlayGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
