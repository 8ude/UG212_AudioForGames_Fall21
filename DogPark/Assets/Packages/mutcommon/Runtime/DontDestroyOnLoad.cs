using UnityEngine;

namespace MutCommon
{
  public class DontDestroyOnLoad : MonoBehaviour
  {
    private void Awake()
    {
      DontDestroyOnLoad(this);
    }
  }
}