using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    public float speed = 2f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
{
    float h = Input.GetAxisRaw("Horizontal");
    float v = Input.GetAxisRaw("Vertical");
    moveInput = new Vector2(h, v).normalized;

    // Rotasi mengikuti arah gerak
    if (moveInput != Vector2.zero)
    {
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }
}