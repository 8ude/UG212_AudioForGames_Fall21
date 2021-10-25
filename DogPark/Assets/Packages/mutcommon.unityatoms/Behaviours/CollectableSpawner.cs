using System.Collections;
using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace MutCommon.UnityAtoms
{
  public class CollectableSpawner : MonoBehaviour
  {
    [Header("Parameters")]
    public FloatReference CooldownTime;

    public GameObjectReference ThingToSpawn;

    public Transform SpawnTransform;

    [Header("State Variables")]
    // TODO: Se um dia implementarem isso, dá pra fazer o tempo restante e a bool serem atoms e pra usar em UI: https://github.com/AdamRamberg/unity-atoms/issues/53
    [SerializeField]
    private float cooldown;

    public float Cooldown => cooldown;

    [SerializeField]
    private bool isInCooldown = false;

    public bool IsInCooldown => isInCooldown;

    [SerializeField] public bool spawnOnAwake = false;
    [SerializeField] public bool respawnOnDestroy = false;

    private void OnValidate()
    {
      if (SpawnTransform == null) SpawnTransform = transform;
    }

    private void Awake()
    {
      if (spawnOnAwake)
      {
        CreateThing();
      }
    }

    // TODO: add condition for when it can be respawned (AKA: dota's pull)
    //public bool CanSpawn;

    void CreateThing()
    {
      var thing = Instantiate(ThingToSpawn.Value, SpawnTransform.position, SpawnTransform.rotation, this.transform);
      thing.SetActive(true);

      if (respawnOnDestroy)
      {
        var triggerEvent = thing.GetComponent<OnDestroyUnityEvent>() ?? thing.AddComponent<OnDestroyUnityEvent>();
        if (triggerEvent.callback == null)
        {
          triggerEvent.callback = new UnityEngine.Events.UnityEvent();
        }
        triggerEvent.callback.AddListener(() =>
        {
          if (!this.isActiveAndEnabled) return;
          this.DoNextFrame(() =>
          {
            if (!this.isActiveAndEnabled) return;
            Respawn();
          });
        });
      }
    }

    private IEnumerator RespawnCoroutine()
    {

      isInCooldown = true;
      CreateThing();

      yield return CoroutineHelpers.InterpolateByTime(CooldownTime.Value, (k) =>
      {
        cooldown = 1 - k;
      });

      isInCooldown = false;
    }

    [ContextMenu("Respawn")]
    public void Respawn()
    {
      if (isInCooldown)
      {
        //UberDebug.LogWarningChannel("MutCommon.unityatoms", "Tried to call respwan on a respawner on cooldown");
        return;
      };

      StartCoroutine(RespawnCoroutine());
    }
  }
}