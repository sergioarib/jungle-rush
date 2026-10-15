using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuNavegacion : MonoBehaviour
{
    [Header("Lista de Botones en orden")]
    public List<BotonArcade> botones;
    
    [Header("Navegación")]
    public bool esNavegacionHorizontal = false; // False = Arriba/Abajo | True = Izquierda/Derecha

    private int indiceSeleccionado = 0;

    void OnEnable()
    {
        // Al activar el menú, seleccionamos el primer botón por defecto
        SeleccionarBotonn(0);
    }

    void Update()
    {
        if (botones == null || botones.Count == 0) return;

        // Detección de teclas de dirección
        if (!esNavegacionHorizontal)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) CambiarSeleccion(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) CambiarSeleccion(1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) CambiarSeleccion(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) CambiarSeleccion(1);
        }

        // Confirmar con Enter o Espacio
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            Button btn = botones[indiceSeleccionado].GetComponent<Button>();
            if (btn != null) btn.onClick.Invoke();
        }
    }

    void CambiarSeleccion(int direccion)
    {
        int nuevoIndice = Mathf.Clamp(indiceSeleccionado + direccion, 0, botones.Count - 1);
        if (nuevoIndice != indiceSeleccionado)
        {
            SeleccionarBotonn(nuevoIndice);
        }
    }

    public void SeleccionarBotonn(int indice)
    {
        for (int i = 0; i < botones.Count; i++)
        {
            if (i == indice)
            {
                botones[i].MarcarComoSeleccionado(true);
            }
            else
            {
                botones[i].MarcarComoSeleccionado(false);
            }
        }
        indiceSeleccionado = indice;
    }

    // Para cuando el ratón pasa por encima de un botón
    public void NotificarHoverRaton(BotonArcade boton)
    {
        int indice = botones.IndexOf(boton);
        if (indice != -1 && indice != indiceSeleccionado)
        {
            SeleccionarBotonn(indice);
        }
    }
}