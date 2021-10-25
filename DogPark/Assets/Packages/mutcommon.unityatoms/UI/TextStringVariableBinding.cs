using UnityEngine;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms.UI
{
  [RequireComponent(typeof(TMPro.TMP_Text))]
  public class TextStringVariableBinding : MonoBehaviour
  {
    [SerializeField] private StringVariable textVariable;
    [SerializeField] private string template = "{0}";

    private TMPro.TMP_Text textComponent;
    // Start is called before the first frame update

    private void Awake()
    {
      textComponent = GetComponent<TMPro.TMP_Text>();
      textVariable.SetChangedIfNull().Register(t => textComponent.text = string.Format(template, t));
      textVariable.Changed.Raise(textVariable.Value);
    }
  }
}