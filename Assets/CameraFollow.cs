using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera follow")]
    [SerializeField] private Transform target; //Player
    [SerializeField] Vector3 offset; //difference between camera and player
    [SerializeField] [Range(8, 15)] private float smoothSpeed; //low number will cause issues, camera will lag behind


    [Header("Rotation")]
    [SerializeField] private float speedYaw = 2.0f;
    [SerializeField] private float speedPitch = 2.0f;

    private float yaw = 0f;
    private float pitch = 0f;

    private void LateUpdate()
    {
        FollowPlayer();
        RotateCamera();
    }

    private void FollowPlayer()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        transform.LookAt(target);
    }

    private void RotateCamera()
    {
        yaw += speedYaw * Input.GetAxis("Mouse X");

        pitch -= speedPitch * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
