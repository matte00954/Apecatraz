// Author: Jacob Wik
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    private readonly float rechargeTime = 3f; // In seconds, for how long it takes to go from 0 to maxEnergy
    private readonly float maxEnergy = 100f;

    [SerializeField] private Slider energyMeter;
    [SerializeField] private bool hasInfiniteEnergy;

    private float currentEnergy = 0f;
    private bool isRegenerating = true;

    public bool IsRegenerating { get => isRegenerating; set => isRegenerating = value; }

    public void SpendEnergy(float energyToBeRemoved)
    {
        if (!hasInfiniteEnergy)
            currentEnergy -= energyToBeRemoved;
    }

    public bool CheckEnergy(float energyToSpend)
    {
        return energyToSpend < currentEnergy;
    }

    private void Update()
    {
        if (isRegenerating)
        {
            // Figure out how much energy should recharge this frame.
            float rechargeDelta = (maxEnergy / rechargeTime) * Time.deltaTime;

            // Set current energy to be the current energy amount plus the recharge amount, clamped to maxEnergy to prevent overcharge.
            currentEnergy = Mathf.Clamp(currentEnergy + rechargeDelta, 0f, maxEnergy);
        }

        energyMeter.value = currentEnergy;
    }
}
