using UnityEngine;

namespace MutCommon
{
  public class LookAtCamera : MonoBehaviour

  {
    private Camera Camera => Camera.main;

    // Update is called once per frame
    void Update()
    {
      transform.LookAt(Camera.transform);
      transform.Rotate(0, 180, 0);
    }
  }
}