using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform target; // Aquí arrastraremos al Player
    public float smoothSpeed = 0.125f; // Velocidad de suavizado (menor es más lento)
    public Vector3 offset = new Vector3(0, 0, -10f); // Distancia Z para que la cámara no atraviese el mapa

    void LateUpdate()
    {
        if (target != null)
        {
            // Calculamos a dónde debería ir la cámara
            Vector3 desiredPosition = target.position + offset;

            // Suavizamos la transición desde la posición actual hasta la deseada
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        }
    }
}