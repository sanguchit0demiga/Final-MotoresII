using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Enemy : MonoBehaviour
{
    public ParticleSystem deathParticles;
    public Transform player;
    public float detectionRange = 15f;
    public float moveSpeed = 5f;
    private EnemySpawner spawner;
    public int health = 50;
   
    void Start()
    {
        spawner = FindAnyObjectByType<EnemySpawner>();

    }

    protected virtual void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player);
        }
    }

 
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
        {
        spawner?.UnregisterEnemy();
        GetComponent<PowerUpDropper>()?.TryDropPowerUp();

        Instantiate(deathParticles, transform.position, Quaternion.identity);

        Destroy(gameObject);

    }

    }


