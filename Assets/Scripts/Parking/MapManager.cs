using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutomaticParking
{
    public sealed class MapManager : MonoBehaviour
    {
        public void LoadMap(int index)
        {
            string sceneName = index switch
            {
                1 => "Map_01_Perpendicular",
                2 => "Map_02_Parallel",
                3 => "Map_03_Dynamic",
                _ => "MainMenu"
            };

            SceneManager.LoadScene(sceneName);
        }

        public void RestartCurrent()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
