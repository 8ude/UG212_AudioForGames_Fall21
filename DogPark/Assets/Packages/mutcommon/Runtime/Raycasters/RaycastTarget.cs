using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaycastTarget : MonoBehaviour
{
  public UnityEvent OnCollisionEnter;
  public UnityEvent OnCollisionStay;
  public UnityEvent OnCollisionExit;
}
