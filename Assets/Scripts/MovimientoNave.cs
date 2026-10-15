using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class MovimientoNave : MonoBehaviour 
{
    [Header("Velocidades")]
    public float velocidadAvance = 150f;
    public float velocidadLateral = 80f;
    public float velocidadVertical = 30f;
    private float posicionZInicial = 0f;

    [Header("Referencias")]
    public Transform modeloHijo;
    public TextMeshProUGUI textoPuntos;
    public TextMeshProUGUI textoInicio;

    [Header("Límites")]
    public float alturaMinima = 3f;
    public float alturaMaxima = 25f;

    [Header("Efectos de Cámara")]
    public Camera camaraPrincipal;
    public float fovMinimo = 30f;
    public float fovMaximo = 120f;
    public float velocidadParaFovMax = 300f; // A qué velocidad el FOV será máximo
    private Vector3 offsetOriginalCamara; // La distancia cámara-nave
    private float balanceoSuaveCamara;

    [Header("Oscilación de Vuelo")]
    public float intensidadOscilacion = 0.5f; // Cuánto se mueve
    public float rapidezOscilacion = 1.5f;    // Cómo de rápido cambia el viento

    [Header("Efectos")]
    public GameObject efectoExplosion;
    public GameObject efectoMoneda;

    [Header("Menús")]
    public GameObject menuInicio;
    public GameObject panelDerrota;
    public GameObject panelPausa;
    public TextMeshProUGUI textoPuntosFinal;
    public TextMeshProUGUI textoRecordFinal;
    public TextMeshProUGUI textoRecordInicio;

    private bool puedePuntuar = false;
    private int puntosPorMonedas = 0;
    private bool estaPausado = false;
    // Variable estática para recordar si venimos de un "Retry"
    private static bool saltarMenuInicio = false;

    void Start()
    {
        // Si venimos de pulsar RETRY en el Game Over
        if (saltarMenuInicio)
        {
            saltarMenuInicio = false; // Reseteamos la bandera para la próxima vez
            menuInicio.SetActive(false);
            Time.timeScale = 1f; // Reanudamos el tiempo
            
            if (camaraPrincipal != null) offsetOriginalCamara = camaraPrincipal.transform.localPosition;
            
            StartCoroutine(CuentaAtrasInicio()); // Empieza la cuenta atrás directo
        }
        else
        {
            Time.timeScale = 0f; // Juego pausado al empezar
            menuInicio.SetActive(true);

            // Cargar récord guardado al abrir el juego
            int recordActual = PlayerPrefs.GetInt("HighScore", 0);
            if (textoRecordInicio != null)
            {
                textoRecordInicio.text = "High Score: " + recordActual;
            }

            puedePuntuar = false;
            if (camaraPrincipal != null) offsetOriginalCamara = camaraPrincipal.transform.localPosition;        
        }
    }

    
    void Update()
    {
        // Pausar y reanudar el juego con Escape o P
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (panelDerrota.activeSelf || menuInicio.activeSelf) return; // No pausar en menús

            if (estaPausado) ReanudarJuego();
            else PausarJuego();
        }

        float movH = Input.GetAxis("Horizontal");
        float movV = Input.GetAxis("Vertical");

        // 1. Calculamos el movimiento (SIN PAREDES INVISIBLES en X)
        Vector3 movimientoFrame = new Vector3(movH * velocidadLateral, movV * velocidadVertical, velocidadAvance);
        transform.position += movimientoFrame * Time.deltaTime;

        // 2. Solo limitamos la altura (Y)
        float yLimitada = Mathf.Clamp(transform.position.y, alturaMinima, alturaMaxima);
        transform.position = new Vector3(transform.position.x, yLimitada, transform.position.z);

        // 3. Inclinación visual
        if (modeloHijo != null)
        {
            float inclinacionRoll = -movH * 30f;
            modeloHijo.localRotation = Quaternion.Lerp(modeloHijo.localRotation, Quaternion.Euler(-inclinacionRoll, 90f, 0f), Time.deltaTime * 5f);
        }
        
        velocidadAvance += 4f * Time.deltaTime;
        if (puedePuntuar){
            // 1. Efecto FOV (el que ya tienes)
            float porcentajeVelocidad = Mathf.InverseLerp(100f, velocidadParaFovMax, velocidadAvance);
            camaraPrincipal.fieldOfView = Mathf.Lerp(fovMinimo, fovMaximo, porcentajeVelocidad);

            // 2. Oscilación suave (Simulación de ráfagas de viento)
            // Usamos el tiempo para navegar por el ruido de Perlin
            float semilla = Time.time * rapidezOscilacion;
            float oscilarX = (Mathf.PerlinNoise(semilla, 0f) - 0.5f) * intensidadOscilacion;
            float oscilarY = (Mathf.PerlinNoise(0f, semilla) - 0.5f) * intensidadOscilacion;

            // Aplicamos sobre el offset original para no perder la perspectiva
            camaraPrincipal.transform.localPosition = offsetOriginalCamara + new Vector3(oscilarX, oscilarY, 0);

            // 3. Interpolamos el input para que no sea brusco
            float balanceoObjetivo = -Input.GetAxis("Horizontal") * 5f;
            balanceoSuaveCamara = Mathf.Lerp(balanceoSuaveCamara, balanceoObjetivo, Time.deltaTime * 3f); // El 3f da la suavidad

            // 4. Vaivén de viento (mantenemos el PerlinNoise)
            float vaivenViento = (Mathf.PerlinNoise(Time.time * rapidezOscilacion, 0f) - 0.5f) * intensidadOscilacion * 10f;

            // 5. Aplicamos la rotación combinada
            camaraPrincipal.transform.localRotation = Quaternion.Euler(0, 0, balanceoSuaveCamara + vaivenViento);

            
            ActualizarPuntuacion();
        }
    }


    void ActualizarPuntuacion()
    {
        // Distancia desde que terminó la cuenta atrás
        float distanciaRecorrida = transform.position.z - posicionZInicial;
        
        // Evitamos que la distancia sea negativa si la nave retrocediera por error
        int puntosDistancia = Mathf.Max(0, Mathf.FloorToInt(distanciaRecorrida));
        
        int puntosTotales = puntosDistancia + puntosPorMonedas;

        if (textoPuntos != null)
        {
            textoPuntos.text = puntosTotales + " puntos";
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstaculo"))
        {
            // 1. Explosión en la posición actual
        Instantiate(efectoExplosion, transform.position, Quaternion.identity);

        // 2. Sacudida de cámara violenta
        StartCoroutine(SacudidaChoque(0.5f, 5f)); // 5 grados de sacudida

        // 3. DESACTIVAR SOLO EL MODELO (no el objeto padre)
        if (modeloHijo != null) modeloHijo.gameObject.SetActive(false);

        // 4. Parar el avance del script
        this.enabled = false;

        Invoke("MostrarPantallaDerrota", 1.5f);
            Debug.Log("GAME OVER");
        }
        else if (other.CompareTag("Punto"))
        {
            puntosPorMonedas += 1000; // La moneda suma su valor fijo
            // Creamos un destello donde estaba la moneda
            if (efectoMoneda != null) Instantiate(efectoMoneda, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
        }
    }

    void MostrarPantallaDerrota()
    {
        Time.timeScale = 0f;
        panelDerrota.SetActive(true);
        if (textoPuntos != null) textoPuntos.text = "";
        
        int puntosTotales = Mathf.FloorToInt(transform.position.z - posicionZInicial) + puntosPorMonedas;
        if (textoPuntosFinal != null) textoPuntosFinal.text = "Score: " + puntosTotales;

        int recordAnterior = PlayerPrefs.GetInt("HighScore", 0);

        if (puntosTotales > recordAnterior)
        {
            PlayerPrefs.SetInt("HighScore", puntosTotales);
            PlayerPrefs.Save();
            if (textoRecordFinal != null) textoRecordFinal.text = "NEW HIGH SCORE!";
        }
        else
        {
            if (textoRecordFinal != null) textoRecordFinal.text = "HIGH SCORE: " + recordAnterior;
        }
    }


    // -------------- Botones --------------
    public void BotonJugar() {
        menuInicio.SetActive(false);
        Time.timeScale = 1f; // Arranca el juego
        StartCoroutine(CuentaAtrasInicio()); // Empieza tu cuenta atrás de 3s
    }

    public void BotonRetry() {
        saltarMenuInicio = true; // Indicamos que venimos de un Retry
        Time.timeScale = 1f; // Arranca el juego
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverMenu()
    {
        saltarMenuInicio = false; // Nos aseguramos de que no se salte el menú
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CerrarJuego()
    {
        Debug.Log("Cerrando el proceso del juego...");
        
        // Mata el proceso en una build (ejecutable)
        Application.Quit();

        // Si estás probando en el editor de Unity, esto detiene el Play
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void PausarJuego()
    {
        estaPausado = true;
        Time.timeScale = 0f;
        if (panelPausa != null) panelPausa.SetActive(true);
    }

    public void ReanudarJuego()
    {
        estaPausado = false;
        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    // -------------- Corrutinas --------------

    IEnumerator SacudidaChoque(float duracion, float magnitud) {
        Vector3 posicionOriginal = camaraPrincipal.transform.localPosition;
        float tiempoPasado = 0f;

        while (tiempoPasado < duracion) {
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            camaraPrincipal.transform.localPosition = new Vector3(x, y, posicionOriginal.z);
            tiempoPasado += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }
        camaraPrincipal.transform.localPosition = posicionOriginal;
    }

    IEnumerator CuentaAtrasInicio() 
        {
            float tiempo = 3f;
            while (tiempo > 0) 
            {
                if (textoInicio != null) 
                    textoInicio.text = tiempo.ToString("F1", System.Globalization.CultureInfo.InvariantCulture); // F1 muestra 1 decimal
                
                yield return new WaitForSeconds(0.1f);
                tiempo -= 0.1f;
            }
            textoInicio.text = "";
            textoPuntos.text = "0 puntos";
            posicionZInicial = transform.position.z;
            puedePuntuar = true;
        }
}

