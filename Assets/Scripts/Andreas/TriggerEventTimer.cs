using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEventTimer : MonoBehaviour
{
    [SerializeField]
    private float timerDuration = 1;

    private float timer;

    private bool enabled;
    
    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        timer = timerDuration;
    }
}
