using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

namespace MutCommon.UnityAtoms.UI
{
  [RequireComponent(typeof(Slider))]
  [ExecuteInEditMode]
  public class SliderAtomVariables : MonoBehaviour
  {
    [SerializeField] private FloatVariable CurrentValue;
    [SerializeField] private FloatReference MinValue;
    [SerializeField] private FloatReference MaxValue;

    [Header("UI elements")]
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text CurrentValueText;
    [SerializeField] private TMP_Text MinValueText;
    [SerializeField] private TMP_Text MaxValueText;

    [SerializeField] private string numberFormat;



    private Slider slider;

    // Start is called before the first frame update
    void Awake()
    {
      slider = GetComponent<Slider>();
      NameText.text = CurrentValue?.name ?? "null";
      CurrentValueText.text = CurrentValue?.Value.ToString(numberFormat) ?? "null";
      MinValueText.text = MinValue?.Value.ToString(numberFormat) ?? "null";
      MaxValueText.text = MaxValue?.Value.ToString(numberFormat) ?? "null";
    }

    void Start()
    {
      slider.value = CurrentValue.Value;
      slider.minValue = MinValue.Value;
      slider.maxValue = MaxValue.Value;
      CurrentValue.SetChangedIfNull()?.Register(v =>
      {
        CurrentValueText.text = CurrentValue.Value.ToString(numberFormat);
        slider.value = v;
      });
      MinValue.GetChanged()?.Register(v =>
      {
        MinValueText.text = MinValue.Value.ToString(numberFormat);
        slider.minValue = v;
      });
      MaxValue.GetChanged()?.Register(v =>
      {
        MaxValueText.text = MaxValue.Value.ToString(numberFormat);
        slider.maxValue = v;
      });
    }
  }
}