using System.Collections;
using UnityEngine;

public class GridBlockMovement : MonoBehaviour
{
    [Header("Configuración del Bloque")]
    public bool esPiedra = false;
    public string tachoCorrecto = "TachoAzul";

    [Header("Configuración de Cuadrícula")]
    public float gridSize = 1f;
    public float moveDuration = 0.2f;

    private bool isMoving = false;
    private Vector2 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isMoving)
        {
            Vector2 pushDirection = transform.position - collision.transform.position;

            Vector2 snapDirection = Vector2.zero;
            if (Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.y))
                snapDirection = new Vector2(Mathf.Sign(pushDirection.x), 0);
            else
                snapDirection = new Vector2(0, Mathf.Sign(pushDirection.y));

            Vector2 targetPosition = (Vector2)transform.position + (snapDirection * gridSize);
            CheckTargetAndMove(targetPosition);
        }
    }

    void CheckTargetAndMove(Vector2 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f);

        if (hit != null)
        {
            if (hit.CompareTag("Wall") || hit.CompareTag("Basura") || hit.CompareTag("Piedra"))
            {
                return;
            }

            if (hit.tag.Contains("Tacho"))
            {
                if (hit.CompareTag(tachoCorrecto) && !esPiedra)
                {
                    Debug.Log("¡Correcto! Basura reciclada.");

                    // NUEVO: Avisamos al GameManager que reciclamos una pieza
                    FindAnyObjectByType<GameManager>().RegistrarBasuraReciclada();

                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log("¡Error! Tacho incorrecto.");
                    transform.position = startPosition;
                    return;
                }
            }

            // Lógica predictiva para el Bache
            if (hit.CompareTag("Bache"))
            {
                StartCoroutine(MoveIntoBache(targetPos, hit.gameObject));
                return;
            }

            // Lógica predictiva para el Ácido
            if (hit.CompareTag("Acido"))
            {
                // Reutilizamos el deslizamiento y aplicamos penalización
                StartCoroutine(MoveIntoAcido(targetPos));
                return;
            }
        }

        StartCoroutine(MoveToPosition(targetPos));
    }

    // Corrutina especial para disolver la caja o penalizarla
    IEnumerator MoveIntoAcido(Vector2 target)
    {
        isMoving = true;
        Vector2 start = transform.position;
        float elapsedTime = 0;

        // Deslizamos la caja hacia el ácido
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(start, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;

        Debug.Log("¡El bloque cayó en el ácido! Bloque destruido. Reiniciando nivel...");
        // Como la pieza se perdió, obligamos al jugador a reiniciar el puzzle
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // Corrutina normal
    IEnumerator MoveToPosition(Vector2 target)
    {
        isMoving = true;
        Vector2 start = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(start, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    // Corrutina especial que resuelve la caída después de deslizarse
    IEnumerator MoveIntoBache(Vector2 target, GameObject bache)
    {
        isMoving = true;
        Vector2 start = transform.position;
        float elapsedTime = 0;

        // Deslizamos la caja hacia el bache ignorando que es sólido
        while (elapsedTime < moveDuration)
        {
            transform.position = Vector2.Lerp(start, target, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;

        // Una vez posicionados exactamente sobre el bache, ejecutamos la lógica
        if (esPiedra)
        {
            Debug.Log("¡Bache rellenado! Ahora es suelo transitable.");
            Destroy(bache);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("¡La basura cayó en el bache! Penalización.");
            transform.position = startPosition;
        }
    }
}