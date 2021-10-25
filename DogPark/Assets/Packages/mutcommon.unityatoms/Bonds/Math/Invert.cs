using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class Invert : FloatVariableInstancer
  {
    public FloatReference Input;

    [Header("Optional")]
    public FloatVariable OutputVariable;

    private void Start()
    {
      if (OutputVariable != null)
      {
        this.Variable.Changed.Register(v => OutputVariable.SetValue(v));
      }

      if (Input == null)
      {
        Debug.LogError("No Input variable for Invert");
      }

      if (Input.Usage == AtomReferenceUsage.VARIABLE || Input.Usage == AtomReferenceUsage.VARIABLE_INSTANCER)
      {
        Input.GetEvent<FloatEvent>().Register(i => this.Variable.SetValue(Calculate()));
      }
      this.Variable.SetValue(Calculate());
    }

    float Calculate()
      => 1.0f / Input.Value;
  }
}