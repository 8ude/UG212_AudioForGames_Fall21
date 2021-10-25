using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class RotateObject : MonoBehaviour
  {

    [SerializeField]
    private Vector3Reference Axis;

    [SerializeField]
    private FloatReference RotationsPerSecond;

    // Update is called once per frame
    void Update()
    {
      transform.Rotate(Axis, Time.deltaTime * 360 * RotationsPerSecond);
    }
  }
}