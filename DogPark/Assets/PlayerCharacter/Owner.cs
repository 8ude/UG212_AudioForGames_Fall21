using Mirror;
using UnityEngine;

public class Owner: NetworkBehaviour {
  // -- fields --
  [SyncVar]
  [SerializeField]
  [Tooltip("The associated player.")]
  private GameObject mPlayer;

  // -- commands --
  public void Assign(GameObject player) {
    mPlayer = player;
  }

  // -- queries --
  public bool IsLocalPlayer => mPlayer.GetComponent<NetworkIdentity>().isLocalPlayer;
}
