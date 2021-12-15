using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static List<GameObject> guards = new List<GameObject>();
    private static List<GameObject> securityCams = new List<GameObject>();

    [SerializeField]
    private ThirdPersonMovement thirdPersonMovement;

    [SerializeField]
    private List<GameObject> checkpoints;

    [SerializeField]
    private GameObject currentCheckpoint;

    public static List<GameObject> Guards { get => guards; }
    public static List<GameObject> SecurityCams { get => securityCams; }

    public void SetCurrentCheckpoint(GameObject checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    public void RespawnAtLatestCheckpoint()
    {
        thirdPersonMovement.MoveTo(currentCheckpoint.transform.position);
    }

    public void RespawnAtCheckpointX(int checkpointnumber)
    {
        thirdPersonMovement.MoveTo(checkpoints[checkpointnumber].transform.position);
    }

    public void ResetAllEnemies()
    {
        foreach (GameObject guard in guards)
            guard.GetComponent<GuardRanged>().ResetGuard();
        foreach (GameObject secCam in securityCams)
            secCam.GetComponent<SecurityCam>().ForceEndAlert();
    }

    private void Update()
    {
        if (thirdPersonMovement.GetGodMode() == true)
        {

            if (Input.GetKeyDown("i"))
            {
                RespawnAtCheckpointX(1);
            }

            if (Input.GetKeyDown("o"))
            {
                RespawnAtCheckpointX(2);
            }
            if (Input.GetKeyDown("p"))
            {
                RespawnAtCheckpointX(0);
            }

        }
    }

}
