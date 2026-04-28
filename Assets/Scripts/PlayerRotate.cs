using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotate : MonoBehaviour
{
    private Camera cam;
    private Vector2 mousePos;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnLook(InputValue value)
    {
        mousePos = value.Get<Vector2>();
        Debug.Log(mousePos);
        Debug.Log("MASUK");
    }

    private void Update()
    {
        Vector3 worldMouse = cam.ScreenToWorldPoint(mousePos);
        worldMouse.z = 0f;

        Vector2 direction = worldMouse - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }
}