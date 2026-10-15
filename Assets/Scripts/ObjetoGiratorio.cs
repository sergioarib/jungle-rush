using UnityEngine;

public class ObjetoGiratorio : MonoBehaviour
{
    public float velocidadGiro = 150f;
    public Vector3 ejeDeGiro = Vector3.up; 

    void Start()
    {
        // OFFSET ALEATORIO: 
        // Cada moneda empieza con una rotación distinta para que no parezcan un ejército sincronizado
        transform.Rotate(ejeDeGiro, Random.Range(0f, 360f));
    }

    void Update()
    {
        // Girar de forma constante
        transform.Rotate(ejeDeGiro * velocidadGiro * Time.deltaTime);
    }
}