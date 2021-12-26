// Author: Emil Moqvist
using UnityEngine;

public class BananaPickup : MonoBehaviour
{
   ////public AnimationCurve curve;

    private void Update()
    {
        transform.Rotate(0, 90 * Time.deltaTime, 0);

       ////transform.position = new Vector3(transform.position.x, curve.Evaluate((Time.time % curve.length)) + 2, transform.position.z);
    }
}
