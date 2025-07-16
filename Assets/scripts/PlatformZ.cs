using UnityEngine;

public class PlatformZ : MonoBehaviour
{
    public float puntoZ1;       
    public float puntoZ2;     
    public float velocidad = 2f;     

    private Vector3 destinoActual;

    void Start()
    {
        destinoActual = new Vector3(transform.position.x, transform.position.y, puntoZ2);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, destinoActual, velocidad * Time.deltaTime);

        
        if (Vector3.Distance(transform.position, destinoActual) < 0.01f)
        {
            if (Mathf.Approximately(destinoActual.z, puntoZ1))
                destinoActual = new Vector3(transform.position.x, transform.position.y, puntoZ2);
            else
                destinoActual = new Vector3(transform.position.x, transform.position.y, puntoZ1);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 puntoInicio = new Vector3(transform.position.x, transform.position.y, puntoZ1);
        Vector3 puntoFin = new Vector3(transform.position.x, transform.position.y, puntoZ2);

        Gizmos.DrawLine(puntoInicio, puntoFin);

        Gizmos.DrawSphere(puntoInicio, 0.2f);
        Gizmos.DrawSphere(puntoFin, 0.2f);
    }
}
