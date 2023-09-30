using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leedong.ZombieAttack
{
    public class Zombie : MonoBehaviour
    {
        private const string LAYER_WALL = "Wall";
        private const int ZOMBIE_HP = 2;

        private Rigidbody2D _rb;
        
        private Transform _target;
        private List<Vector3> _targetPath;

        private float _radius = 0.4f;
        private float _moveSpeed = 2f;
        private float _rotateSpeed = 10f;
        private float _attackRange = 0.75f;
        
        [HideInInspector]
        public Transform Shooters;

        [HideInInspector]
        public ZombieMap GameMap;

        private int _hp = ZOMBIE_HP;

        private int _layerMask;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();

            _layerMask = LayerMask.GetMask(LAYER_WALL);
        }

        private void FixedUpdate()
        {
            if (Shooters == null || GameMap == null)
            {
                return;
            }

            if (_target != null && !_target.gameObject.activeSelf) _target = null;

            // 타겟팅
            if (_target == null)
            {
                _targetPath = null;

                int minCount = 999;

                int childCount = Shooters.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    Transform target = Shooters.transform.GetChild(i);

                    if (!target.gameObject.activeSelf)
                    {
                        continue;
                    }

                    List<Vector3> targetPath = GameMap.GetPath(this.transform.position, target.position);

                    if (targetPath != null)
                    {
                        if (targetPath.Count < minCount)
                        {
                            minCount = targetPath.Count;

                            _target = target;
                            _targetPath = targetPath;
                        }
                    }
                }
            }

            // 추적
            if (_target != null && _targetPath != null)
            {
                Vector3 nextPosition = Vector3.back;

                bool isTargetIndex = false;

                // 빠른 이동 체크
                for (int i = 0; i < _targetPath.Count; i++)
                {
                    Vector2 direction = _targetPath[i] - transform.position;
                    
                    RaycastHit2D hit = Physics2D.CircleCast(transform.position, _radius, direction.normalized, direction.magnitude, _layerMask);

                    if (hit.collider == null)
                    {
                        nextPosition = _targetPath[i];
                        isTargetIndex = i == 0;
                        break;
                    }
                }

                if (nextPosition == Vector3.back)
                {
                    // 빠른 이동 체크 2
                    for (int i = 0; i < _targetPath.Count; i++)
                    {
                        Vector2 direction = _targetPath[i] - transform.position;

                        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, direction.magnitude, _layerMask);

                        if (hit.collider == null)
                        {
                            nextPosition = _targetPath[i];
                            isTargetIndex = i == 0;
                            break;
                        }
                    }
                }

                if (nextPosition != Vector3.back)
                {
                    Vector2 direction = (nextPosition - this.transform.position).normalized;

                    // 이동
                    if (Vector2.Distance(this.transform.position, _target.transform.position) > _attackRange || !isTargetIndex)
                    {
                        _rb.velocity = direction * _moveSpeed;
                        // this.transform.position = Vector3.MoveTowards(this.transform.position, nextPosition, Time.deltaTime * _moveSpeed);
                    }

                    // 회전
                    if (this.transform.position != nextPosition)
                    {
                        // Calculate the rotation angle from the direction vector (https://unity-programmer.tistory.com/30)
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                        // Smoothly rotate towards the target rotation
                        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, angle), Time.deltaTime * _rotateSpeed);
                    }
                }
                else
                {
                    _target = null;
                }
            }
        }

        public void Init()
        {
            _hp = ZOMBIE_HP;

            _target = null;
            _targetPath = null;
        }

        public void Damage()
        {
            _hp--;

            if (_hp <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}