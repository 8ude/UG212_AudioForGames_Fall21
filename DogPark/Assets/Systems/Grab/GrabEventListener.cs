using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class GrabEventListener : MonoBehaviour, GrabListener
{
    [SerializeField] private UnityEvent onTargetted;
    public void OnTargetted() => onTargetted?.Invoke();

    [SerializeField] private UnityEvent onUntargetted;
    public void OnUntargetted() => onUntargetted?.Invoke();

    [SerializeField] private UnityEvent onGrabbed;
    public void OnGrabbed() => onGrabbed?.Invoke();

    [SerializeField] private UnityEvent onReleased;
    public void OnReleased() => onReleased?.Invoke();
}