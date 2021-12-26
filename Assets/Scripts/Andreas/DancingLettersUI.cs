// Author: Andreas Scherman
using TMPro;
using UnityEngine;

public class DancingLettersUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;

    [SerializeField] private float waveSpeed = -2f;
    [SerializeField] private float waveSize = 10f;

    private void Update()
    {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; ++i)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            Vector3[] verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; ++j)
            {
                Vector3 orig = verts[charInfo.vertexIndex + j];
                verts[charInfo.vertexIndex + j] = orig + new Vector3(0, Mathf.Sin((Time.realtimeSinceStartup * waveSpeed) + (orig.x * 0.01f)) * waveSize, 0);
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; ++i)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
