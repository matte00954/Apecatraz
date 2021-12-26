// Author: [full name here]
using UnityEngine;

public class WonkyBanana : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;

   /* void Start()
    {
        pos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1, this.gameObject.transform.position.z);

    } */

    private void Update()
    {
        transform.Rotate(90 * Time.deltaTime * 5, 1, 2);

        transform.position = new Vector3(transform.position.x, curve.Evaluate(Time.time % curve.length) + 2, transform.position.z);
    }
}
