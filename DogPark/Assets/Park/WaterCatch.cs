using UnityEngine;

public sealed class WaterCatch {
    // -- props --
    private readonly Body[] mBodies;
    private int mFlowCount = 0;
    private bool mIsInitialFlow = true;

    // -- lifetime --
    public WaterCatch(Rigidbody[] rigidbodies) {
        mBodies = CreateBodies(rigidbodies);
    }

    // -- commands --
    public void EnterFlow() {
        mFlowCount += 1;
    }

    public void OffsetDrag(float offset) {
        foreach (var body in mBodies) {
            body.OffsetDrag(offset);
        }
    }

    public void ResetDrag() {
        foreach (var body in mBodies) {
            body.ResetDrag();
        }
    }

    public void ExitFlow() {
        // once we exit any flow, turn off this flag so that the next clump
        // can pull the item in
        if (mIsInitialFlow) {
            mIsInitialFlow = false;
        }

        mFlowCount -= 1;
    }

    // -- commands/physics
    public void AddForce(Vector3 force) {
        foreach (var body in mBodies) {
            body.AddForce(force);
        }
    }

    // -- queries --
    public bool IsInFlow() {
        return mFlowCount > 0;
    }

    public bool IsInInitialFlow() {
        return mIsInitialFlow;
    }

    // -- queries/physics
    public Vector3 Position => mBodies[0].Position;

    // -- comparison --
    public override int GetHashCode() {
        return mBodies[0].GetHashCode();
    }

    // -- factories --
    private static Body[] CreateBodies(Rigidbody[] rigidbodies) {
        var bodies = new Body[rigidbodies.Length];

        for (var i = 0; i < rigidbodies.Length ; i++) {
            bodies[i] = new Body(rigidbodies[i]);
        }

        return bodies;
    }

    // -- children --
    private struct Body {
        // -- props --
        private readonly Rigidbody mRigidbody;
        private readonly float mInitialDrag;
        private readonly float mInitialAngularDrag;

        // -- lifetime --
        public Body(Rigidbody rigidbody) {
            mRigidbody = rigidbody;
            mInitialDrag = mRigidbody.drag;
            mInitialAngularDrag = mRigidbody.angularDrag;
        }

        // -- commands --
        public void OffsetDrag(float offset) {
            mRigidbody.drag += offset;
            mRigidbody.angularDrag += offset;
        }

        public void ResetDrag() {
            mRigidbody.drag = mInitialDrag;
            mRigidbody.angularDrag = mInitialAngularDrag;
        }

        // -- commands/physics
        public void AddForce(Vector3 force) {
            mRigidbody.AddForce(force, ForceMode.Acceleration);
        }

        // -- queries --
        public Vector3 Position => mRigidbody.transform.position;

        // -- comparison --
        public override int GetHashCode() {
            return mRigidbody.GetHashCode();
        }
    }
}
