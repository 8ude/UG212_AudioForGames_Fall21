using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

namespace MutCommon
{
  public class NetworkedKeyCodeUnityEvent : NetworkBehaviour
  {
    public enum KeyCodeType
    {
      Down,
      Hold,
      Up,
    }

    [Serializable]
    public class KeyCodeTypeEvent
    {
      public KeyCode Key;
      public KeyCodeType Type;
      public UnityEvent Event;
    }

    [SerializeField] public KeyCode Key;
    [SerializeField] public KeyCodeType Type;
    [SerializeField] public UnityEvent Event;

    void Update()
    {
      if (!isLocalPlayer) return;
      switch (Type)
      {
        case KeyCodeType.Down:
          if (Input.GetKeyDown(Key)) Event.Invoke();
          break;
        case KeyCodeType.Hold:
          if (Input.GetKey(Key)) Event.Invoke();
          break;
        case KeyCodeType.Up:
          if (Input.GetKeyUp(Key)) Event.Invoke();
          break;
      }
    }
  }
}