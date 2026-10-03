using UnityEngine;

public class TrampaAcido : MonoBehaviour
{
    // Al ser un Trigger, detectará a Jack instantáneamente al sobreponerse
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Buscamos el sistema de vidas modular
            PlayerHealth sistemaVidas = FindFirstObjectByType<PlayerHealth>();

            if (sistemaVidas != null)
            {
                Debug.Log("¡Jack pisó el ácido!");
                // 2. Le pasamos la posición del ácido para el empuje
                sistemaVidas.RecibirDano(transform.position);
            }
            else
            {
                // Fallback de muerte instantánea
                FindFirstObjectByType<GameManager>()?.MostrarDerrota("¡Jack cayó en el ácido tóxico!");
            }
        }
    }
}