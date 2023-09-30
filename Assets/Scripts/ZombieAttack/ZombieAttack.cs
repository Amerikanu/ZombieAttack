using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leedong.ZombieAttack
{
    public class ZombieAttack : MonoBehaviour
    {
        [Header("Camera")]

        [SerializeField]
        private Camera _camera;

        [Header("Input")]

        [SerializeField]
        private InputHandler _handler;

        [Header("Components")]

        [SerializeField]
        private ZombieMap _gameMap;

        [SerializeField]
        private Transform _shooters;

        [SerializeField]
        private ZombiePool _zombiePool;

        private void Start()
        {
            InitHandler();

            _gameMap.Init();
            _zombiePool.Init(_shooters, _gameMap);
        }

        private void InitHandler()
        {
            if (_handler != null)
            {
                _handler.OnInputEvent += InputCallback;
            }
        }

        private void InputCallback(Vector2 screenVec, TouchPhase touchPhase)
        {
            if (touchPhase == TouchPhase.Began)
            {
                Vector3Int gridTilePosition = _gameMap.Grid.WorldToCell(_camera.ScreenToWorldPoint(screenVec));

                if (_gameMap.IsGrass(gridTilePosition))
                {
                    Debug.Log("Position : " + gridTilePosition);
                    Vector3 vec = _gameMap.Grid.CellToWorld(gridTilePosition) + new Vector3(0.5f, 0.5f, 0);
                    _zombiePool.CreateZombie(vec);
                }
            }
        }

    }

}
