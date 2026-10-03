using UnityEngine;

public class TrampaDanina : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Buscamos el sistema de vidas en la escena
            PlayerHealth sistemaVidas = FindFirstObjectByType<PlayerHealth>();

            if (sistemaVidas != null)
            {
                // Le mandamos la posición del ácido/rata para saber hacia dónde empujar a Jack
                sistemaVidas.RecibirDano(transform.position);
            }
        }
    }

    // Ponemos también OnTrigger por si usas colisionadores "Is Trigger"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth sistemaVidas = FindFirstObjectByType<PlayerHealth>();
            if (sistemaVidas != null)
            {
                sistemaVidas.RecibirDano(transform.position);
            }
        }
    }
}