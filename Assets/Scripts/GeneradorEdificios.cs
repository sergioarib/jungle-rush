using UnityEngine;
using System.Collections.Generic;

public class GeneradorEdificios : MonoBehaviour
{
    public GameObject prefabEdificio;
    public GameObject prefabMoneda;
    public Transform nave;

    [Header("Ajustes de Selva")]
    public float frecuenciaSpawn = 15f; 
    public float radioLateral = 100f;   
    public float dispersionZ = 30f;     

    [Header("Dificultad y Alturas")]
    public float alturaMinimaNave = 3f; // Debe coincidir con el script de la nave
    public float probabilidadEdificioBajo = 0.3f; // 30% de edificios bajos
    public float probabilidadMoneda = 0.5f; // 50% de probabilidad de moneda en edificios bajos

    private float proximaDistanciaSpawn; 
    private List<GameObject> edificiosExistentes = new List<GameObject>();
    private bool juegoIniciado = false;

    void Start()
    {
        proximaDistanciaSpawn = nave.position.z + 500f;
        Invoke("IniciarGeneracion", 1f);
    }

    void IniciarGeneracion() { juegoIniciado = true; }

    void Update()
    {
        if (!juegoIniciado) return;

        if (nave.position.z > proximaDistanciaSpawn - 500f)
        {
            CrearPilarIndividual();
            proximaDistanciaSpawn += frecuenciaSpawn;
        }

        LimpiarEdificiosLejanos();
    }

    void CrearPilarIndividual()
    {
        float angulo = Random.Range(-60f, 60f);
        float radianes = angulo * Mathf.Deg2Rad;

        // Aumentamos a 600m para que aparezcan dentro de la niebla densa
        float spawnX = nave.position.x + (Mathf.Sin(radianes) * 600f);
        float spawnZ = nave.position.z + (Mathf.Cos(radianes) * 600f);

        float alturaFinal;
        // ENUNCIADO: Se toma un número a suertes para la altura
        bool esBajo = Random.value < 0.3f; // 30% de probabilidad de ser bajo

        if (esBajo) {
            // Altura menor que la mínima del avión (alturaMinimaNave suele ser 3)
            alturaFinal = Random.Range(0.5f, 2.5f);
        } else {
            // Altura a suertes entre valores grandes
            alturaFinal = Random.Range(10f, 45f);
        }

        float ancho = Random.Range(5f, 15f);
        GameObject nuevoEdificio = Instantiate(prefabEdificio);
        nuevoEdificio.transform.localScale = new Vector3(ancho, alturaFinal, ancho);
        nuevoEdificio.transform.position = new Vector3(spawnX, alturaFinal / 2f, spawnZ);
        nuevoEdificio.transform.Rotate(0, Random.Range(0, 360f), 0);
        edificiosExistentes.Add(nuevoEdificio);

        if (esBajo && Random.value < 0.5f) {
            Vector3 posMoneda = new Vector3(spawnX, alturaFinal + 20f, spawnZ);
            GameObject nuevaMoneda = Instantiate(prefabMoneda, posMoneda, prefabMoneda.transform.rotation);
            
            // Forzamos escala uniforme 10,10,10 (o la que necesites)
            // Usamos localScale para que sea independiente de lo demás
            nuevaMoneda.transform.localScale = new Vector3(503.21f, 29.31031f, 503.21f); 
            
            edificiosExistentes.Add(nuevaMoneda);
        }
    }
    
    void LimpiarEdificiosLejanos()
    {
        for (int i = edificiosExistentes.Count - 1; i >= 0; i--)
        {
            if (edificiosExistentes[i] != null && edificiosExistentes[i].transform.position.z < nave.position.z - 50f)
            {
                GameObject obj = edificiosExistentes[i];
                edificiosExistentes.RemoveAt(i);
                Destroy(obj);
            }
        }
    }
}
