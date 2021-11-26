using UnityEngine;


public class Quest : MonoBehaviour
{

    [SerializeField]
    private GameObject[] missions;
    private int currentMissionID;
    public Animator animator;


    public void NextMission()
    {
        if(currentMissionID <= missions.Length - 1)
        {
            missions[currentMissionID].SetActive(false);
            currentMissionID++;
            missions[currentMissionID].SetActive(true);
        }  
    }

    private void Awake()
    {
        currentMissionID = 0;
    }

    public void ActivateMission()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetBool("IsACtive", true);
        }

        
      
    }

}
