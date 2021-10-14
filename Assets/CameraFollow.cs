using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera follow")]
    [SerializeField] private Transform player; //Player
    [SerializeField] Vector3 offset; //difference between camera and player
    [SerializeField] [Range(8, 15)] private float smoothSpeed; //low number will cause issues, camera will lag behind

    [SerializeField] private float turnSpeed = 4.0f;


    [Header("Rotation")]
    [SerializeField] private float speedYaw = 2.0f;
    [SerializeField] private float speedPitch = 2.0f;

  
      void LateUpdate()
      {
          offset = Quaternion.AngleAxis (Input.GetAxis("Mouse X") * turnSpeed, Vector3.up) * offset;
          transform.position = player.position + offset; 
          transform.LookAt(player.position);
          
      }

}
