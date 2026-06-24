using UnityEngine;

public class LunahodController : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 60f;

    void Update()
    {
        // Управление
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) turn = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) turn = 1f;

        transform.Translate(Vector3.right * move * speed * Time.deltaTime);
        transform.Rotate(Vector3.forward * turn * turnSpeed * Time.deltaTime);

        // Прижимаем к поверхности
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, 20f))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.5f;
            transform.position = pos;
        }
    }
}