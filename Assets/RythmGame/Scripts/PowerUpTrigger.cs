using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpTrigger : MonoBehaviour
{
    [SerializeField] PowerUpTypes PowerUpType = PowerUpTypes.Bonus100;

    [SerializeField] AudioSource SFXEnterAudioSource = null;
    [SerializeField] VibrationProfile EnterVibrationProfile = null;

    [SerializeField] AudioSource SFXExitAudioSource = null;
    [SerializeField] VibrationProfile ExitVibrationProfile = null;


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Vehicle"))
        {
            RythmGameController.Instance.StartModifier(PowerUpType);
            
            if (SFXEnterAudioSource != null)
                SFXEnterAudioSource.PlayOneShot(SFXEnterAudioSource.clip);

            if (EnterVibrationProfile != null)
                EnterVibrationProfile.Vibrate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            RythmGameController.Instance.EndModifier(PowerUpType);

            if (SFXExitAudioSource != null)
                SFXExitAudioSource.PlayOneShot(SFXExitAudioSource.clip);

            if (ExitVibrationProfile != null)
                ExitVibrationProfile.Vibrate();
        }
    }
}
