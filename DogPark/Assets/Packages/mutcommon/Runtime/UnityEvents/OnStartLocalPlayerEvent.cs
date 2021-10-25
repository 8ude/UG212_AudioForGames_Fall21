using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

namespace MutCommon
{
  public class OnStartLocalPlayerEvent : NetworkBehaviour
  {
    [SerializeField] private UnityEvent callback;
    public override void OnStartLocalPlayer()
    {
      callback?.Invoke();
    }
  }
}