using UnityEngine;

/// <summary>
/// Patrol Waypoint 2D — musuh bergerak dari satu titik ke titik berikutnya.
/// Dua mode: Loop (terus berputar) dan PingPong (bolak-balik).
/// Buat array Transform waypoints di Inspector, drag titik-titik patrol ke sana.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    // ─── Pengaturan ────────────────────────────────────────────
    [Header("Waypoints")]
    [Tooltip("Drag Transform titik-titik patrol ke sini")]
    public Transform[] waypoints;

    [Header("Batas Area")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Patrol Mode")]
    public PatrolMode mode = PatrolMode.Loop;
    public enum PatrolMode { Loop, PingPong }

    [Header("Perilaku")]
    public float moveSpeed      = 2f;
    [Tooltip("Waktu berhenti di setiap waypoint (detik)")]
    public float waitTime       = 0.5f;
    [Tooltip("Jarak minimum untuk dianggap sampai di waypoint")]
    public float arrivalThresh  = 0.15f;

    // ─── Internal ──────────────────────────────────────────────
    Rigidbody2D rb;
    int   currentIndex   = 0;
    int   direction      = 1;       // +1 maju, -1 mundur (mode PingPong)
    float waitCounter    = 0f;
    bool  waiting        = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale    = 0f;
        rb.freezeRotation  = true;
    }

    void OnEnable()
    {
        // Pastikan musuh tidak diam saat patrol diaktifkan kembali
        waiting     = false;
        waitCounter = 0f;
    }

    void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (waiting)
        {
            rb.linearVelocity = Vector2.zero;
            waitCounter -= Time.fixedDeltaTime;
            if (waitCounter <= 0f)
            {
                waiting = false;
                AdvanceWaypoint();
            }
            return;
        }

        MoveToCurrentWaypoint();
    }

   void MoveToCurrentWaypoint()
{
    Transform target = waypoints[currentIndex];
    Vector2 dir  = ((Vector2)target.position - (Vector2)transform.position).normalized;
    float dist = Vector2.Distance(transform.position, target.position);

    if (dist <= arrivalThresh)
    {
        waiting = true;
        waitCounter = waitTime;
        rb.linearVelocity = Vector2.zero;
    }
    else
    {
        Vector2 nextPos = rb.position + dir * moveSpeed * Time.fixedDeltaTime;

        // Batas area map
        nextPos.x = Mathf.Clamp(nextPos.x, minBounds.x, maxBounds.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBounds.y, maxBounds.y);

        rb.MovePosition(nextPos);

        // Rotasi arah gerak
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, 0, angle),
            Time.fixedDeltaTime * 10f
        );
    }
}

    void AdvanceWaypoint()
    {
        if (mode == PatrolMode.Loop)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else // PingPong
        {
            currentIndex += direction;
            if (currentIndex >= waypoints.Length - 1 || currentIndex <= 0)
                direction *= -1;
            currentIndex = Mathf.Clamp(currentIndex, 0, waypoints.Length - 1);
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.6f);
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Gambar titik waypoint
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);

            // Gambar garis antar waypoint
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
            {
                if (mode == PatrolMode.Loop || i < waypoints.Length - 1)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }

        // Highlight waypoint aktif saat runtime
        if (Application.isPlaying && waypoints[currentIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waypoints[currentIndex].position, 0.25f);
        }
    }
}
