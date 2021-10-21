using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontChanger : MonoBehaviour
{

    public TMPro.TMP_FontAsset[] fontAsset;

    private TMPro.TMP_Text[] TMProTextList;

    // Start is called before the first frame update
    void Start()
    {
        TMProTextList = FindObjectsOfType<TMPro.TMP_Text>(true);
    }

    // Update is called once per frame
    

    public void ChangeAllFonts()
    {
      
        
            foreach (TMPro.TMP_Text text in TMProTextList)
            {
                text.GetComponent<TMPro.TMP_Text>().font = fontAsset[+1];
            }
        
    }
}
