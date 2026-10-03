using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; // Necesario para el AudioMixer

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioMixer audioMixer;
    public AudioSource sfxSource;
    public AudioClip sonidoHover;
    public AudioClip sonidoClick;

    private VisualElement menuBotones;
    private VisualElement panelNiveles;
    private VisualElement panelAjustes;
    private Slider sldMusica;
    private Slider sldEfectos;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        menuBotones = root.Q<VisualElement>("MenuBotones");
        panelNiveles = root.Q<VisualElement>("PanelNiveles");
        panelAjustes = root.Q<VisualElement>("PanelAjustes");

        // --- SISTEMA DE AUDIO (SLIDERS) ---
        sldMusica = root.Q<Slider>("SldVolumenMusica");
        sldEfectos = root.Q<Slider>("SldVolumenEfectos");

        if (sldMusica != null)
        {
            // Cargar volumen guardado (100 por defecto)
            sldMusica.value = PlayerPrefs.GetFloat("VolMusica", 100f);
            sldMusica.RegisterValueChangedCallback(evt => SetVolumen("VolMusica", evt.newValue));
            SetVolumen("VolMusica", sldMusica.value);
        }

        if (sldEfectos != null)
        {
            sldEfectos.value = PlayerPrefs.GetFloat("VolEfectos", 100f);
            sldEfectos.RegisterValueChangedCallback(evt => SetVolumen("VolEfectos", evt.newValue));
            SetVolumen("VolEfectos", sldEfectos.value);
        }

        // --- INYECCIÓN DE SFX A TODOS LOS BOTONES ---
        // Buscamos cualquier botón en la pantalla y le asignamos los sonidos
        root.Query<Button>().ForEach(btn =>
        {
            // Al pasar el ratón por encima (Hover)
            btn.RegisterCallback<PointerEnterEvent>(evt => PlaySFX(sonidoHover));
            // Al hacer clic
            btn.RegisterCallback<ClickEvent>(evt => PlaySFX(sonidoClick));
        });

        // --- NAVEGACIÓN --- (Tu código existente)
        var btnJugar = root.Q<Button>("BtnJugar");
        var btnPruebas = root.Q<Button>("BtnPruebas");
        var btnAjustes = root.Q<Button>("BtnAjustes");
        var btnSalir = root.Q<Button>("BtnSalir");
        var btnMovimiento = root.Q<Button>("BtnMovimiento");
        var btnBaches = root.Q<Button>("BtnBaches");
        var btnAcido = root.Q<Button>("BtnAcido");
        var btnVolver = root.Q<Button>("BtnVolver");
        var btnVolverAjustes = root.Q<Button>("BtnVolverAjustes");

        if (btnJugar != null) btnJugar.SetEnabled(false);
        if (btnSalir != null) btnSalir.clicked += () => Application.Quit();

        if (btnPruebas != null) btnPruebas.clicked += () => {
            menuBotones.style.display = DisplayStyle.None;
            panelNiveles.style.display = DisplayStyle.Flex;
        };
        if (btnVolver != null) btnVolver.clicked += () => {
            panelNiveles.style.display = DisplayStyle.None;
            menuBotones.style.display = DisplayStyle.Flex;
        };

        if (btnAjustes != null) btnAjustes.clicked += () => {
            menuBotones.style.display = DisplayStyle.None;
            panelAjustes.style.display = DisplayStyle.Flex;
        };
        if (btnVolverAjustes != null) btnVolverAjustes.clicked += () => {
            panelAjustes.style.display = DisplayStyle.None;
            menuBotones.style.display = DisplayStyle.Flex;
        };

        if (btnMovimiento != null) btnMovimiento.clicked += () => SceneManager.LoadScene("Prueba_Movimiento");
        if (btnBaches != null) btnBaches.clicked += () => SceneManager.LoadScene("Prueba_Baches");
        if (btnAcido != null) btnAcido.clicked += () => SceneManager.LoadScene("Prueba_Acido");
    }

    // --- MÉTODOS DE AUDIO ---
    private void SetVolumen(string parametro, float valorSlider)
    {
        // 1. Evitamos el 0 absoluto para que no dé error matemático en el logaritmo
        float valorNormalizado = Mathf.Clamp(valorSlider / 100f, 0.0001f, 1f);

        // 2. Convertimos el valor lineal (0 a 1) a decibelios logarítmicos (-80dB a 0dB)
        float volumenDecibelios = Mathf.Log10(valorNormalizado) * 20f;

        // 3. Aplicamos al Mixer y guardamos en el disco duro (PlayerPrefs)
        audioMixer.SetFloat(parametro, volumenDecibelios);
        PlayerPrefs.SetFloat(parametro, valorSlider);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}