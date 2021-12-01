using UnityEngine;

public class TriggerSetActive : MonoBehaviour
{
    [SerializeField] private GameObject[] toActivate;

    [SerializeField] private float delayBetween;

    private float timer;

    private bool start;

    private int i = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
            start = true;
    }

    private void Start()
    {
        timer = delayBetween;
    }
    private void Update()
    {
        if (i <= toActivate.Length && start)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                toActivate[i].SetActive(true);
                i++;
                ResetTimer();
                if(i >= toActivate.Length)
                {
                    start = false;
                }
            }
        }
    }

    private void ResetTimer()
    {
        timer = delayBetween;
    }
}
