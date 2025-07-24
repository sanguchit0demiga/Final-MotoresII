using UnityEngine;

public class Background : MonoBehaviour
{
    public float scrollSpeed = 0.1f;
    private Material mat;
    private Vector2 offset;

    void Start()
    {
        
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        
        offset.x += scrollSpeed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }
}