using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms
{
  public class FloatFromRange : MonoBehaviour
  {
    public FloatReference Min;
    public FloatReference Max;
    public FloatVariable In;
    public FloatVariable Out;

    void Update()
    {
      if (In != null)
      {
        Out?.SetValue(Mathf.InverseLerp(Min, Max, In.Value));
      }
    }
  }
}