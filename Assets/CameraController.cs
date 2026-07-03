using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 3f;

    private float rotX = 0f;
    private float rotY = 90f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        rotY += Input.GetAxis("Mouse X") * sensitivity;
        rotX -= Input.GetAxis("Mouse Y") * sensitivity;
        rotX = Mathf.Clamp(rotX, -60f, 60f);

        transform.localRotation = Quaternion.Euler(rotX, rotY, 0f);
    }
}