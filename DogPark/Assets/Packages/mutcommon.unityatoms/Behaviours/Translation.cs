using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;

namespace MutCommon.UnityAtoms
{
  public class Translation : MonoBehaviour
  {
    public enum LoopType
    {
      Once,
      PingPong,
      Repeat
    }


    [Header("Parameters")]
    [SerializeField] private LoopType loopType;
    [SerializeField] private FloatReference duration;
    [FormerlySerializedAs("activate")]
    [SerializeField] private BoolReference isActive;
    [SerializeField] private AnimationCurve accelCurve;
    [SerializeField] private Vector3 start;
    [SerializeField] private Vector3 end;

    [Header("Debug")]
    [SerializeField] internal bool preview;
    [Range(0, 1)]
    [SerializeField] internal float position;

    private float time = 0f;

    private void Reset()
    {
      start = transform.position;
      end = transform.position + Vector3.forward;
    }

    public void PerformTransform(float position)
    {
      this.position = position;
      var curvePosition = accelCurve.Evaluate(position);
      transform.position = Vector3.Lerp(start, end, curvePosition);
    }

    public void FixedUpdate()
    {
      if (isActive.Value)
      {
        time = time + Time.deltaTime / duration.Value;
        float position =
         loopType == LoopType.Once
         ? Mathf.Clamp01(time)
         : loopType == LoopType.PingPong
         ? Mathf.PingPong(time, 1f)
         : loopType == LoopType.Repeat
         ? Mathf.Repeat(time, 1f)
         : 0;

        PerformTransform(position);
      }
    }
  }
}
