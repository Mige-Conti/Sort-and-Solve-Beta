using UnityEngine;

public class InterruptorPresion : MonoBehaviour
{
    [Header("Conexión Lógica")]
    [Tooltip("Arrastra aquí el objeto de la Puerta que este interruptor debe abrir")]
    public GameObject puertaAsociada;

    private int objetosSobrePlaca = 0; // Contador por si el jugador y el bloque están encima al mismo tiempo

    // Detectamos cuando algo pesado entra a la placa
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Basura") || collision.CompareTag("Piedra"))
        {
            objetosSobrePlaca++;
            ActualizarEstadoPuerta();
        }
    }

    // Detectamos cuando el objeto se baja de la placa
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Basura") || collision.CompareTag("Piedra"))
        {
            objetosSobrePlaca--;
            ActualizarEstadoPuerta();
        }
    }

    void ActualizarEstadoPuerta()
    {
        // Si hay al menos 1 objeto pisando el interruptor, la puerta desaparece (se desactiva)
        if (objetosSobrePlaca > 0)
        {
            puertaAsociada.SetActive(false);
            Debug.Log("Interruptor presionado: Puerta Abierta");
        }
        // Si no hay nada pisando, la puerta vuelve a aparecer
        else
        {
            puertaAsociada.SetActive(true);
            Debug.Log("Interruptor liberado: Puerta Cerrada");
        }
    }
}