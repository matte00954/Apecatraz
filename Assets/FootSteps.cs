// Author: [full name here]
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    [SerializeField] private AudioClip[] steps;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private int pitchVal;

    private void Step()
    {
        AudioClip clip = GetRandomClip();
        audioSource.pitch = pitchVal;
        audioSource.PlayOneShot(clip);
        Debug.Log("StepEventTriggered");
    }

    private AudioClip GetRandomClip()
    {
        return steps[Random.Range(0, steps.Length)];
    }
}
