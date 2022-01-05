// Author: [Jacob Wik]
using UnityEngine;
using UnityEngine.SceneManagement;

public class BigRedButton : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject interactableObjectsText;
    [SerializeField] private float endTimer;
    [SerializeField] private GameObject boom;

    private float explosiontimer = 3.5f;
    private bool canPress = false;
    private bool preparedToExplode = false;

    private void Start()
    {
        if (interactableObjectsText.activeInHierarchy)
            interactableObjectsText.SetActive(false);
    }

    private void Update()
    {
        if ((Input.GetButtonDown("Fire4") && canPress == true && preparedToExplode == false) || (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha0)))
        {
            cutsceneCamera.SetActive(true);
            playerCamera.SetActive(false);
            preparedToExplode = true;
            interactableObjectsText.SetActive(false);
        }

        if (preparedToExplode)
            explosiontimer -= Time.deltaTime;

        if (explosiontimer < 0)
        {
            PlayExplosionAnimation();
            endTimer -= Time.deltaTime;
        }

        if (endTimer < 0)
        {
            cutsceneCamera.SetActive(false);
            SceneManager.LoadScene("EndScene");
        }
    }

    private void PlayExplosionAnimation() => boom.SetActive(true);

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
