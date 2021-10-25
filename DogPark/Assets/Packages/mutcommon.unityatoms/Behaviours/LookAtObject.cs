using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms
{
  public class LookAtObject : MonoBehaviour
  {
    [SerializeField] private GameObjectReference target;

    // Update is called once per frame
    void Update()
    {
      if (target?.Value == null) return;
      transform.LookAt(target.Value.transform);
    }
  }
}
