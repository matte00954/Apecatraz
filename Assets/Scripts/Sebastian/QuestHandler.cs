// Author: Sebastian Klötz
using UnityEngine;

public class QuestHandler : MonoBehaviour
{
    [SerializeField] private Quest quest;
    [SerializeField] private GameObject QuestWindow;

    public void ActivateQuestWindow() => QuestWindow.SetActive(true);
}
