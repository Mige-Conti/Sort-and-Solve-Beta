using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// 1. Estructura de Datos para la Novela Visual
[System.Serializable]
public class LineaDialogo
{
    [TextArea(2, 5)] // Esto hace que el cuadro de texto en Unity sea más grande para escribir cómodo
    public string texto;
    public Sprite poseKate; // Aquí arrastraremos la cara feliz, seria, asustada, etc.
}

public class DialogoManager : MonoBehaviour
{
    [Header("Guion del Nivel")]
    public List<LineaDialogo> guion;
    public float velocidadTipeo = 0.03f; // Tiempo entre cada letra

    private UIDocument uiDocument;
    private VisualElement panelDialogo;
    private VisualElement retratoKate;
    private Label txtMensaje;
    private Button btnAvanzar;
    private Label txtIndicador; // Variable agregada para el indicador

    private int indiceLinea = 0;
    private bool escribiendo = false;
    private Coroutine rutinaEscribir;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        panelDialogo = root.Q<VisualElement>("PanelDialogo");
        retratoKate = root.Q<VisualElement>("RetratoKate");
        txtMensaje = root.Q<Label>("txt_MensajeKate");
        btnAvanzar = root.Q<Button>("BtnAvanzarDialogo");

        // Vinculamos el indicador en la UI
        txtIndicador = root.Q<Label>("txt_Indicador");

        // Suscribimos el botón invisible de pantalla completa
        if (btnAvanzar != null)
        {
            btnAvanzar.clicked += AlHacerClic;
        }

        // Si el profesor/diseñador escribió líneas en el guion, iniciamos
        if (guion != null && guion.Count > 0)
        {
            IniciarDialogo();
        }
        else
        {
            panelDialogo.style.display = DisplayStyle.None; // Ocultamos si no hay diálogo
        }
    }

    public void IniciarDialogo()
    {
        panelDialogo.style.display = DisplayStyle.Flex;
        indiceLinea = 0;
        MostrarLineaActual();

        // Opcional: Podríamos buscar al Player y desactivar su script de movimiento aquí
        // GameObject.FindGameObjectWithTag("Player").GetComponent<MovimientoJugador>().enabled = false;
    }

    private void MostrarLineaActual()
    {
        LineaDialogo lineaActual = guion[indiceLinea];

        // 1. Actualizamos la pose de Kate si se asignó una
        if (lineaActual.poseKate != null)
        {
            retratoKate.style.backgroundImage = new StyleBackground(lineaActual.poseKate);
        }

        // 2. Iniciamos el tipeado letra por letra
        if (rutinaEscribir != null) StopCoroutine(rutinaEscribir);
        rutinaEscribir = StartCoroutine(EfectoMaquinaDeEscribir(lineaActual.texto));
    }

    private IEnumerator EfectoMaquinaDeEscribir(string textoCompleto)
    {
        escribiendo = true;
        txtMensaje.text = ""; // Vaciamos el texto anterior

        // Ocultamos el indicador mientras escribe
        if (txtIndicador != null) txtIndicador.style.display = DisplayStyle.None;

        // Recorremos el texto letra por letra
        foreach (char letra in textoCompleto.ToCharArray())
        {
            txtMensaje.text += letra;
            yield return new WaitForSecondsRealtime(velocidadTipeo); // Usamos Realtime porque Time.timeScale es 0
        }

        escribiendo = false; // Terminó de escribir toda la línea

        // Mostramos el indicador cuando termina
        if (txtIndicador != null) txtIndicador.style.display = DisplayStyle.Flex;
    }

    // Lógica del clic estilo Novela Visual
    private void AlHacerClic()
    {
        if (escribiendo)
        {
            // A. Si el jugador hace clic MIENTRAS Kate habla: Se autocompleta el texto (Skip)
            if (rutinaEscribir != null) StopCoroutine(rutinaEscribir);
            txtMensaje.text = guion[indiceLinea].texto;
            escribiendo = false;

            // El jugador saltó el tipeo, mostramos el indicador
            if (txtIndicador != null) txtIndicador.style.display = DisplayStyle.Flex;
        }
        else
        {
            // B. Si ya terminó de hablar y hace clic: Pasamos a la siguiente línea
            indiceLinea++;
            if (indiceLinea < guion.Count)
            {
                MostrarLineaActual();
            }
            else
            {
                FinalizarDialogo(); // Ya no hay más líneas, cerramos el menú
            }
        }
    }

    private void FinalizarDialogo()
    {
        panelDialogo.style.display = DisplayStyle.None;

        // Opcional: Reactivar el script de movimiento
        // GameObject.FindGameObjectWithTag("Player").GetComponent<MovimientoJugador>().enabled = true;
    }
}