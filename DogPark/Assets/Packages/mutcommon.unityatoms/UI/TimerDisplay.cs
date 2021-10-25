using UnityEngine;
using TMPro;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms.UI
{
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class TimerDisplay : MonoBehaviour
  {
    private TextMeshProUGUI timerText;

    [SerializeField]
    private FloatVariable timer;

    public void UpdateTimer(float time)
    {
      string timeFormat;
      int minutes = (int)time / 60;
      int seconds = (int)time % 60;
      string secondsAux = "";
      string minutesAux = "";
      if (seconds < 10)
        secondsAux = "0";
      if (minutes < 10)
        minutesAux = "0";

      timeFormat = minutesAux + minutes + ":" + secondsAux + seconds;
      timerText.text = timeFormat;
    }

    private void Awake()
    {
      timerText = GetComponent<TextMeshProUGUI>();
      timer.Changed = timer.Changed
      ?? ScriptableObject.CreateInstance<FloatEvent>();

      timer.Changed.Register(UpdateTimer);
      UpdateTimer(timer.Value);
    }
  }
}
