// Author: Sebastian Klötz
using UnityEngine;
using TMPro;

public class Quest : MonoBehaviour
{
    //[SerializeField] private GameObject[] missions;
    [SerializeField] private Animator animator;
    //[SerializeField] private GameObject[] quest;
    [SerializeField] private string[] quests;
    [SerializeField] private TextMeshProUGUI questText;

    private int currentMissionID;

    // Public methods
    public void ActivateMission() => animator.SetBool("IsActive", true);
    public void DeactivateMission() => animator.SetBool("IsActive", false);
    public void NextMission()
    {
       /* if (currentMissionID <= missions.Length - 1)
        {
            missions[currentMissionID].SetActive(false);
            currentMissionID++;
            missions[currentMissionID].SetActive(true);
        } */

        if (currentMissionID < quests.Length -1)
        {
            questText.text = quests[currentMissionID];
            currentMissionID++;
        }
    }

    // Private methods
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.X))
                ActivateMission();

            if (Input.GetKeyDown(KeyCode.C))
                DeactivateMission();
        }
    }

    private void Awake()
    {
        currentMissionID = 0;
        questText.text = string.Empty;
    }
}