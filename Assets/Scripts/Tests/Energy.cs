using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
 

private float maxEnergy = 100f; 

private GameObject energyMeter;

 
private float currentEnergy = 0f;
private float rechargeTime = 10f; //In seconds, for how long it takes to go from 0 to maxEnergy
private bool regenerateEnergy = true;
 
void Start() {
 
    //energyMeter = GetComponent<energyMeter>();
 
}  
 
void Update() 
    {
        if(regenerateEnergy == true)
        {
            //Figure out how much energy should recharge this frame.
            float rechargeDelta = (maxEnergy / rechargeTime) * Time.deltaTime;
        
            //Set current energy to be the current energy amount plus the recharge amount, clamped to maxEnergy, so
            //you can't over charge.
            currentEnergy = Mathf.Clamp(currentEnergy + rechargeDelta, 0f, maxEnergy);
        
            //Set the energy label text. We use Mathf.Approximatly() here instead of == because
            //of complications arising from float point precision. You should look that up.
            if (Mathf.Approximately(currentEnergy, maxEnergy))
                Debug.Log("Energy: FULL");
            else
                Debug.Log("Energy: "+currentEnergy);
        }
    }

public void SpendEnergy(float energyToBeRemoved)
    {
        currentEnergy = currentEnergy - energyToBeRemoved;
    }
public bool CheckEnergy(float energyToSpend)
    {
    return energyToSpend < currentEnergy;
    }

public void ActivateEnergyRegen(bool trueOrFalse)
    {
        regenerateEnergy = trueOrFalse;
    }

}
