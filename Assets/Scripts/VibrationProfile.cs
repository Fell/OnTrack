using UnityEngine;

// This profile is used to activate and state changes of the gamepad vibration motors
[System.Serializable]
[CreateAssetMenu(fileName = "VibrationProfile", menuName = "FX/VibrationProfile", order = 0)]
public class VibrationProfile : ScriptableObject
{
    // Public variables
    [SerializeField] AnimationCurve VibrationCurve = new AnimationCurve();
    [SerializeField] float VibrationDuration = 1.0f;

    // Interface function
    public void Vibrate()
    {
        InputController.Instance.Vibrate(VibrationCurve, VibrationDuration);
    }
}