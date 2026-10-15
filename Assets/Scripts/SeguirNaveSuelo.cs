using UnityEngine;

public class SeguirNaveSuelo : MonoBehaviour
{
    public Transform nave;
    private Material mat;

    [Header("Sensibilidad de Movimiento")]
    public float escalaAvanceZ = -0.0025f; // Más rápido para el frente
    public float escalaLateralX = 0.0000000001f; // Mucho más lento para los lados

    void Start() {
        mat = GetComponent<Renderer>().material;
    }

    void Update() {
        if (nave == null) return;

        // El plano físico SIEMPRE sigue a la nave para no acabarse
        transform.position = new Vector3(nave.position.x, 0, nave.position.z);

        // El truco está aquí: ambos ejes deben ser NEGATIVOS 
        // para que la textura retroceda mientras tú avanzas.
        float offsetX = -nave.position.x * escalaAvanceZ; 
        float offsetZ = -nave.position.z * escalaAvanceZ; 

        if (mat.HasProperty("_BaseMap"))
            mat.SetTextureOffset("_BaseMap", new Vector2(offsetX, offsetZ));
        else
            mat.SetTextureOffset("_MainTex", new Vector2(offsetX, offsetZ));
    }

}