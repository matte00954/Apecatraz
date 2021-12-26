// Author: Emil Moqvist
using UnityEngine;
using UnityEngine.VFX;

public class ColorSwap : MonoBehaviour
{
    private Color[] colors;

    [SerializeField] private Material mat1;
    [SerializeField] private Material mat2;

    [SerializeField] private VisualEffect telekinesis;

    // Lägg till getcomponent carriedobject set materialcolor = rend color.
    public void AssignColors(int colorNumber)
    {
        mat1.SetColor("GradientNoiseColor", colors[colorNumber]);
        mat2.SetColor("GradientNoiseColor", colors[colorNumber]);
        telekinesis.SetVector4("OutlineColor", colors[colorNumber]);
    }

    private void Start()
    {
        colors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta };
        AssignColors(0);
    }
}
