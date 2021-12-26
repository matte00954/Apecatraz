// Author: [full name here]
using UnityEngine;
using UnityEngine.Rendering;

public class DashEffects : MonoBehaviour
{
    [Header("Movement reference")]
    [SerializeField] private ThirdPersonMovement thirdPersonMovement;

    [Header("Player game objects")]
    [SerializeField] private GameObject monkey;
    [SerializeField] private GameObject ball;

    private SkinnedMeshRenderer monkeySkinnedMeshRenderer;
    private MeshRenderer ballMeshRenderer;

    [Header("Particle system")]
    [SerializeField] private ParticleSystem particle1;

    [Header("Audio clips")]
    [SerializeField] private AudioClip slowDownClip;
    [SerializeField] private AudioClip speedUpClip;

    [Header("Effect")]
    [SerializeField] private float fadeSpeed;

    [SerializeField] private GameObject volume;

    private AudioSource audioSource;

    private float effectWeight;
    private float targetWeight;

    private float changeTimer;

    private bool slowDownIsReady;
    private bool isDashing;
    private bool effectsIsActive;

    [SerializeField] private GameObject helpUI;
    [SerializeField] private bool jochePrototypeActive;

    public void SlowDown()
    {
        if (!isDashing && slowDownIsReady)
        {
            if (!effectsIsActive)
            {
                particle1.Play();

                monkey.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false; // TODO: ingen getComponent
                ballMeshRenderer.enabled = true;
                targetWeight = 100;
            }

            ////monkeySkinnedMeshRenderer.enabled = false;
            ballMeshRenderer.enabled = true;
            isDashing = true;

            changeTimer = 0.5f;

            audioSource.clip = slowDownClip;
            audioSource.Play();
        }
    }

    public void SpeedUp()
    {
        if (isDashing)
        {
            Time.timeScale = 1;
            particle1.Stop();

            monkey.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true; // TODO: ingen getComponent
            ballMeshRenderer.enabled = false;
            slowDownIsReady = false;
            isDashing = false;

            changeTimer = 0.5f;

            audioSource.clip = speedUpClip;
            audioSource.Play();
            targetWeight = 0;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        targetWeight = 0;
        slowDownIsReady = true;

        isDashing = false;

        monkeySkinnedMeshRenderer = monkey.gameObject.GetComponent<SkinnedMeshRenderer>();
        ballMeshRenderer = ball.gameObject.GetComponent<MeshRenderer>();

        if (thirdPersonMovement == null)
        {
            Debug.LogError("Third person movement reference in dash effects on main camera is not assigned!!!");
            thirdPersonMovement = FindObjectOfType<ThirdPersonMovement>(); // Prevents any scene from being bugged in case someone forgets
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown && jochePrototypeActive)
        {
            if (Input.GetKeyDown(KeyCode.U))
                effectsIsActive = !effectsIsActive;
            if (Input.GetKeyDown(KeyCode.I))
                helpUI.active = !helpUI.active; // TODO: Solve obsolete issue
        }

        if (changeTimer > 0)
        {
            effectWeight += (targetWeight - effectWeight) * Time.deltaTime * fadeSpeed;
            volume.GetComponent<Volume>().weight = effectWeight / 100;  
            changeTimer -= Time.deltaTime;

            if (changeTimer < 0)
                changeTimer = 0;
        }

        if (thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.dashing))
            slowDownIsReady = true;
    }
}

// OLD CODE
/*using System.Collections;
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

        if (changeTimer > 0)
        {
            effectWeight += (targetWeight - effectWeight) * Time.deltaTime * fadeSpeed;
            volume.GetComponent<Volume>().weight = effectWeight / 100;
            changeTimer -= Time.deltaTime;
            if (changeTimer < 0)
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
            particle1.enableEmission = true;
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
        if (dashing)
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
}*/