using UnityEngine;
using UnityEngine.Assertions;

public class RopeNode {
    // -- props --
    private GameObject mGameObject;
    private Rigidbody mBody;
    private CharacterJoint mJoint;
    private Joint mAnchor;

    // -- lifetime --
    public RopeNode(GameObject obj) {
        mGameObject = obj;
        mBody = obj.GetComponent<Rigidbody>();
        mJoint = obj.GetComponent<CharacterJoint>();
    }

    // -- commands --
    // anchor this node to an external object, if its the tail (hack), is reuse's the
    // node's character joint.
    public bool AnchorTo(Rigidbody anchor, bool isTail = false) {
        if (mAnchor) {
            return false;
        }

        // move this node to the position of the anchor
        Position = anchor.transform.position;

        // create an anchor between the node and the target
        if (isTail) {
            mAnchor = mJoint;
        } else {
            mAnchor = mGameObject.AddComponent<FixedJoint>();
        }

        mAnchor.connectedBody = anchor;

        return true;
    }

    public void Unanchor() {
        if (mAnchor == null) {
            return;
        }

        Object.Destroy(mAnchor);
        mAnchor = null;
    }

    public void ConnectTo(RopeNode other) {
        Assert.IsNotNull(mJoint);
        mJoint.connectedBody = other.Rigidbody;
    }


    // -- queries --
    public GameObject GameObject => mGameObject;

    public Rigidbody Rigidbody => mBody;

    public Vector3 Position {
        get => GameObject.transform.position;
        set => GameObject.transform.position = value;
    }
}
