using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : MonoBehaviour {
	private GameObject mainCamera;
    private GameObject player;
	bool carrying;
	GameObject carriedObject;
	public float distance;
	public float smooth;
	void Start () {
		mainCamera = GameObject.FindWithTag("MainCamera");
        player = GameObject.FindWithTag("Player");
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
		o.transform.position = Vector3.Lerp (o.transform.position, player.transform.position + player.transform.forward * distance, Time.deltaTime * smooth);
        //o.transform.position = Vector3.Lerp (o.transform.position, mainCamera.transform.position + mainCamera.transform.forward * distance, Time.deltaTime * smooth);
		o.transform.rotation = Quaternion.identity;
	}

	void pickup() {
		if(Input.GetKeyDown (KeyCode.E)) {
			int x = Screen.width / 2;
			int y = Screen.height / 2;

			Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector3(x,y));
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)) {
				GameObject p = hit.collider.gameObject;
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