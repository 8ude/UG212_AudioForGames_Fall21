using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityAtoms.BaseAtoms;

public class ServerObjectSpawner : NetworkBehaviour {
    public enum SpawnLimitMode {
        DeleteOldest,
        StopSpawning
    }
    
    [SerializeField] private GameObject thingToSpawnPrefab;
    [SerializeField] private FloatReference spawnDelay;
    [SerializeField] private IntReference maxObjects;
    [SerializeField] private SpawnLimitMode limitMode;
    [Header("PANIC")]
    [SerializeField] private bool PANIC;

    private IList<GameObject> thingsSpawned = new List<GameObject>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(SpawnThing());
    }

    private IEnumerator SpawnThing() {
        while(!PANIC) {
            while(spawnDelay.Value <= 0) yield return null;
            // this is awful
            thingsSpawned = thingsSpawned.Where(go => go != null).ToList();
            while(thingsSpawned.Count() >= maxObjects.Value) {
                if(limitMode == SpawnLimitMode.DeleteOldest) {
                    thingsSpawned.RemoveAt(0);
                } 
                else {
                    yield return null;
                }
            }
            var thing = Instantiate(thingToSpawnPrefab, transform.position, transform.rotation);
            //NetworkServer.Spawn(thing);
            thingsSpawned.Add(thing);
            yield return new WaitForSeconds(spawnDelay.Value);
        }
    }
}
