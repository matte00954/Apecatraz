using UnityEngine;
using UnityEngine.SceneManagement;

public class BigRedButton : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneCamera;

    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject interactableObjectsText;

    private float explosiontimer = 3.5f;

    [SerializeField] private float endTimer;

    private bool canPress = false;

    [SerializeField] private GameObject boom;
    private bool prepareToExplode = false;

    private void Start()
    {
        if (interactableObjectsText.activeInHierarchy) interactableObjectsText.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canPress == true && prepareToExplode == false || Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha0))
        {
            cutsceneCamera.SetActive(true);
            playerCamera.SetActive(false);
            prepareToExplode = true;
            interactableObjectsText.SetActive(false);
        }
        if (prepareToExplode == true)
        {
            explosiontimer -= Time.deltaTime;
        }
        if (explosiontimer < 0)
        {
            playExplosionAnimation();
            endTimer -= Time.deltaTime;
        }

        if (endTimer < 0)
        {
            cutsceneCamera.SetActive(false);
            SceneManager.LoadScene("EndScene");
        }
    }

    private void playExplosionAnimation()
    {
        boom.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BigRedButton"))
        {
            interactableObjectsText.SetActive(true);
            canPress = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BigRedButton"))
        {
            interactableObjectsText.SetActive(false);
            canPress = false;
        }

    }
}
