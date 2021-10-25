using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;

namespace MutCommon.UnityAtoms
{
  public class AXplusB : MonoBehaviour//: FloatVariableInstancer
  {
    public FloatReference A;
    [FormerlySerializedAs("In")]
    public FloatReference X;
    public FloatReference B;

    [Header("Optional")]
    [FormerlySerializedAs("Out")]
    public FloatVariable OutputVariable;

    private FloatVariable Variable => OutputVariable;

    private void Start()
    {
      //if (OutputVariable != null)
      //{
      //this.Variable.Changed.Register(v => OutputVariable.SetValue(v));
      //}

      if (X.Usage == AtomReferenceUsage.VARIABLE || X.Usage == AtomReferenceUsage.VARIABLE_INSTANCER)
      {
        X.GetEvent<FloatEvent>().Register(i => this.Variable.SetValue(Calculate()));
      }
      if (A.Usage == AtomReferenceUsage.VARIABLE || A.Usage == AtomReferenceUsage.VARIABLE_INSTANCER)
      {
        A.GetEvent<FloatEvent>().Register(i => this.Variable.SetValue(Calculate()));
      }
      if (B.Usage == AtomReferenceUsage.VARIABLE || B.Usage == AtomReferenceUsage.VARIABLE_INSTANCER)
      {
        B.GetEvent<FloatEvent>().Register(i => this.Variable.SetValue(Calculate()));
      }
      this.Variable.SetValue(Calculate());
    }

    float Calculate()
      => A.Value * X.Value + B.Value;
  }
}