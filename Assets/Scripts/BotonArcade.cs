using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class BotonArcade : MonoBehaviour, IPointerEnterHandler
{
    private TextMeshProUGUI textoBoton;
    private string textoOriginal;
    private MenuNavegacion menuPadre;

    [Header("Configuración visual")]
    public float opacidadNormal = 0.65f;
    public float opacidadHover = 1.0f;
    public string prefijoCursor = "> ";
    public float velocidadParpadeo = 0.4f;

    private Coroutine corrutinaParpadeo;

    void Awake()
    {
        InicializarComponente();
    }

    private void InicializarComponente()
    {
        if (textoBoton == null)
        {
            textoBoton = GetComponentInChildren<TextMeshProUGUI>();
            menuPadre = GetComponentInParent<MenuNavegacion>();

            if (textoBoton != null && string.IsNullOrEmpty(textoOriginal))
            {
                textoOriginal = textoBoton.text;
            }
        }
    }

    public void MarcarComoSeleccionado(bool seleccionado)
    {
        InicializarComponente();

        if (corrutinaParpadeo != null) StopCoroutine(corrutinaParpadeo);

        if (seleccionado)
        {
            AjustarOpacidad(opacidadHover);
            if (gameObject.activeInHierarchy)
            {
                corrutinaParpadeo = StartCoroutine(EfectoParpadeo());
            }
        }
        else
        {
            if (textoBoton != null) textoBoton.text = textoOriginal;
            AjustarOpacidad(opacidadNormal);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuPadre != null)
        {
            menuPadre.NotificarHoverRaton(this);
        }
    }

    private void AjustarOpacidad(float alfa)
    {
        if (textoBoton == null) return;
        Color c = textoBoton.color;
        c.a = alfa;
        textoBoton.color = c;
    }

    IEnumerator EfectoParpadeo()
    {
        bool mostrarCursor = true;

        while (true)
        {
            if (textoBoton != null)
            {
                if (mostrarCursor)
                {
                    textoBoton.text = prefijoCursor + textoOriginal;
                }
                else
                {
                    textoBoton.text = "<color=#00000000>" + prefijoCursor + "</color>" + textoOriginal;
                }
            }

            mostrarCursor = !mostrarCursor;
            yield return new WaitForSecondsRealtime(velocidadParpadeo);
        }
    }

    void OnDisable()
    {
        if (corrutinaParpadeo != null) StopCoroutine(corrutinaParpadeo);
        if (textoBoton != null) textoBoton.text = textoOriginal;
    }
}