using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashTrigger : MonoBehaviour
{
    [SerializeField] PowerUpTypes PowerUpType = PowerUpTypes.Bonus100;

    [SerializeField] AudioSource SFXAudioSource = null;
    [SerializeField] VibrationProfile VibrationProfile = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Vehicle"))
        {
            Animator anim = other.transform.parent.GetComponent<Animator>();
            anim.SetTrigger("Crash");

            if(SFXAudioSource != null)
                SFXAudioSource.PlayOneShot(SFXAudioSource.clip);

            if(VibrationProfile != null)
                VibrationProfile.Vibrate();
        }
    }
}
