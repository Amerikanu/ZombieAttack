using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leedong.ZombieAttack
{
    public class ZombiePool : MonoBehaviour
    {
        [SerializeField]
        private Zombie _initZombie;

        private Transform _zombies;

        private const int MAX_ZOMBIE_COUNT = 10;

        public void Init(Transform shooters, ZombieMap gameMap)
        {
            _initZombie.Shooters = shooters;
            _initZombie.GameMap = gameMap;
        }

        public void CreateZombie(Vector3 position)
        {
            if (_zombies == null) _zombies = transform;

            if (_zombies.childCount < MAX_ZOMBIE_COUNT + 1) // + _initZombie
            {
                Zombie zombie = Instantiate(_initZombie, transform);
                zombie.transform.position = position;
                zombie.gameObject.SetActive(true);
                return;
            }

            for (int i = 1; i < _zombies.childCount; i++)
            {
                if (!_zombies.GetChild(i).gameObject.activeSelf)
                {
                    Zombie zombie = _zombies.GetChild(i).GetComponent<Zombie>();
                    zombie.transform.position = position;
                    zombie.Init();
                    zombie.gameObject.SetActive(true);
                    break;
                }
            }
        }

    }
}