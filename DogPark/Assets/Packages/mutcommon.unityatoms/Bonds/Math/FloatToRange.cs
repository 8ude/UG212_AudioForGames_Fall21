using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class FloatToRange : MonoBehaviour
  {
    public FloatReference Min;
    public FloatReference Max;
    public FloatVariable In;
    public FloatVariable Out;

    void Update()
    {
      if (In != null)
      {
        Out?.SetValue(Mathf.Lerp(Min, Max, In.Value));
      }
    }
  }
}