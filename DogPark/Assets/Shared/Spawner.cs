using UnityEngine;
using UnityAtoms.BaseAtoms;
using Mirror;

public class Spawner : NetworkBehaviour
{
  [SerializeField] private NetworkIdentity prizePrefab;

  [Header("Tunables")]
  [SerializeField] private FloatReference InitialPrizeCount;
  [SerializeField] private FloatReference RandomSpawnRadius;

  [Header("Debug")]
  [SerializeField] private Color gizmosColor;

  public override void OnStartServer()
  {
    // Spawn multiple prizes once the server starts
    for (int i = 0; i < InitialPrizeCount.Value; i++)
      SpawnPrize();
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = gizmosColor;
    Gizmos.DrawWireSphere(transform.position, RandomSpawnRadius.Value);
  }

  public void SpawnPrize()
  {
    var randomCircle = Random.insideUnitCircle * RandomSpawnRadius;
    Vector3 spawnPosition = new Vector3(randomCircle.x, 0, randomCircle.y);

    // spawn as child of the spawner that's already in the additive scene at 0,0,0 so we don't have to move it
    GameObject newPrize = Instantiate(prizePrefab.gameObject, spawnPosition, Quaternion.identity, transform);
    //Reward reward = newPrize.gameObject.GetComponent<Reward>();
    //reward.spawner = this;

    NetworkServer.Spawn(newPrize);
  }
}
