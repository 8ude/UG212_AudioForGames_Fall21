using UnityEngine;
using Mirror;
using MutCommon;

public class SpawnPet : NetworkBehaviour
{
    public GameObject PetPrefab;
    public GameObject RopePrefab;
    public GameObject PlayerHand;
    public Transform spawnPosition;

    public void Call() {
        CmdSpawn();
    }

    [Command]
    void CmdSpawn(NetworkConnectionToClient sender = null) {
        var pos = spawnPosition.position;
        var player = gameObject;

        // find a pet hue that complements the player
        var hue = GetComponent<RandomColor>().hue;
        var complement = Mathf.Repeat(hue + 0.5f, 1.0f);

        // spawn the pet first
        var pet = Instantiate(PetPrefab, pos, Random.rotationUniform);
        pet.GetComponent<Owner>().Assign(player);
        pet.GetComponent<RandomColor>().hue = complement;
        NetworkServer.Spawn(pet, sender);

        // spawn the rope, attaching it to the player and pet
        var rope = Instantiate(RopePrefab, pos, Random.rotationUniform);
        rope.GetComponent<RemoveForeignRigidbodies>().SetOwner(player);
        rope.GetComponent<Rope>().SetAnchors(head: player, tail: pet);
        NetworkServer.Spawn(rope, sender);
    }
}
