using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class Vector3Pack : MonoBehaviour
  {
    public Vector3Variable Out;

    public FloatVariable InX;

    public FloatVariable InY;

    public FloatVariable InZ;

    // Update is called once per frame
    void Update()
    {
      if (InX?.Value != null && InY?.Value != null && InZ?.Value != null)
      {
        Out?.SetValue(new Vector3(InX.Value, InY.Value, InZ.Value));
      }
    }
  }
}