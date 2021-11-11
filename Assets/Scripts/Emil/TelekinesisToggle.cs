using UnityEngine;

public class TelekinesisToggle : MonoBehaviour
{
    public Material[] materials;
    public bool isCarried;
    public int x;

    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;
        isCarried = false;
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[x];
    }

    // Update is called once per frame
    void Update()
    {
        rend.sharedMaterial = materials[x];

        if (isCarried)
        {
            x = 1;
        }
        if (!isCarried)
        {
            x = 0;
        }
    }

}
