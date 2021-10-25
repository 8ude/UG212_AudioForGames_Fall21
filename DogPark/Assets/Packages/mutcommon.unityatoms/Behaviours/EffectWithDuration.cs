using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class EffectWithDuration : MonoBehaviour
  {
    [SerializeField] private BoolVariable IsInEffect;
    [SerializeField] private FloatVariable durationLeft;

    // Update is called once per frame
    void Update()
    {
      durationLeft.Value = Mathf.Max(0, durationLeft.Value - Time.deltaTime);
      IsInEffect.Value = durationLeft.Value > 0;
    }

    public void IncreaseDuration(float increase) => durationLeft.Value += increase;

    public void IncreaseDuration(FloatConstant increase) => IncreaseDuration(increase.Value);
  }
}