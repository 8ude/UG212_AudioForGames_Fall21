using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class ParticleSystemCallbacks : MonoBehaviour
  {
    public void ChangeStartColor(Color c)
      => DoForSelfAndChild(ps =>
      {
        var change = ps.main;
        change.startColor = c;
      });

    public void ChangeRateMultiplier(float r)
        => DoForSelfAndChild(ps =>
        {
          var change = ps.emission;
          change.rateOverTimeMultiplier = r;
        });

    public void Play()
      => DoForSelfAndChild(ps => ps.Play());

    public void Stop()
      => DoForSelfAndChild(ps => ps.Stop());

    private List<ParticleSystem> selfAndChildren = null;

    private void DoForSelfAndChild(Action<ParticleSystem> action)
    {
      if (selfAndChildren == null)
      {
        selfAndChildren = new List<ParticleSystem>();

        // Self
        {
          var ps = GetComponent<ParticleSystem>();
          if (ps != null)
          {
            action(ps);
          }
        }

        // Children
        selfAndChildren.AddRange(GetComponentsInChildren<ParticleSystem>());
      }

      selfAndChildren.ForEach(action);
    }
  }
}
