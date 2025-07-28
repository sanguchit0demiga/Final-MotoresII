using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage = 1f;

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }

        Destroy(gameObject);
    }
}
