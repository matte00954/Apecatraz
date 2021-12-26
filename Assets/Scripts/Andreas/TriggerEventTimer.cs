// Author: Andreas Scherman
using UnityEngine;

public class TriggerEventTimer : MonoBehaviour
{
    [SerializeField] private float timerDuration = 1f;

    private float timer;
    private bool isEnabled;
    
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
            this.gameObject.SetActive(false);
    }

    private void OnEnable() => timer = timerDuration;
}
