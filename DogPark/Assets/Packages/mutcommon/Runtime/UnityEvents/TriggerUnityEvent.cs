using UnityEngine;
using UnityEngine.Events;

namespace MutCommon
{
  public class TriggerUnityEvent : MonoBehaviour
  {
    [SerializeField] public UnityEvent OnEvent;

    [SerializeField] public bool OnEnter;
    [SerializeField] public bool OnStay;
    [SerializeField] public bool OnExit;

    public bool filterByTag;
    public string tag;

    public bool filterByLayer;
    public LayerMask layerMask;

    private void OnTriggerEnter(Collider other)
    {
      DoTrigger(other, OnEnter);
    }

    private void OnTriggerExit(Collider other)
    {
      DoTrigger(other, OnExit);
    }

    private void OnTriggerStay(Collider other)
    {
      DoTrigger(other, OnStay);
    }

    private void DoTrigger(Collider other, bool ofType)
    {
      if (filterByTag && other.tag != tag) return;
      if (filterByLayer && !(layerMask == (layerMask | (1 << other.gameObject.layer)))) return;
      if (ofType) OnEvent.Invoke();
    }
  }
}