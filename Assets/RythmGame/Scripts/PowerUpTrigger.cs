using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpTrigger : MonoBehaviour
{
    [SerializeField] PowerUpTypes PowerUpType = PowerUpTypes.Bonus100;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Vehicle"))
        {
            RythmGameController.Instance.StartModifier(PowerUpType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            RythmGameController.Instance.EndModifier(PowerUpType);
        }
    }
}
