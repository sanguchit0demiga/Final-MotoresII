using UnityEngine;


public class EnemyShooter : Enemy 
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletForce = 10f;
    private float nextFireTime = 0f;
  

    protected override void Update() 
    {
        base.Update();

        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);


        if (distance <= detectionRange)
        {

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
            }
        }
        else
        {
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        Vector3 targetPlayerPosition = player.position;

        Vector3 direction = (targetPlayerPosition - firePoint.position).normalized;
        firePoint.rotation = Quaternion.LookRotation(direction);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(bullet.transform.forward * bulletForce, ForceMode.Impulse);
            Debug.Log($"[EnemyShooter] Bala instanciada '{bullet.name}' y fuerza aplicada: {bullet.transform.forward * bulletForce}.");
        }
        else
        {
            Debug.LogWarning($"[EnemyShooter] La bala '{bullet.name}' NO TIENE UN RIGIDBODY! La fuerza no se aplicará.", bullet);
        }

        Destroy(bullet, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (player != null && firePoint != null)
        {
            Vector3 targetPlayerPosition = player.position;

            Vector3 directionToPlayer = (targetPlayerPosition - firePoint.position).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePoint.position, targetPlayerPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(firePoint.position, directionToPlayer * 5f);
        }
    }
}