using UnityEngine;
// No necesitas 'System' aquí a menos que uses Action directamente en EnemyShooter,
// ya que ya lo tienes en la clase base Enemy.

public class EnemyShooter : Enemy // Asegúrate de que hereda de Enemy
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletForce = 10f;
    private float nextFireTime = 0f;

    // No necesitas sobrescribir Awake() o Die() a menos que EnemyShooter tenga lógica
    // adicional específica al inicio o al morir que no esté en Enemy.
    // Si no los sobrescribes, las versiones de la clase base se usarán.

    protected override void Update() // Usa 'protected override' para sobrescribir el Update de la base
    {
        base.Update(); // Llama al Update de la clase base (para el seguimiento del jugador)

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
        // targetPlayerPosition.y += 1.0f; // Ajusta la altura si las balas no van al centro del jugador

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

    // Esto dibuja el rango de detección y la dirección de disparo en la escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (player != null && firePoint != null)
        {
            Vector3 targetPlayerPosition = player.position;
            // targetPlayerPosition.y += 1.0f; // Si ajustas la altura en Shoot(), hazlo también aquí

            Vector3 directionToPlayer = (targetPlayerPosition - firePoint.position).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(firePoint.position, targetPlayerPosition);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(firePoint.position, directionToPlayer * 5f);
        }
    }
}