using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Move : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    protected Rigidbody2D body;
    protected Vector2 currentinput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        body.linearVelocity = currentinput * speed; // ✅ ini yang benar
    }
}