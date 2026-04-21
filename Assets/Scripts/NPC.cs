using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Patroli")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    [Header("Deteksi Player")]
    public float detectionRange = 5f;
    public Transform player;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingRight = true;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.right * patrolDistance;

        // Auto cari player lewat tag
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectionRange)
            FaceTarget(player.position);
        else
            Patrol();
    }

    void Patrol()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime
        );

        FaceTarget(targetPos);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            movingRight = !movingRight;
            targetPos = movingRight
                ? startPos + Vector3.right * patrolDistance
                : startPos - Vector3.right * patrolDistance;
        }
    }

    void FaceTarget(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolDistance);
    }
}