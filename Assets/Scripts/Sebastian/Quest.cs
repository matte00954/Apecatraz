// Author: Sebastian Klötz
using UnityEngine;

public class Quest : MonoBehaviour
{
    [SerializeField] private GameObject[] missions;
    [SerializeField] private Animator animator;

    private int currentMissionID;

    // Public methods
    public void ActivateMission() => animator.SetBool("IsActive", true);
    public void DeactivateMission() => animator.SetBool("IsActive", false);
    public void NextMission()
    {
        if (currentMissionID <= missions.Length - 1)
        {
            missions[currentMissionID].SetActive(false);
            currentMissionID++;
            missions[currentMissionID].SetActive(true);
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

    private void Awake() => currentMissionID = 0;
}