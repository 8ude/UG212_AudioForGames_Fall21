using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class SetAmbientLight : MonoBehaviour
  {
    public ColorReference color;

    public void Update()
    {
      RenderSettings.ambientLight = color;
    }
  }
}
