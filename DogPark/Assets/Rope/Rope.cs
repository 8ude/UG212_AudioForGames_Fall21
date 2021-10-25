using System.Linq;
using Mirror;
using MutCommon.UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

public class Rope: NetworkBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("The total length of the rope.")]
    private FloatReference fLength;

    [SerializeField]
    [Tooltip("The number of nodes in the rope.")]
    private IntReference fNodeCount;

    [SerializeField]
    [Tooltip("A list of predefined rope nodes.")]
    private GameObject[] fNodes;

    [SerializeField]
    [Tooltip("The prefab for the rope nodes if fNodes is not set.")]
    private GameObject fNodePrefab;

    // -- props --
    // the line renderer attached to this game object
    private LineRenderer mLine;
    // the list of all the nodes, including the head and tail
    private RopeNode[] mNodes;
    // the object to anchor the head to on client start
    [SyncVar] [SerializeField] [HideInInspector] private GameObject mHeadAnchor;
    // the object to anchor the tail to on client start
    [SyncVar] [SerializeField] [HideInInspector] private GameObject mTailAnchor;

    // -- lifecycle --
    protected void Awake() {
        // capture component refs
        mLine = GetComponent<LineRenderer>();

        // listen to variable change events
        X.Assert.Unwrap(fNodeCount.GetChanged()).Register(Reset);
        X.Assert.Unwrap(fLength.GetChanged()).Register(Reset);
    }

    protected void Update() {
        DrawLine();
    }

    // -- NetworkBehaviour --
    public override void OnStartClient() {
        base.OnStartClient();

        Build();
        AnchorTo(mHeadAnchor);
        AnchorTo(mTailAnchor);
        Layout();
    }

    // -- commands --
    public void Reset() {
        Build();
        Layout();
    }

    // rebuilds the list of rope nodes
    private void Build() {
        RebuildInnerNodes();
    }

    // resets the rope layout based on the current length and nodes
    private void Layout() {
        ResetLength();
        ReconnectJoints();
    }

    // sets the anchors to wire on client start
    public void SetAnchors(GameObject head, GameObject tail) {
        Debug.Assert(head.GetComponent<RopeAttachable>(), "Rope - head missing attachable!");
        Debug.Assert(tail.GetComponent<RopeAttachable>(), "Rope - tail missing attachable!");

        mHeadAnchor = head;
        mTailAnchor = tail;
    }

    // anchor the object free end of the rope
    private void AnchorTo(GameObject obj) {
        var anchor = X.Assert.Unwrap(FindAnchor(obj));

        if (Head.AnchorTo(anchor)) {
            return;
        }

        if (Tail.AnchorTo(anchor, isTail: true)) {
            return;
        }

        Log.Debug("Rope - tried to anchor, but there were no endpoints available.");
    }

    // recreate nodes based on current node count
    private void RebuildInnerNodes() {
        // if we have predefined nodes, map them into rope nodes
        if (fNodes != null && fNodes.Length != 0) {
            mNodes = fNodes.Select((n) => new RopeNode(n)).ToArray();
        }
        // otherwise, destroy and instantiate new nodes
        else {
            // destroy existing inner nodes
            if (mNodes != null) {
                foreach (var node in mNodes) {
                    Destroy(node.GameObject);
                }
            }

            // rebuild array of all nodes
            mNodes = InstantiateNodes();
        }

        // sync the line renderer's node count
        mLine.positionCount = mNodes.Length;
    }

    // resets the length of the rope, repositioning each vertex
    private void ResetLength() {
        // each node is equidistant so that the joints configure properly
        var delta = fLength.Value / (mNodes.Length - 1);

        // position rope relative to the head
        var pos = Head.Position;
        var dir = Vector3.Normalize(Tail.Position - Head.Position);

        // position each node relative to the last
        foreach (var node in mNodes.Skip(1)) {
            pos += dir * delta;
            node.Position = pos;
        }
    }

    // reconnects joints based on the their current physical distance
    private void ReconnectJoints() {
        for (var i = 0; i < mNodes.Length - 1; i++) {
            var joint = mNodes[i];
            var other = mNodes[i + 1];
            joint.ConnectTo(other);
        }
    }

    // redraw the line along the vertices of the rope
    private void DrawLine() {
        if (mNodes == null) {
            return;
        }

        var i = 0;
        foreach (var node in mNodes) {
            mLine.SetPosition(i, node.Rigidbody.position);
            i++;
        }
    }

    // -- queries --
    private RopeNode Head => mNodes.First();
    private RopeNode Tail => mNodes.Last();

    private Rigidbody FindAnchor(GameObject obj) {
        var attachable = obj.GetComponent<RopeAttachable>();
        if (attachable) {
            return attachable.anchor;
        } else {
            return obj.GetComponent<Rigidbody>();
        }
    }

    // -- factories --
    // builds rope nodes from the prefab
    private RopeNode[] InstantiateNodes() {
        var count = fNodeCount.Value;
        if (count < 0) {
            count = 0;
        }

        var nodes = new RopeNode[count];
        for (var i = 0; i < count; i++) {
            var instance = Instantiate(fNodePrefab, parent: transform);
            instance.name = "RopeNode[" + i + "]";
            nodes[i] = new RopeNode(instance);
        }

        return nodes;
    }
}
