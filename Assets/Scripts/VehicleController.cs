using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    // Public variables
    public static VehicleController Instance { get; private set; }
    public int LaneId { get; set; } = 1;

    // Public variables
    public Transform vehicleTrans;
    public float OffsetFactor = 5.0f;

    public float LaneTransitionDuration = 2.0f;
    public float Duration = 300.0f;

    public AudioSource[] LaneAudioSources;

    public VibrationProfile LaneSwitchVibrationProfile = null;

    public AudioSource LaneSwitchUpAudioSource = null;
    public AudioSource LaneSwitchDownAudioSource = null;
    public AudioSource HornAudioSource = null;

    public Animator TrackAnimator = null;

    public Material LeftIndicatorMat = null;
    public Material RightIndicatorMat = null;

    // Private variables
    int laneId = 1;

    Vector3 laneOne = Vector3.zero;
    Vector3 laneZero;
    Vector3 laneTwo;

    float lerpPercent = 0.5f;
    float[] lerpPercentValues = new float[] { 1.0f, 0.5f, 0.0f };

    bool isSwitchingLanes = false;

    FluffyUnderware.Curvy.Controllers.CurvyController curvyController = null;

    // Aewake function
    private void Awake()
    {
        curvyController = GetComponent<FluffyUnderware.Curvy.Controllers.CurvyController>();

        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    // Start function
    void Start()
    {
        InputController.Instance.OnLaneDownButtonDown += OnLaneDown;
        InputController.Instance.OnLaneUpButtonDown += OnLaneUp;
        InputController.Instance.OnResetButtonDown += OnReset;
        InputController.Instance.OnStartButtonDown += OnStart;

        InputController.Instance.OnHornButtonDown += OnHorn;

        laneZero = Vector3.left * OffsetFactor;
        laneTwo = Vector3.right * OffsetFactor;

        OnReset();
    }

    // OnDestroy function
    private void OnDestroy()
    {
        if(InputController.Instance != null)
        {
            InputController.Instance.OnLaneDownButtonDown -= OnLaneDown;
            InputController.Instance.OnLaneUpButtonDown -= OnLaneUp;
            InputController.Instance.OnResetButtonDown -= OnReset;
            InputController.Instance.OnStartButtonDown -= OnStart;

            InputController.Instance.OnHornButtonDown -= OnHorn;

        }
    }

    // Update function
    void Update()
    {
        vehicleTrans.localPosition = Vector3.Lerp(laneZero, laneTwo, lerpPercent);
    }

    // Helper functions
    void OnStart()
    {
        LaneId = 1;
        lerpPercent = 0.5f;

        LaneAudioSources[0].Play();
        LaneAudioSources[0].volume = 0.0f;
        LaneAudioSources[1].Play();
        LaneAudioSources[1].volume = 1.0f;
        LaneAudioSources[2].Play();
        LaneAudioSources[2].volume = 0.0f;

        TrackAnimator.SetBool("IsRunning", true);

        InputController.Instance.OnLaneDownButtonDown += OnLaneDown;
        InputController.Instance.OnLaneUpButtonDown += OnLaneUp;

        LeftIndicatorMat.DisableKeyword("_EMISSION");
        RightIndicatorMat.DisableKeyword("_EMISSION");
    }

    void OnReset()
    {
        LaneId = 1;
        lerpPercent = 0.5f;

        LaneAudioSources[0].Stop();
        LaneAudioSources[0].volume = 0.0f;
        LaneAudioSources[1].Stop();
        LaneAudioSources[1].volume = 0.0f;
        LaneAudioSources[2].Stop();
        LaneAudioSources[2].volume = 0.0f;

        TrackAnimator.SetBool("IsRunning", false);

        InputController.Instance.OnLaneDownButtonDown -= OnLaneDown;
        InputController.Instance.OnLaneUpButtonDown -= OnLaneUp;

        LeftIndicatorMat.DisableKeyword("_EMISSION");
        RightIndicatorMat.DisableKeyword("_EMISSION");
    }

    void OnLaneUp()
    {
        if (!isSwitchingLanes && LaneId <= 1)
            StartCoroutine(SwitchLaneTo(LaneId + 1));
    }

    void OnLaneDown()
    {
        if (!isSwitchingLanes && LaneId >= 1)
            StartCoroutine(SwitchLaneTo(LaneId - 1));
    }

    void OnHorn()
    {
        if (HornAudioSource != null)
            HornAudioSource.PlayOneShot(HornAudioSource.clip);
    }

    IEnumerator SwitchLaneTo(int newLaneId)
    {
        isSwitchingLanes = true;

        if (newLaneId > LaneId)
        {
            LaneSwitchDownAudioSource.PlayOneShot(LaneSwitchDownAudioSource.clip);

            RightIndicatorMat.EnableKeyword("_EMISSION");
        }
        else
        {
            LaneSwitchUpAudioSource.PlayOneShot(LaneSwitchUpAudioSource.clip);

            LeftIndicatorMat.EnableKeyword("_EMISSION");
        }

        if (LaneSwitchVibrationProfile != null)
            LaneSwitchVibrationProfile.Vibrate();

        float from = lerpPercent;
        float to = lerpPercentValues[newLaneId];

        float percent = 0.0f;
        
        while(percent < 1.0f)
        {
            percent += (Time.deltaTime / LaneTransitionDuration);

            LaneAudioSources[LaneId].volume = 1.0f - percent;
            LaneAudioSources[newLaneId].volume = percent;

            lerpPercent = Mathf.Lerp(from, to, percent);
            yield return null;
        }

        LaneAudioSources[LaneId].volume = 0.0f;
        LaneAudioSources[newLaneId].volume = 1.0f;

        LaneId = newLaneId;

        LeftIndicatorMat.DisableKeyword("_EMISSION");
        RightIndicatorMat.DisableKeyword("_EMISSION");

        isSwitchingLanes = false;
    }

}
