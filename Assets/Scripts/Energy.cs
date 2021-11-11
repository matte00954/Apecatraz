//Author: Jacob Wik
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    [SerializeField] private Slider energyMeter;

    [SerializeField] private bool infiniteEnergy = false;

    private float currentEnergy = 0f;
    private float rechargeTime = 3f; //In seconds, for how long it takes to go from 0 to maxEnergy
    private float maxEnergy = 100f;

    private bool regenerateEnergy = true;

    void Update()
    {

        if (regenerateEnergy == true)
        {
            //Figure out how much energy should recharge this frame.
            float rechargeDelta = (maxEnergy / rechargeTime) * Time.deltaTime;

            //Set current energy to be the current energy amount plus the recharge amount, clamped to maxEnergy, so
            //you can't over charge.

            currentEnergy = Mathf.Clamp(currentEnergy + rechargeDelta, 0f, maxEnergy);

            //Set the energy label text. We use Mathf.Approximatly() here instead of == because
            //of complications arising from float point precision. You should look that up.

            energyMeter.value = currentEnergy;

            /*if ((int)currentEnergy < (int)maxEnergy)
            {
                energyMeter.value = currentEnergy;
            } 
            else
            {

            }*/


            /*if (Mathf.Approximately(currentEnergy, maxEnergy))
                Debug.Log("Energy: FULL");
            else
                Debug.Log("Energy: " + currentEnergy);
                */
        }

    }

    public void SpendEnergy(float energyToBeRemoved)
    {
        if (!infiniteEnergy)
        {
            currentEnergy -= energyToBeRemoved;
        }
    }

    public bool CheckEnergy(float energyToSpend)
    {
        return energyToSpend < currentEnergy;
    }

    public void ActivateEnergyRegen(bool activate)
    {
        regenerateEnergy = activate;
    }

    private void InfiniteEnergy()
    {
        rechargeTime = 0.1f;
        currentEnergy = 10000f;
        maxEnergy = 10000f;
    }
}
