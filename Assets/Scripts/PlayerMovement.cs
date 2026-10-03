using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public InputAction moveAction;

    [Header("Detección de Interacción")]
    public LayerMask capaBasura;

    [Header("Configuración de Daño (Knockback)")] // NUEVO: Las variables que faltaban
    public float fuerzaEmpuje = 6f;
    public float tiempoEmpuje = 0.2f;
    private bool estaSiendoEmpujado = false; // Bloquea el control al recibir daño

    private Rigidbody2D rb;
    private Animator animator;
    private bool isMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void OnEnable() { moveAction.Enable(); }
    void OnDisable() { moveAction.Disable(); }

    void Update()
    {
        // Si ya se está moviendo por la cuadrícula O está saliendo volando por daño, no hacemos nada
        if (isMoving || estaSiendoEmpujado) return;

        Vector2 rawInput = moveAction.ReadValue<Vector2>();
        Vector2 movement = Vector2.zero;

        if (Mathf.Abs(rawInput.x) > 0.1f)
            movement = new Vector2(Mathf.Sign(rawInput.x), 0);
        else if (Mathf.Abs(rawInput.y) > 0.1f)
            movement = new Vector2(0, Mathf.Sign(rawInput.y));

        if (movement != Vector2.zero)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.SetFloat("Speed", 1f);

            // EL RADAR: Disparamos un rayo invisible 1 casilla hacia la dirección que queremos ir
            RaycastHit2D hit = Physics2D.Raycast(rb.position, movement, 1f, capaBasura);

            if (hit.collider != null)
            {
                animator.SetBool("IsPushing", true);
            }
            else
            {
                animator.SetBool("IsPushing", false);
            }

            StartCoroutine(MoveToGridPosition(movement));
        }
        else
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsPushing", false);
        }
    }

    private IEnumerator MoveToGridPosition(Vector2 direction)
    {
        isMoving = true;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));

            // Usamos WaitForFixedUpdate para que la física sea perfecta
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        isMoving = false;
    }

    // --- NUEVO: LAS FUNCIONES DE DAÑO QUE FALTABAN ---
    public void AplicarEmpuje(Vector2 posicionAmenaza)
    {
        // Detenemos cualquier movimiento hacia una casilla para que el daño tenga prioridad
        StopAllCoroutines();
        isMoving = false;

        // Iniciamos el vuelo hacia atrás
        StartCoroutine(RutinaEmpuje(posicionAmenaza));
    }

    private IEnumerator RutinaEmpuje(Vector2 posicionAmenaza)
    {
        estaSiendoEmpujado = true; // Bloquea el Update

        // Calculamos la dirección contraria a la amenaza (el ácido o la rata)
        Vector2 direccionEmpuje = (rb.position - posicionAmenaza).normalized;

        float tiempoPasado = 0f;
        while (tiempoPasado < tiempoEmpuje)
        {
            rb.MovePosition(rb.position + direccionEmpuje * fuerzaEmpuje * Time.deltaTime);
            tiempoPasado += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // AUTO-ALINEACIÓN: Al terminar de volar por los aires, redondeamos la posición de Jack
        // para asegurarnos de que quede perfectamente encajado en la cuadrícula y no entre dos bloques.
        Vector2 posFinal = rb.position;
        posFinal.x = Mathf.Round(posFinal.x);
        posFinal.y = Mathf.Round(posFinal.y);
        rb.MovePosition(posFinal);

        estaSiendoEmpujado = false; // Le devolvemos el control al jugador
    }
}