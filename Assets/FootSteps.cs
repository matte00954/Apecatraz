using System.Collections;
using System.Collections.Generic;
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
        return steps[UnityEngine.Random.Range(0, steps.Length)];
    }
}
