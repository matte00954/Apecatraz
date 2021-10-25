using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField]
    private float energy = 100f;

    [SerializeField]
    private float energyRegenModifier;

    private GameObject player;


    
    private void Start() {
        
    }

    private void FixedUpdate() {
        if(energy < 100)
        {
            energy += energyRegenModifier;

        }
        Debug.Log(energy);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            energy = 1.0f ;}
    }
}
