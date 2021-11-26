using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private ThirdPersonMovement thirdPersonMovement;

    [SerializeField]
    private List<GameObject> checkpoints;

    [SerializeField]
    private GameObject currentCheckpoint;

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

    private void Update()
    {
        if (Input.GetKeyDown("i"))
        {
            RespawnAtCheckpointX(1);
        }

        if (Input.GetKeyDown("o"))
        {
            RespawnAtCheckpointX(2);
        }
        if(Input.GetKeyDown("p"))
        {
            RespawnAtCheckpointX(3);
        }
    }

}
