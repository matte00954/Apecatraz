using UnityEngine;
using UnityEngine.Rendering;

public class DashEffects : MonoBehaviour
{
    [Header("Movement reference")]
    [SerializeField] private ThirdPersonMovement thirdPersonMovement;

    [Header("Player game objects")]
    [SerializeField] private GameObject monkey;
    [SerializeField] private GameObject ball;

    [SerializeField] private SkinnedMeshRenderer monkeySkinnedMeshRenderer;
    [SerializeField] private MeshRenderer ballMeshRenderer;

    [Header("Particle system")]
    [SerializeField] private ParticleSystem particle1;

    [Header("Audio clips")]
    [SerializeField] private AudioClip slowDownClip;
    [SerializeField] private AudioClip speedUpClip;

    [Header("Effect")]
    [SerializeField] private float fadeSpeed;

    private AudioSource aSource;
    private float effectWeight;
    private float targetWeight;

    private bool slowDownReady;
    private GameObject volume;
    private float changeTimer;
    // Start is called before the first frame update

    void Start()
    {
        volume = GameObject.Find("DashVolume"); //Find is always bad, but only done once
        aSource = GetComponent<AudioSource>();
        aSource.volume = 0.1f; //TEMP FIX, audio too loud
        targetWeight = 0;
        slowDownReady = true;
        particle1.enableEmission = false;

        monkeySkinnedMeshRenderer = monkey.GetComponent<SkinnedMeshRenderer>();
        ballMeshRenderer = ball.GetComponent<MeshRenderer>();
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

        if (thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.dashing))
        {
            slowDownReady = true;
        }
    }

    public void SlowDown()
    {
        if (!thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.dashing) && slowDownReady)
        {
            particle1.enableEmission = true;
            monkeySkinnedMeshRenderer.enabled = false;
            ballMeshRenderer.enabled = true;

            changeTimer = 0.5f;

            aSource.clip = slowDownClip;
            aSource.Play();
            targetWeight = 100;
        }
    }

    public void SpeedUp()
    {
        if(thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.dashing))
        {
            particle1.enableEmission = false;
            monkeySkinnedMeshRenderer.enabled = true;
            ballMeshRenderer.enabled = false;
            slowDownReady = false;

            changeTimer = 0.5f;

            aSource.clip = speedUpClip;
            aSource.Play();
            targetWeight = 0;
        }
    }
}
