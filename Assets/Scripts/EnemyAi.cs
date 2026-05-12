using System.Collections;
using UnityEngine;

/// <summary>
/// AI Musuh 2D Top-down dengan State Machine
/// State: Patrol → Detect → Chase → Lost → Patrol
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAi : MonoBehaviour
{
    // ─── Referensi ─────────────────────────────────────────────
    [Header("Referensi")]
    public Transform player;
    public FieldOfView fov;                  // script FOV (lihat FieldOfView.cs)
    public EnemyPatrol patrol;               // script Patrol (lihat EnemyPatrol.cs)

    // ─── Kecepatan ─────────────────────────────────────────────
    [Header("Kecepatan")]
    public float patrolSpeed   = 2f;
    public float chaseSpeed    = 4.5f;

    // ─── Jarak Deteksi ─────────────────────────────────────────
    [Header("Jarak")]
    [Tooltip("Jika player lebih jauh dari ini, musuh berhenti mengejar")]
    public float loseDistance  = 10f;

    // ─── Timer Kehilangan Target ────────────────────────────────
    [Header("Timer")]
    public float lostDuration  = 3f;        // detik musuh mencari sebelum kembali patrol
    float lostTimer;

    // ─── State ─────────────────────────────────────────────────
    public enum State { Patrol, Detect, Chase, Lost }
    public State currentState { get; private set; } = State.Patrol;

    // ─── Komponen ──────────────────────────────────────────────
    Rigidbody2D rb;
    Vector2 lastKnownPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Cari referensi otomatis jika belum di-assign
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (fov    == null) fov    = GetComponent<FieldOfView>();
        if (patrol == null) patrol = GetComponent<EnemyPatrol>();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Detect: UpdateDetect(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Lost:   UpdateLost();   break;
        }
    }

    // ─── State: Patrol ─────────────────────────────────────────
    void UpdatePatrol()
    {
        patrol.enabled = true;
        patrol.moveSpeed = patrolSpeed;

        if (fov.CanSeePlayer())
        {
            ChangeState(State.Detect);
        }
    }

    // ─── State: Detect ─────────────────────────────────────────
    // Musuh berhenti sebentar, "menatap" player sebelum mengejar
    void UpdateDetect()
    {
        patrol.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // Putar menghadap player
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0, 0, angle), Time.deltaTime * 8f);

        if (!fov.CanSeePlayer())
        {
            ChangeState(State.Patrol);
            return;
        }

        // Transisi ke Chase setelah selesai menatap (0.4 detik)
        StartCoroutine(TransitionToChase());
    }

    IEnumerator TransitionToChase()
    {
        yield return new WaitForSeconds(0.4f);
        if (currentState == State.Detect)
            ChangeState(State.Chase);
    }

    // ─── State: Chase ──────────────────────────────────────────
    void UpdateChase()
    {
        patrol.enabled = false;
        lastKnownPosition = player.position;

        MoveToward(player.position, chaseSpeed);

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > loseDistance)
        {
            ChangeState(State.Lost);
        }
    }

    // ─── State: Lost ───────────────────────────────────────────
    void UpdateLost()
    {
        patrol.enabled = false;

        // Bergerak ke posisi terakhir player terlihat
        float distToLast = Vector2.Distance(transform.position, lastKnownPosition);
        if (distToLast > 0.5f)
        {
            MoveToward(lastKnownPosition, patrolSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            // Putar-putar mencari player
            transform.Rotate(0, 0, 90f * Time.deltaTime);

            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0f)
            {
                ChangeState(State.Patrol);
            }
        }

        // Kalau lihat player lagi, langsung kejar
        if (fov.CanSeePlayer())
        {
            ChangeState(State.Chase);
        }
    }

    // ─── Helper: Pindah State ──────────────────────────────────
    void ChangeState(State newState)
    {
        if (currentState == newState) return;

        // Reset timer saat masuk Lost
        if (newState == State.Lost)
            lostTimer = lostDuration;

        currentState = newState;
        Debug.Log($"[EnemyAI] State: {newState}");
    }

    // ─── Helper: Gerak ke Titik ────────────────────────────────
    void MoveToward(Vector2 target, float speed)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;

        // Rotasi menghadap arah gerak
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0, 0, angle), Time.deltaTime * 10f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseDistance);
    }
}
