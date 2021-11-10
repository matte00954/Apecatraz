using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DashEffects : MonoBehaviour
{
    public float fadeSpeed;

    public GameObject monkey;
    public GameObject ball;
    public ParticleSystem particle1;

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
        particle1.enableEmission = false;
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
            particle1.enableEmission= true;
            monkey.GetComponent<SkinnedMeshRenderer>().enabled = false;
            ball.GetComponent<MeshRenderer>().enabled = true;
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
            particle1.enableEmission = false;
            monkey.GetComponent<SkinnedMeshRenderer>().enabled = true;
            ball.GetComponent<MeshRenderer>().enabled = false;
            changeTimer = 0.5f;
            ready = false;
            dashing = false;
            aSource.clip = speedUpClip;
            aSource.Play();
            targetWeight = 0;
        }
    }
}
