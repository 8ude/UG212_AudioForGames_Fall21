using UnityEngine;
using System.Collections;
using UnityAtoms.BaseAtoms;
using UnityEngine.Events;

namespace MutCommon.UnityAtoms
{
  public class Timer : MonoBehaviour
  {
    // TODO: custom editor que faz ser possivel ativar o timer pelo unity
    [SerializeField]
    private FloatReference Duration;

    [SerializeField]
    private UnityEvent Callbacks;

    [SerializeField]
    [Tooltip("How much of the current timeout has elapsed (0 -> 1)")]
    private FloatVariable Elapsed;

    public void TimeoutCustom(float duration) => StartCoroutine(DoTimeout(duration));
    public void TimeoutCustom(FloatConstant duration) => TimeoutCustom(duration.Value);
    public void Timeout() => TimeoutCustom(Duration.Value);

    IEnumerator DoTimeout(float duration)
    {
      if (Elapsed != null)
        yield return CoroutineHelpers.InterpolateByTime(duration, k => Elapsed.SetValue(k));
      else
        yield return new WaitForSeconds(duration);

      Callbacks?.Invoke();
    }

    public void TimeoutRealtimeCustom(float duration) => StartCoroutine(DoTimeoutRealtime(duration));
    public void TimeoutRealtimeCustom(FloatConstant duration) => TimeoutRealtimeCustom(duration.Value);
    public void TimeoutRealtime() => TimeoutRealtimeCustom(Duration.Value);

    IEnumerator DoTimeoutRealtime(float duration)
    {
      if (Elapsed != null)
        yield return CoroutineHelpers.InterpolateByUnscaledTime(duration, k => Elapsed.SetValue(k));
      else
        yield return new WaitForSecondsRealtime(duration);

      Callbacks?.Invoke();
    }

    public void TimeoutFrames(int frames) => StartCoroutine(DoTimeoutFrames(frames));
    public void TimeoutFrames(IntConstant frames) => TimeoutFrames(frames.Value);

    IEnumerator DoTimeoutFrames(int frames)
    {
      for (int i = 0; i < frames; i++)
      {
        if (Elapsed != null)
          Elapsed.SetValue((float)i / (float)frames);
        yield return null;
      }
      Callbacks?.Invoke();
    }
  }
}