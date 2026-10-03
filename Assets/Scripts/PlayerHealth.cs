using UnityEngine;
using UnityEngine.UIElements;
using System.Collections; // Necesario para las Corrutinas

public class PlayerHealth : MonoBehaviour
{
    public int vidasMaximas = 3;
    private int vidasActuales;

    [Header("Configuración de Daño")]
    public float tiempoInvulnerabilidad = 1f;
    private bool esInvulnerable = false;

    private VisualElement corazon1;
    private VisualElement corazon2;
    private VisualElement corazon3;

    private Animator playerAnimator;
    private GameManager gameManager;
    private PlayerMovement playerMovement; // Referencia al script de movimiento

    void Start()
    {
        vidasActuales = vidasMaximas;

        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            corazon1 = root.Q<VisualElement>("Corazon1");
            corazon2 = root.Q<VisualElement>("Corazon2");
            corazon3 = root.Q<VisualElement>("Corazon3");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
            playerMovement = player.GetComponent<PlayerMovement>(); // Capturamos el movimiento
        }

        gameManager = FindFirstObjectByType<GameManager>();
    }

    // AHORA RECIBE LA POSICIÓN DEL ENEMIGO PARA SABER HACIA DÓNDE EMPUJAR
    public void RecibirDano(Vector2 posicionAmenaza)
    {
        // Si ya perdió todas las vidas o es invulnerable, ignorar el golpe
        if (vidasActuales <= 0 || esInvulnerable) return;

        vidasActuales--;
        ActualizarUI();

        if (vidasActuales > 0)
        {
            if (playerAnimator != null) playerAnimator.SetTrigger("Damaged");

            // Iniciar empuje y periodo de invulnerabilidad
            if (playerMovement != null) playerMovement.AplicarEmpuje(posicionAmenaza);
            StartCoroutine(RutinaInvulnerabilidad());
        }
        else
        {
            // 🔹 MODIFICACIÓN: En lugar de ejecutar la derrota de golpe, llamamos a la corrutina
            StartCoroutine(RutinaMuerte());
        }
    }

    private void ActualizarUI()
    {
        if (vidasActuales < 3 && corazon3 != null) corazon3.style.display = DisplayStyle.None;
        if (vidasActuales < 2 && corazon2 != null) corazon2.style.display = DisplayStyle.None;
        if (vidasActuales < 1 && corazon1 != null) corazon1.style.display = DisplayStyle.None;
    }

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;
        // Opcional: Aquí podrías hacer que el SpriteRenderer cambie de color a rojo un instante
        yield return new WaitForSeconds(tiempoInvulnerabilidad);
        esInvulnerable = false; // Jack ya puede volver a recibir daño
    }

    // 🔹 NUEVO CÓDIGO: La corrutina que genera la pausa dramática antes de la pantalla de derrota
    private IEnumerator RutinaMuerte()
    {
        // 1. Bloqueamos el movimiento al instante para que el jugador no siga caminando muerto
        if (playerMovement != null) playerMovement.enabled = false;

        // 2. Disparamos la animación de muerte
        if (playerAnimator != null) playerAnimator.SetTrigger("Die");

        // 3. Pausa dramática: Esperamos 1.5 segundos a que la animación termine
        // (Puedes subir o bajar este número si ves que la pantalla aparece muy rápido o muy lento)
        yield return new WaitForSeconds(2.5f);

        // 4. Lanzamos la pantalla de derrota del UI Toolkit
        if (gameManager != null) gameManager.MostrarDerrota("El nivel de daño superó el traje protector.");
    }
}