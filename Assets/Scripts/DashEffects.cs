using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DashEffects : MonoBehaviour
{
    public float fadeSpeed;

    public AudioClip slowDownClip;
    public AudioClip speedUpClip;

    private AudioSource aSource;
    private float effectWeight;
    private float targetWeight;

    private bool dashing;
    private bool ready;
    private GameObject volume;
    private float changeTimer;
    // Start is called before the first frame update
    void Start()
    {
        volume = GameObject.Find("DashVolume");
        aSource = GetComponent<AudioSource>();
        targetWeight = 0;
        dashing = false;
        ready = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (changeTimer> 0)
        {
            effectWeight += (targetWeight - effectWeight) * Time.deltaTime * fadeSpeed;
            volume.GetComponent<Volume>().weight = effectWeight / 100;
            changeTimer -= Time.deltaTime;
            if (changeTimer< 0)
            {
                changeTimer = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            ready = true;
        }
    }

    public void SlowDown()
    {
        if (!dashing && ready)
        {
            changeTimer = 0.5f;
            dashing = true;
            aSource.clip = slowDownClip;
            aSource.Play();
            targetWeight = 100;
        }
    }

    public void SpeedUp()
    {
        if(dashing)
        {
            changeTimer = 0.5f;
            ready = false;
            dashing = false;
            aSource.clip = speedUpClip;
            aSource.Play();
            targetWeight = 0;
        }
    }
}
