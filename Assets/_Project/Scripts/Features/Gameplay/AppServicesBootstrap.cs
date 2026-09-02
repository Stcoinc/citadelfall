using UnityEngine;

namespace ClubGamerZone.TowerDefense.Features.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class AppServicesBootstrap : MonoBehaviour
    {
        private static AppServicesBootstrap _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
