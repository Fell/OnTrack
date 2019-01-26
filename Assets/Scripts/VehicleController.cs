using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    // Public variables
    public Transform vehicleTrans;
    public float OffsetFactor = 5.0f;

    public float LaneTransitionDuration = 2.0f;
    public float Duration = 300.0f;

    public AudioSource[] LaneAudioSources;

    public VibrationProfile LaneSwitchVibrationProfile = null;

    public AudioSource LaneSwitchUpAudioSource = null;
    public AudioSource LaneSwitchDownAudioSource = null;

    // Private variables
    int laneId = 1;

    Vector3 laneOne = Vector3.zero;
    Vector3 laneZero;
    Vector3 laneTwo;

    float lerpPercent = 0.5f;
    float[] lerpPercentValues = new float[] { 0.0f, 0.5f, 1.0f };

    bool isSwitchingLanes = false;
    
    // Start function
    void Start()
    {
        InputController.Instance.OnLaneDownButtonDown += LaneDown;
        InputController.Instance.OnLaneUpButtonDown += LaneUp;

        laneZero = Vector3.left * OffsetFactor;
        laneTwo = Vector3.right * OffsetFactor;

        LaneAudioSources[0].volume = 0.0f;
        LaneAudioSources[1].volume = 1.0f;
        LaneAudioSources[2].volume = 0.0f;
    }

    // OnDestroy function
    private void OnDestroy()
    {
        if(InputController.Instance != null)
        {
            InputController.Instance.OnLaneDownButtonDown -= LaneDown;
            InputController.Instance.OnLaneUpButtonDown -= LaneUp;
        }
    }

    // Update function
    void Update()
    {
        vehicleTrans.localPosition = Vector3.Lerp(laneZero, laneTwo, lerpPercent);
    }

    // Helper functions
    void LaneUp()
    {
        if (!isSwitchingLanes && laneId <= 1)
            StartCoroutine(SwitchLaneTo(laneId + 1));
    }

    void LaneDown()
    {
        if (!isSwitchingLanes && laneId >= 1)
            StartCoroutine(SwitchLaneTo(laneId - 1));
    }

    IEnumerator SwitchLaneTo(int newLaneId)
    {
        isSwitchingLanes = true;

        if (newLaneId > laneId)
            LaneSwitchUpAudioSource.PlayOneShot(LaneSwitchUpAudioSource.clip);
        else
            LaneSwitchDownAudioSource.PlayOneShot(LaneSwitchDownAudioSource.clip);

        if (LaneSwitchVibrationProfile != null)
            LaneSwitchVibrationProfile.Vibrate();

        float from = lerpPercent;
        float to = lerpPercentValues[newLaneId];

        float percent = 0.0f;
        
        while(percent < 1.0f)
        {
            percent += (Time.deltaTime / LaneTransitionDuration);

            LaneAudioSources[laneId].volume = 1.0f - percent;
            LaneAudioSources[newLaneId].volume = percent;

            lerpPercent = Mathf.Lerp(from, to, percent);
            yield return null;
        }

        LaneAudioSources[laneId].volume = 0.0f;
        LaneAudioSources[newLaneId].volume = 1.0f;

        laneId = newLaneId;

        isSwitchingLanes = false;
    }

}
