using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 25f;
    public float sensY = 25f;

    public Transform orientation;

    float rotationX;
    float rotationY;

    void Start()
    {

    }

void Update()
{
    float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
    float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

    rotationX += mouseX;
    rotationY -= mouseY;
    rotationY = Mathf.Clamp(rotationY, -90f, 90f);

    transform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
    orientation.rotation = Quaternion.Euler(0, rotationX, 0);
}

}
