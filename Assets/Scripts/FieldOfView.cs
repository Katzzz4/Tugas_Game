using UnityEngine;

/// <summary>
/// Field of View 2D — mendeteksi player dalam sudut dan jarak tertentu.
/// Gunakan Physics2D.Raycast untuk cek penghalang (dinding dll).
/// Pasang script ini pada GameObject yang sama dengan EnemyAI.
/// </summary>
public class FieldOfView : MonoBehaviour
{
    // ─── Pengaturan FOV ────────────────────────────────────────
    [Header("Field of View")]
    [Range(10f, 360f)]
    public float viewAngle   = 90f;         // sudut kerucut penglihatan (derajat)

    [Range(1f, 30f)]
    public float viewRadius  = 8f;          // jarak pandang maksimum

    // ─── Layer ─────────────────────────────────────────────────
    [Header("Layer")]
    public LayerMask obstacleMask;          // layer dinding / penghalang
    public LayerMask playerMask;            // layer player

    // ─── Status ────────────────────────────────────────────────
    public bool playerVisible { get; private set; }
    Transform playerTransform;

    void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        playerVisible = CheckPlayerVisibility();
    }

    /// <summary>
    /// Kembalikan true jika player ada di dalam sudut pandang DAN tidak ada penghalang.
    /// </summary>
    public bool CanSeePlayer()
    {
        return playerVisible;
    }

    bool CheckPlayerVisibility()
    {
        if (playerTransform == null) return false;

        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        float   distance = toPlayer.magnitude;

        // 1. Cek jarak
        if (distance > viewRadius) return false;

        // 2. Cek sudut — bandingkan dengan arah hadap musuh
        float angleToPlayer = Vector2.SignedAngle(GetForwardDirection(), toPlayer);
        if (Mathf.Abs(angleToPlayer) > viewAngle / 2f) return false;

        // 3. Raycast — pastikan tidak ada penghalang di antara musuh dan player
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            toPlayer.normalized,
            distance,
            obstacleMask
        );

        // Jika ray mengenai penghalang sebelum sampai ke player → tidak terlihat
        return hit.collider == null;
    }

    /// <summary>
    /// Arah hadap musuh dalam koordinat 2D berdasarkan rotasi transform.
    /// </summary>
    Vector2 GetForwardDirection()
    {
        float angleRad = transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    /// <summary>
    /// Konversi sudut lokal ke arah world space (berguna untuk debug Gizmos).
    /// </summary>
    public Vector3 DirectionFromAngle(float angleDeg, bool global)
    {
        if (!global) angleDeg += transform.eulerAngles.z;
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
    }

    // ─── Visualisasi di Scene View ─────────────────────────────
    void OnDrawGizmos()
    {
        // Lingkaran radius pandang
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Garis batas sudut FOV
        Vector3 leftBound  = DirectionFromAngle(-viewAngle / 2f, false);
        Vector3 rightBound = DirectionFromAngle( viewAngle / 2f, false);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, leftBound  * viewRadius);
        Gizmos.DrawRay(transform.position, rightBound * viewRadius);

        // Warna merah jika player terdeteksi
        if (playerVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
