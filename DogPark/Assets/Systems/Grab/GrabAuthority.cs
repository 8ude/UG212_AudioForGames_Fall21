using Mirror;
using UnityEngine.Events;

public class GrabAuthority : NetworkBehaviour, GrabListener
{
    // -- GrabListener --
    public void OnTargetted() {
    }

    public void OnUntargetted() {
    }

    public void OnGrabbed() {
        CmdGetClientAuthority();
        var rb = GetComponent<Mirror.Experimental.NetworkRigidbody>();
        //if (rb) rb.clientAuthority = true;

        var nt = GetComponent<NetworkTransform>();
        //if (nt) nt.clientAuthority = true;
    }

    public void OnReleased() {
        // this.DoAfterTime(30f, () => CmdReleaseClientAuthority());

        var rb = GetComponent<Mirror.Experimental.NetworkRigidbody>();
        //if (rb) rb.clientAuthority = false;

        var nt = GetComponent<NetworkTransform>();
        //if (nt) nt.clientAuthority = false;
    }

    // -- commands --
    // [Command(ignoreAuthority = true)]
    public void CmdGetClientAuthority(NetworkConnectionToClient sender = null)
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
        if (sender != null) GetComponent<NetworkIdentity>().AssignClientAuthority(sender);
    }

    // [Command(ignoreAuthority = true)]
    public void CmdReleaseClientAuthority(NetworkConnectionToClient sender = null)
    {
        // GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }
}
