using UnityEngine;
using System.Collections;
using Rewired;

public class InputController : MonoBehaviour
{
    // Public variables
    public static InputController Instance { get; private set; }

    public System.Action OnLaneDownButtonDown;
    public System.Action OnLaneUpButtonDown;

    public System.Action OnAButtonDown;
    public System.Action OnBButtonDown;
    public System.Action OnXButtonDown;
    public System.Action OnYButtonDown;


    // Private variables
    Player player = null;

    Coroutine vibrationCoroutine = null;

    // Awake function
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            player = ReInput.players.GetPlayer(0);
        }
        else
            Destroy(gameObject);
    }

    // Start function
    void Start()
    {
        // Coonect all controllers to player one
        ReInput.ControllerConnectedEvent += OnControllerConnected;

        foreach (Joystick joystick in ReInput.controllers.Joysticks)
            player.controllers.AddController(joystick, true);

        player.AddInputEventDelegate(OnLaneDownButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Lane Down");
        player.AddInputEventDelegate(OnLaneUpButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Lane Up");

        player.AddInputEventDelegate(OnAButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Button A");
        player.AddInputEventDelegate(OnBButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Button B");
        player.AddInputEventDelegate(OnXButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Button X");
        player.AddInputEventDelegate(OnYButtonPushed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Button Y");
    }

    // Interface functions
    public void Vibrate(AnimationCurve strengthCurve, float duration, int motorIndex = 0)
    {
        if (vibrationCoroutine != null)
            StopCoroutine(vibrationCoroutine);

        vibrationCoroutine = StartCoroutine(VibrateCoroutine(strengthCurve, duration, motorIndex));
    }

    // Helper functions
    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        if (args.controllerType != ControllerType.Joystick)
            return;

        player.controllers.AddController(ReInput.controllers.GetJoystick(args.controllerId), true);
    }

    void OnLaneDownButtonPushed(InputActionEventData data)
    {
        if (OnLaneDownButtonDown != null)
            OnLaneDownButtonDown.Invoke();
    }

    void OnLaneUpButtonPushed(InputActionEventData data)
    {
        if (OnLaneUpButtonDown != null)
            OnLaneUpButtonDown.Invoke();
    }

    void OnAButtonPushed(InputActionEventData data)
    {
        if (OnAButtonDown != null)
            OnAButtonDown.Invoke();
    }

    void OnBButtonPushed(InputActionEventData data)
    {
        if (OnBButtonDown != null)
            OnBButtonDown.Invoke();
    }

    void OnXButtonPushed(InputActionEventData data)
    {
        if (OnXButtonDown != null)
            OnXButtonDown.Invoke();
    }

    void OnYButtonPushed(InputActionEventData data)
    {
        if (OnYButtonDown != null)
            OnYButtonDown.Invoke();
    }

    // This corroutine is required to play a vibration profile
    IEnumerator VibrateCoroutine(AnimationCurve strengthCurve, float duration, int motorIndex)
    {
        float startTime = Time.time;
        float endTime = Time.time + duration;

        player.SetVibration(motorIndex, Mathf.Clamp01(strengthCurve.Evaluate(0.0f)));

        yield return null;

        while (endTime > Time.time)
        {
            player.SetVibration(motorIndex, Mathf.Clamp01(strengthCurve.Evaluate(Time.time - startTime)));
            yield return null;
        }

        player.StopVibration();

        yield break;
    }
}
