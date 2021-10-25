using UnityEngine;
using UnityAtoms.BaseAtoms;
using TMPro;

public class Menu: MonoBehaviour {
    // -- props --
    private string mIp;

    // -- p/outlets
    [SerializeField]
    private StringEvent mStartEvent;

    // -- events --
    public void DidChangeIp(string ip) {
        mIp = ip;
    }

    public void DidClickConnect() {
        mStartEvent.Raise(mIp.Trim());
    }
}
