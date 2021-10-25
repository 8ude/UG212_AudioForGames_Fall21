using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace MutCommon.UnityAtoms
{
  public class StopWatch : MonoBehaviour
  {
    [SerializeField] private BoolVariable isOn;
    [SerializeField] private FloatVariable percentLeft;
    [SerializeField] private FloatReference duration;
    [SerializeField] private BoolReference isRealTime;
    [SerializeField] private BoolReference isOver;
    [SerializeField] private UnityEvent onDone;



    // Start is called before the first frame update
    private void Awake()
    {
      percentLeft.SetChangedIfNull();
      Reset();
    }

    void Toggle()
    {
      isOn.Value = !isOn.Value;
    }

    private float timeLeft = 0;
    void Reset()
    {
      timeLeft = duration;
      percentLeft.Value = 1.0f;
      isOver.Value = false;
    }

    // Update is called once per frame
    void Update()
    {
      if (isOn.Value)
      {
        timeLeft -= (isRealTime.Value ? Time.unscaledDeltaTime : Time.deltaTime);
        percentLeft.Value = timeLeft / duration.Value;
        if (percentLeft.Value < 0 && isOver.Value == false)
        {
          isOver.Value = true;
          onDone?.Invoke();
        }
      }
    }
  }
}