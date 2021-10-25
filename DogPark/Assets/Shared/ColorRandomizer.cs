using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class ColorRandomizer : MonoBehaviour
  {
    [SerializeField] private ColorReference target;
    [SerializeField] private Gradient gradient;


    public void Randomize()
    {
      target.Value = Random.ColorHSV();
    }

    public void RandomizeGradient()
    {
      target.Value = gradient.Evaluate(Random.value);
    }
  }
}