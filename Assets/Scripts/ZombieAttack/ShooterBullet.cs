using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leedong.ZombieAttack
{
    public class ShooterBullet : MonoBehaviour
    {
        float _lifeTime = 5f;

        void Update()
        {
            _lifeTime -= Time.deltaTime;

            if (_lifeTime < 0f)
            {
                Destroy(gameObject);
            }
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Zombie"))
            {
                Zombie zombie = col.gameObject.GetComponent<Zombie>();
                zombie.Damage();
            }

            Destroy(gameObject);
        }
    }
}

