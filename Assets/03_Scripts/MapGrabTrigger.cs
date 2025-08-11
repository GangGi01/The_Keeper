using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class MapGrabTrigger : MonoBehaviour
{
    [SerializeField] private EnvironmentTransition target;

    [Header("When to trigger")]
    [SerializeField] private bool onSelectEntered = true;   // 잡을 때
    [SerializeField] private bool onSelectExited = false;  // 놓을 때

    [Header("How to trigger")]
    [SerializeField] private bool toggle = true;            // 토글 모드
    [SerializeField] private bool forwardWhenCalled = true; // toggle=false일 때 사용

    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (onSelectEntered) grab.selectEntered.AddListener(OnGrab);
        if (onSelectExited) grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        if (onSelectEntered) grab.selectEntered.RemoveListener(OnGrab);
        if (onSelectExited) grab.selectExited.RemoveListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        if (target == null) return;
        if (toggle) target.TriggerTransition();
        else target.TriggerTransition(forwardWhenCalled);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (!onSelectExited || target == null) return;
        if (toggle) target.TriggerTransition();
        else target.TriggerTransition(!forwardWhenCalled);
    }
}
