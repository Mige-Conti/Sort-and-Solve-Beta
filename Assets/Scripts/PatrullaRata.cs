using UnityEngine;

public class PatrullaRata : MonoBehaviour
{
    [Header("Ruta de Patrullaje")]
    [Tooltip("Arrastra aquí los objetos vacíos que servirán como puntos de ruta")]
    public Transform[] puntosDeRuta;
    public float velocidad = 2f;

    private int puntoActual = 0;

    void Update()
    {
        // Si no hay puntos asignados, la rata no se mueve
        if (puntosDeRuta.Length == 0) return;

        // Movemos a la rata hacia el punto actual usando matemáticas de vectores
        transform.position = Vector2.MoveTowards(transform.position, puntosDeRuta[puntoActual].position, velocidad * Time.deltaTime);

        // Si la distancia al punto de destino es casi cero, cambiamos al siguiente punto
        if (Vector2.Distance(transform.position, puntosDeRuta[puntoActual].position) < 0.1f)
        {
            puntoActual++;

            // Si llegamos al final del arreglo de puntos, volvemos al inicio (bucle)
            if (puntoActual >= puntosDeRuta.Length)
            {
                puntoActual = 0;
            }
        }
    }

    // Detectamos el choque sólido con el jugador
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Buscamos el sistema de vidas modular en la escena
            PlayerHealth sistemaVidas = FindFirstObjectByType<PlayerHealth>();

            if (sistemaVidas != null)
            {
                Debug.Log("¡La rata atacó a Jack!");
                // 2. Le pasamos la posición de la rata para calcular el empuje
                sistemaVidas.RecibirDano(transform.position);
            }
            else
            {
                // Fallback: Si no hay sistema de vidas (ej. nivel sin prefab), muerte instantánea
                FindFirstObjectByType<GameManager>()?.MostrarDerrota("¡Jack fue atacado por la rata mutante!");
            }
        }
    }
}