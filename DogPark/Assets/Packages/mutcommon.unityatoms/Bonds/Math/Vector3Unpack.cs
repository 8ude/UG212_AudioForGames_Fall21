using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class Vector3Unpack : MonoBehaviour
  {
    public Vector3Variable In;

    public FloatVariable OutX;

    public FloatVariable OutY;

    public FloatVariable OutZ;

    // Update is called once per frame
    void Update()
    {
      if (In?.Value != null)
      {
        OutX?.SetValue(In.Value.x);
        OutY?.SetValue(In.Value.y);
        OutZ?.SetValue(In.Value.z);
      }
    }
  }
}