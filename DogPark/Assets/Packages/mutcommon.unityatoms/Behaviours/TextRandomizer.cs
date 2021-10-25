using UnityEngine;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using System.Linq;

namespace MutCommon.UnityAtoms
{
  public class TextRandomizer : MonoBehaviour
  {
    [SerializeField] private List<StringReference> Alternatives;
    [SerializeField] private StringVariable RandomString;

    void Awake()
    {
      RandomString.Value = "";
      Randomize();
    }

    public void Randomize()
    {
      var uniqueAlternatives = Alternatives.Where((s, i) => s != RandomString.Value);
      var newRandom = Random.Range(0, uniqueAlternatives.Count());
      var randomText = uniqueAlternatives.ElementAt(newRandom);

      RandomString.Value = randomText;
    }
  }
}