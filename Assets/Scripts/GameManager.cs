using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; // Vital para el AudioMixer

public class GameManager : MonoBehaviour
{
    [Header("Configuración de Audio")]
    public AudioMixer audioMixer; // Consola de sonido global
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip clipHover;
    public AudioClip clipClick;
    public AudioClip musicaVictoria;
    public AudioClip musicaDerrota;

    private UIDocument uiDocument;
    private VisualElement panelPausa;
    private VisualElement panelVictoria;
    private VisualElement panelDerrota;

    // Contenedores internos de Pausa
    private VisualElement botonesPausa;
    private VisualElement panelAjustesPausa;

    private Label txtMotivo;
    private Button btnPausa;

    private bool juegoPausado = false;
    private bool juegoTerminado = false;
    private int basuraTotal = 0;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // --- REFERENCIAS DE PANELES PRINCIPALES ---
        panelPausa = root.Q<VisualElement>("PanelPausa");
        panelVictoria = root.Q<VisualElement>("PanelVictoria");
        panelDerrota = root.Q<VisualElement>("PanelDerrota");

        // --- REFERENCIAS INTERNAS DE PAUSA ---
        botonesPausa = root.Q<VisualElement>("BotonesPausa");
        panelAjustesPausa = root.Q<VisualElement>("PanelAjustesPausa");

        txtMotivo = root.Q<Label>("txt_Motivo");
        btnPausa = root.Q<Button>("BtnPausa");

        // --- AUDIO: CARGAR Y CONFIGURAR SLIDERS ---
        var sldMusica = root.Q<Slider>("SldVolumenMusica");
        var sldEfectos = root.Q<Slider>("SldVolumenEfectos");

        if (sldMusica != null)
        {
            // Cargar el valor guardado (por defecto 100) y aplicar
            sldMusica.value = PlayerPrefs.GetFloat("VolMusica", 100f);
            sldMusica.RegisterValueChangedCallback(evt => SetVolumen("VolMusica", evt.newValue));
            // Aplicar el volumen inicial al arrancar la escena
            SetVolumen("VolMusica", sldMusica.value);
        }

        if (sldEfectos != null)
        {
            sldEfectos.value = PlayerPrefs.GetFloat("VolEfectos", 100f);
            sldEfectos.RegisterValueChangedCallback(evt => SetVolumen("VolEfectos", evt.newValue));
            SetVolumen("VolEfectos", sldEfectos.value);
        }

        // --- INYECCIÓN SFX A TODOS LOS BOTONES ---
        root.Query<Button>().ForEach(btn =>
        {
            btn.RegisterCallback<PointerEnterEvent>(evt => PlaySFX(clipHover));
            btn.RegisterCallback<ClickEvent>(evt => PlaySFX(clipClick));
        });

        // --- REFERENCIAS DE BOTONES PAUSA ---
        var btnReanudar = root.Q<Button>("BtnReanudar");
        var btnReiniciar = root.Q<Button>("BtnReiniciar");
        var btnAjustesPausa = root.Q<Button>("BtnAjustesPausa");
        var btnVolverAjustes = root.Q<Button>("BtnVolverAjustes");
        var btnMenuPausa = root.Q<Button>("BtnMenuPrincipal");

        // Eventos generales de Pausa
        if (btnPausa != null) btnPausa.clicked += AlternarPausa;
        if (btnReanudar != null) btnReanudar.clicked += AlternarPausa;
        if (btnReiniciar != null) btnReiniciar.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); };
        if (btnMenuPausa != null) btnMenuPausa.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); };

        // Lógica del sub-menú de Ajustes
        if (btnAjustesPausa != null) btnAjustesPausa.clicked += () => {
            if (botonesPausa != null) botonesPausa.style.display = DisplayStyle.None;
            if (panelAjustesPausa != null) panelAjustesPausa.style.display = DisplayStyle.Flex;
        };

        if (btnVolverAjustes != null) btnVolverAjustes.clicked += () => {
            if (panelAjustesPausa != null) panelAjustesPausa.style.display = DisplayStyle.None;
            if (botonesPausa != null) botonesPausa.style.display = DisplayStyle.Flex;
        };

        // --- BOTONES VICTORIA/DERROTA ---
        var btnSiguienteNivel = root.Q<Button>("BtnSiguienteNivel");
        var btnRejugarVictoria = root.Q<Button>("BtnRejugarVictoria");
        var btnMenuVictoria = root.Q<Button>("BtnMenuVictoria");
        var btnReintentar = root.Q<Button>("BtnReintentar");
        var btnMenuDerrota = root.Q<Button>("BtnMenuDerrota");

        if (btnSiguienteNivel != null) btnSiguienteNivel.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); };
        if (btnRejugarVictoria != null) btnRejugarVictoria.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); };
        if (btnMenuVictoria != null) btnMenuVictoria.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); };
        if (btnReintentar != null) btnReintentar.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); };
        if (btnMenuDerrota != null) btnMenuDerrota.clicked += () => { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); };

        // Conteo inicial
        basuraTotal = GameObject.FindGameObjectsWithTag("Basura").Length;
    }

    // --- MÉTODOS DE AUDIO ---
    private void SetVolumen(string parametro, float valorSlider)
    {
        // Convierte el valor del slider (0 a 100) a un valor logarítmico para el AudioMixer (-80dB a 0dB)
        float valorNormalizado = Mathf.Clamp(valorSlider / 100f, 0.0001f, 1f);
        float volumenDecibelios = Mathf.Log10(valorNormalizado) * 20f;

        if (audioMixer != null)
            audioMixer.SetFloat(parametro, volumenDecibelios);

        PlayerPrefs.SetFloat(parametro, valorSlider); // Guarda el valor para la próxima vez
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private void CambiarMusica(AudioClip nuevaMusica)
    {
        if (bgmSource != null && nuevaMusica != null)
        {
            bgmSource.Stop();
            bgmSource.clip = nuevaMusica;
            bgmSource.Play();
        }
    }

    // --- LÓGICA DEL JUEGO ---
    public void AlternarPausa()
    {
        if (juegoTerminado) return;

        juegoPausado = !juegoPausado;
        if (juegoPausado)
        {
            Time.timeScale = 0f;
            if (panelPausa != null) panelPausa.style.display = DisplayStyle.Flex;
            if (btnPausa != null) btnPausa.style.display = DisplayStyle.None;

            // Asegurarnos de que siempre abra en la vista de botones principales
            if (botonesPausa != null) botonesPausa.style.display = DisplayStyle.Flex;
            if (panelAjustesPausa != null) panelAjustesPausa.style.display = DisplayStyle.None;
        }
        else
        {
            Time.timeScale = 1f;
            if (panelPausa != null) panelPausa.style.display = DisplayStyle.None;
            if (btnPausa != null) btnPausa.style.display = DisplayStyle.Flex;
        }
    }

    public void RegistrarBasuraReciclada()
    {
        basuraTotal--;
        Debug.Log("¡Basura reciclada! Faltan: " + basuraTotal);

        if (basuraTotal <= 0)
        {
            MostrarVictoria();
        }
    }

    public void MostrarVictoria()
    {
        juegoTerminado = true;
        Time.timeScale = 0f;
        if (btnPausa != null) btnPausa.style.display = DisplayStyle.None;
        if (panelVictoria != null) panelVictoria.style.display = DisplayStyle.Flex;

        CambiarMusica(musicaVictoria);
    }

    public void MostrarDerrota(string motivo)
    {
        juegoTerminado = true;
        Time.timeScale = 0f;
        if (btnPausa != null) btnPausa.style.display = DisplayStyle.None;
        if (txtMotivo != null) txtMotivo.text = motivo;
        if (panelDerrota != null) panelDerrota.style.display = DisplayStyle.Flex;

        CambiarMusica(musicaDerrota);
    }
}