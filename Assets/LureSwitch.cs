using UnityEngine.Events;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LureSwitch : MonoBehaviour
{
#pragma warning disable IDE0044 // Add readonly modifier
    [SerializeField] private UnityEvent onActivate;
    [SerializeField] private Material readyMaterial;
    [SerializeField] private Material cooldownMaterial;
    [Range(0f, 30f), Tooltip("The amount of time between activations."),
        SerializeField] private float cooldownTime;
#pragma warning restore IDE0044 // Add readonly modifier

    private MeshRenderer meshRenderer;
    private readonly string playerLayerName = "Player";
    private float cooldownTimer;

#pragma warning disable IDE0051 //Remove unused parameters
    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = readyMaterial;
        cooldownTimer = cooldownTime;
    } 

    private void Update()
    {
        if (cooldownTimer < cooldownTime)
            cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= cooldownTime && meshRenderer.material != readyMaterial)
            meshRenderer.material = readyMaterial;
    }


    private void OnTriggerStay(Collider other)
#pragma warning restore IDE0051 //Remove unused parameters
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(playerLayerName) && cooldownTimer >= cooldownTime)
            {
                cooldownTimer = 0f;
                onActivate.Invoke();
                meshRenderer.material = cooldownMaterial;
            }
        }
    }
}
