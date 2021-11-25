using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class QuestHandler : MonoBehaviour
{
    public Quest quest;

    public GameObject QuestWindow;

    public void questLog()
    {
        QuestWindow.SetActive(true);

        
    }
}
