using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour {
	private GameObject mainCamera;
    private GameObject player;
	CharacterController characterController;
	bool carrying;
	GameObject carriedObject;
	public float distance;
	public float smooth;
	void Start () {
		mainCamera = GameObject.FindWithTag("MainCamera");
        player = GameObject.FindWithTag("Player");
		characterController = player.GetComponent<CharacterController>();
	}
	
	void Update () {
		if(carrying) {
			carry(carriedObject);
			checkDrop();
		} else {
			pickup();
		}
	}


	void carry(GameObject o) {
		RaycastHit hit;
		Physics.SphereCast(o.transform.position, o.GetComponent<BoxCollider>().bounds.size.y / 2, transform.forward,out hit);
		Debug.Log(hit);
		o.transform.position = Vector3.Lerp (o.transform.position, player.transform.position + player.transform.forward * distance, Time.deltaTime * smooth);
		o.transform.rotation = Quaternion.identity;
	}

	void pickup() {
		if(Input.GetKeyDown (KeyCode.E)) {
			Vector3 startPositionForRay = player.transform.position + characterController.center;
			RaycastHit hit;
			if(Physics.SphereCast(startPositionForRay, characterController.height / 2, player.transform.forward, out hit, distance ))
			{
				GameObject p = hit.collider.gameObject;
				Debug.Log(p);
				if(p.CompareTag("moveable")) {
					carrying = true;
					carriedObject = p;
					carriedObject.GetComponent<Rigidbody>().useGravity = false;
				}
			}
		}
	}

	void checkDrop() {
		if(Input.GetKeyDown (KeyCode.E)) {
			dropObject();
		}
	}

	void dropObject() {
		carrying = false;
		carriedObject.gameObject.GetComponent<Rigidbody>().useGravity = true;
		carriedObject = null;
	}
}