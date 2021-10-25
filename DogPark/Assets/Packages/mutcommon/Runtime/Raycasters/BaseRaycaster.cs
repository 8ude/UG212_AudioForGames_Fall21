using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public abstract class Raycaster : MonoBehaviour
  {
    [SerializeField] protected Camera camera;

    [SerializeField] private LayerMask layer = 0;

    [SerializeField] private float maxDistance = 1000;

    private RaycastTarget currentTarget;

    private void OnValidate()
    {
      camera = GetComponent<Camera>();
      if (camera == null) { camera = Camera.main; }
    }

    // Update is called once per frame
    void Update()
    {
      var ray = Ray();
      bool acquiredTarget = false;
      if (Physics.Raycast(ray, out var hitInfo, maxDistance, layer))
      {
        var target = hitInfo.transform.GetComponent<RaycastTarget>();
        if (target == null) return;
        acquiredTarget = true;
        if (target != currentTarget)
        {
          currentTarget?.OnCollisionExit.Invoke();
          currentTarget = target;
          currentTarget.OnCollisionEnter.Invoke();
        }
        else
        {
          currentTarget = target;
          currentTarget.OnCollisionStay.Invoke();
        }
      }

      if (!acquiredTarget && currentTarget != null)
      {
        currentTarget.OnCollisionExit.Invoke();
        currentTarget = null;
      }
    }

    protected abstract Ray Ray();

    private void OnDrawGizmos()
    {
      var r = Ray();
      Gizmos.color = Color.magenta;
      Gizmos.DrawLine(r.origin, r.origin + r.direction * maxDistance);
    }
  }
}
