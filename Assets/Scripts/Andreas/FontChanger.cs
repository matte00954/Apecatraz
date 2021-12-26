// Author: Andreas Scherman
using UnityEngine;

public class FontChanger : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_FontAsset[] fontAsset;
    private TMPro.TMP_Text[] tmproTextList;

    public void ChangeAllFonts(int fontNumber)
    {
        foreach (TMPro.TMP_Text text in tmproTextList)
            text.GetComponent<TMPro.TMP_Text>().font = fontAsset[fontNumber];
    }

    private void Start() => tmproTextList = FindObjectsOfType<TMPro.TMP_Text>(true);
}
