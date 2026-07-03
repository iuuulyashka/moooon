using UnityEngine;

public class LunahodController : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 60f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turn = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turn = 1f;

        // Движение вперед/назад строго по КРАСНОЙ стрелке (transform.right)
        Vector3 moveDir = transform.right * move * speed;
        rb.MovePosition(rb.position + moveDir * Time.fixedDeltaTime);

        // Вращение строго вокруг СВОЕЙ локальной оси Y на одной точке
        Quaternion deltaRotation = Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
