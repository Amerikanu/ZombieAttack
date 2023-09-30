using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leedong.ZombieAttack
{
    public class Shooter : MonoBehaviour
    {
        [Header("Equipments")]
        
        [SerializeField]
        private GameObject _muzzle;

        [SerializeField]
        private GameObject _bullet;

        [Header("States")]

        [SerializeField]
        private GameObject _stateIdle;

        [SerializeField]
        private GameObject _stateAttack;

        private const string LAYER_ZOMBIE = "Zombie";
        private const string LAYER_WALL = "Wall";

        private float _attackSpeed = 0.5f; // 초당 2발
        private float _rotateSpeed = 10f;
        private float _bulletSpeed = 10f;

        private HashSet<GameObject> _checkedObjects; // 이미 체크한 오브젝트를 제외하기 위한 HashSet
        private GameObject _targetObject;

        private float _attackRange = 10f;
        private float _attackTimer = 0f;

        private int _layerMask;
        private int _targetLayerMask;

        private void Start()
        {
            _checkedObjects = new HashSet<GameObject>();

            _layerMask = LayerMask.GetMask(LAYER_ZOMBIE, LAYER_WALL);
            _targetLayerMask = LayerMask.GetMask(LAYER_ZOMBIE);
        }

        private void FixedUpdate()
        {
            // 이전 타겟팅 오브젝트 우선 체크
            if (_targetObject != null)
            {
                if (!_targetObject.activeSelf || !CanAttack(_targetObject, _muzzle, true))
                {
                    _targetObject = null;
                }
            }

            // Find Target
            if (_targetObject == null)
            {
                _checkedObjects.Clear();

                // 가장 가까운 공격 가능한 타겟 찾기
                for (float radius = 1f; radius <= _attackRange; radius += 1f)
                {
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, radius, _targetLayerMask);

                    foreach (Collider2D collider in colliders)
                    {
                        GameObject obj = collider.gameObject;

                        // 이미 체크한 오브젝트인 경우 스킵
                        if (_checkedObjects.Contains(obj)) continue;

                        // 이미 체크한 오브젝트가 아니면 HashSet에 추가하고 추가 처리
                        _checkedObjects.Add(obj);

                        if (CanAttack(obj, _muzzle))
                        {
                            _targetObject = obj;
                            break;
                        }
                    }

                    if (_targetObject != null) break;
                }
            }
            
            // Targeting, Attack
            if (_targetObject != null)
            {
                // Rotate
                Vector2 direction = (_targetObject.transform.position - this.transform.position).normalized;

                // Calculate the rotation angle from the direction vector (https://unity-programmer.tistory.com/30)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Smoothly rotate towards the target rotation
                this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, angle), _rotateSpeed * Time.deltaTime);

                _attackTimer += Time.deltaTime;

                if (_attackTimer >= _attackSpeed && Mathf.Abs(this.transform.rotation.z) - Mathf.Abs(angle) < 0.1f)
                {
                    _attackTimer -= _attackSpeed;

                    if (CanAttack(_targetObject, _muzzle))
                    {
                        // Shoot
                        GameObject bullet = Instantiate(_bullet, _muzzle.transform.position, _muzzle.transform.rotation);
                        bullet.gameObject.SetActive(true);
                        Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
                        bulletRB.velocity = _muzzle.transform.right * _bulletSpeed;
                    }
                }
            }
            else
            {
                _attackTimer = 0f;
            }

            // State
            if (_targetObject != null && !_stateAttack.activeSelf)
            {
                _stateIdle.SetActive(false);
                _stateAttack.SetActive(true);
            }
            else if (_targetObject == null && !_stateIdle.activeSelf)
            {
                _stateIdle.SetActive(true);
                _stateAttack.SetActive(false);
            }
        }

        private bool CanAttack(GameObject target, GameObject startPoint, bool isCheckTarget = false)
        {
            // Check Distance
            if (Vector2.Distance(target.transform.position, startPoint.transform.position) > _attackRange)
            {
                return false;
            }

            // Check Raycast
            Vector2 direction = (target.transform.position - startPoint.transform.position).normalized;

            RaycastHit2D hit;

            // CircleCast만 사용하는 경우 State Idle과 State Attack이 번갈아가며 나옴
            if (isCheckTarget)
            {
                hit = Physics2D.Raycast(startPoint.transform.position, direction, Mathf.Infinity, _layerMask);
            }
            else
            {
                hit = Physics2D.CircleCast(startPoint.transform.position, 0.1f, direction, Mathf.Infinity, _layerMask);
            }

            if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer(LAYER_ZOMBIE))
            {
                Debug.DrawRay(startPoint.transform.position, target.transform.position - startPoint.transform.position, Color.green);
                return true;
            }

            Debug.DrawRay(startPoint.transform.position, target.transform.position - startPoint.transform.position, Color.red);
            return false;
        }

    }
}