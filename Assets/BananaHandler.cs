using UnityEngine;
using UnityEngine.UI;

public class BananaHandler : MonoBehaviour
{
    private int bananaCounter;
    [SerializeField] private Text bananaCounterUI;
    [SerializeField] private AudioClip bananaPickupSound;

    [SerializeField] private AudioSource audioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Banana"))
        {
            other.gameObject.SetActive(false);
            bananaCounter++;
            SetBananaCounterUI(bananaCounter);
            audioSource.PlayOneShot(bananaPickupSound);
        }
    }

    private void SetBananaCounterUI(int numOfBananas)
    {
        bananaCounterUI.text = ": " + bananaCounter.ToString();
    }
}
