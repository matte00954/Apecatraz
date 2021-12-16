using UnityEngine;

public class TriggerSetActive : MonoBehaviour
{
    [SerializeField] private GameObject[] toActivate;

    [SerializeField] private float delayBetween;

    private float timer;

    private bool start;

    private int i = 0;

    private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && start == false)
            start = true;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        timer = delayBetween;
    }

    private void Update()
    {
        if (i <= toActivate.Length - 1 && start)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                audioSource.Play();
                toActivate[i].SetActive(true);
                i++;
                ResetTimer();
            }
        }
    }

    private void ResetTimer()
    {
        timer = delayBetween;
    }
}
