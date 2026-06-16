using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutomaticParking
{
    public sealed class ImmediateMapUi : MonoBehaviour
    {
        private MapManager manager;

        private void Awake()
        {
            manager = GetComponent<MapManager>();
        }

        private void OnGUI()
        {
            if (manager == null)
            {
                return;
            }

            GUI.Box(new Rect(12, 12, 230, 150), "Map controls");

            string activeScene = SceneManager.GetActiveScene().name;
            if (activeScene == "MainMenu")
            {
                if (GUI.Button(new Rect(24, 42, 206, 28), "Mapa 1 - prostopadle")) manager.LoadMap(1);
                if (GUI.Button(new Rect(24, 76, 206, 28), "Mapa 2 - rownolegle")) manager.LoadMap(2);
                if (GUI.Button(new Rect(24, 110, 206, 28), "Mapa 3 - dynamiczna")) manager.LoadMap(3);
                return;
            }

            if (GUI.Button(new Rect(24, 42, 96, 28), "Menu")) manager.LoadMap(0);
            if (GUI.Button(new Rect(132, 42, 96, 28), "Restart")) manager.RestartCurrent();
            if (GUI.Button(new Rect(24, 76, 64, 28), "Map 1")) manager.LoadMap(1);
            if (GUI.Button(new Rect(94, 76, 64, 28), "Map 2")) manager.LoadMap(2);
            if (GUI.Button(new Rect(164, 76, 64, 28), "Map 3")) manager.LoadMap(3);
        }
    }
}
