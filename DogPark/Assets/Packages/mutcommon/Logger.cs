using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class Logger : MonoBehaviour
  {
    public void Log(string value) => Debug.Log(value);
    public void LogWarning(string value) => Debug.LogWarning(value);
    public void LogError(string value) => Debug.LogError(value);
  }
}
