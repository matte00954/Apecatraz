using UnityEngine;


public class Quest : MonoBehaviour
{

    [SerializeField]
    private GameObject[] missions;
    private int currentMissionID;
    

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


    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            NextMission();
        }
    }




}
