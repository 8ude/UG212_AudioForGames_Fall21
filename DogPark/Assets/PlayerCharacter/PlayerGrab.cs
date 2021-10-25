using UnityEngine;
using MutCommon;
using UnityAtoms.BaseAtoms;
using System.Linq;
using UnityEngine.Serialization;
using System;

public class PlayerGrab : MonoBehaviour {
    private enum PlayerArmMoveMode {
        mesh,
        curves
    }

    private enum PlayerGrabType {
       kinematic,
       joint,
    }

    private enum GrabRaycastMode {
        cursorPosition,
        handPosition
    }

    // -- fields --
    [SerializeField] private FloatReference radius;
    [SerializeField] private FloatReference sphereCastRadius;
    [SerializeField] private FloatReference holdDistance;
    [SerializeField] private FloatReference handInterpolation;
    [SerializeField] private IntReference velocityBufferedFrames;
    [SerializeField] private FloatReference handCenterHeightOffset;
    [SerializeField] private FloatReference handHeightCurveConstant;

    [SerializeField] private FloatReference justReleasedDuration;

    [FormerlySerializedAs("layer")]
    [SerializeField] private string grabbableLayer = "Grabbable";
    private LayerMask grabbableLayerMask;
    private int grabbableLayerIndex;

    [SerializeField] private string justReleasedLayer = "JustReleased";
    private int justReleasedLayerIndex;

    [Header("Hand references")]
    [SerializeField] private GameObject handTarget;
    [SerializeField] private Transform handTransform;
    [SerializeField] private Transform shoulderTransform;
    [SerializeField] private Collider holdMesh;
    [SerializeField] private Collider playerCollider;

    [Header("Move Tunables")]
    [SerializeField] public FloatReference mouseSensitivity;
    [SerializeField] private PlayerArmMoveMode armMoveMode;
    [SerializeField] private PlayerGrabType grabType;
    [SerializeField] private GrabRaycastMode grabRaycastMode;

    // -- props --
    private Vector3 expectedHandPosition;
    private bool isDragging;
    private Rigidbody currentTarget;
    private Rigidbody currentHold;
    private Vector3 currentTargetPoint = Vector3.zero;
    private Vector3 currentHoldOffset = Vector3.zero;
    private Vector3 targetNormal = Vector3.zero;

    // saves all the previous frames velocity
    private Vector3[] velocityBuffer;
    // the current index in the buffer
    private int velocityBufferIndex;

    private Camera mCamera => Camera.main;
    private float maxDistance => Vector3.Distance(mCamera.transform.position, transform.position) + radius;

    private Vector3 cameraForwardOnXZPlane => Vector3.ProjectOnPlane(mCamera.transform.forward, Vector3.up).normalized;
    private Plane idlePlane => new Plane(cameraForwardOnXZPlane, transform.position + cameraForwardOnXZPlane * holdDistance);

    private Vector2 cursorPosition;


    // -- lifecycle
    protected void Awake() {
        velocityBuffer = new Vector3[velocityBufferedFrames.Value];
        if(grabType != PlayerGrabType.joint) {
            Destroy(handTarget.GetComponent<Joint>());
        }
        cursorPosition = Vector2.zero;

        grabbableLayerMask = LayerMask.GetMask(grabbableLayer, justReleasedLayer); // justreleased objects can be grabbed
        grabbableLayerIndex = LayerMask.NameToLayer(grabbableLayer);
        justReleasedLayerIndex = LayerMask.NameToLayer(justReleasedLayer);
    }

    // Update is called once per frame
    protected void FixedUpdate() {
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
        cursorPosition += mouseSensitivity.Value * Time.deltaTime * new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0f, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0f, Screen.height);

        // if we're not dragging an object, move/snap the mouse freely
        if (!isDragging) {
            // check if the hand hits a grabbable object
            var ray = Ray();
            if (Physics.SphereCast(ray, sphereCastRadius.Value, out var hitInfo, maxDistance, grabbableLayerMask)) {
                var target = hitInfo.rigidbody;

                if (target && Vector3.Distance(hitInfo.point, transform.position) < radius.Value) {
                    expectedHandPosition = hitInfo.point;
                    if(target != currentTarget) {
                        NotifyUntargetted(currentTarget);
                        currentTarget = target;
                        NotifyTargetted(currentTarget);
                    }
                    currentTargetPoint = hitInfo.point;
                    targetNormal = hitInfo.normal;
                } else {
                    NotifyUntargetted(currentTarget);
                    currentTarget = null;
                }
            } else {
                NotifyUntargetted(currentTarget);
                currentTarget = null;
            }

            // otherwise, snap to the mouse position
            if (currentTarget == null) {
                // update position
                expectedHandPosition = FindExpectedHandPositionIdle();
            }
        }

        // if we have a target, run grab controls
        var input = Inputs.Play;
        if (currentTarget != null) {
            if (input.GetMouseButtonDown(0))  {
                StartDrag();
            } else if (isDragging && input.GetMouseButton(0)) {
                Drag();
            } else if (isDragging && input.GetMouseButtonUp(0)) {
                ReleaseDrag();
            }
        }

        handTarget.transform.position = Vector3.Slerp(handTarget.transform.position, expectedHandPosition, handInterpolation.Value);
    }

    // -- commands --
    private Vector3? lastValidDragHandPositionOffset;
    private Vector3 ExpectedHandPositionMesh() {
        var cursorRay = mCamera.ScreenPointToRay(cursorPosition);
        // Raycast from the camera to the holdMesh
        if(holdMesh.Raycast(cursorRay, out RaycastHit hitInfo, maxDistance)) {
            // save the offset so that if the character moves, the offset is constant (hitInfo.point is a global thing)
            lastValidDragHandPositionOffset = (hitInfo.point - transform.position);
        }

        return transform.position + (lastValidDragHandPositionOffset
                                        ?? (holdMesh.transform.position - transform.position));

    }

    private Vector3 FindExpectedHandPositionIdle() {
        var mouseX01 = cursorPosition.x/Screen.width;
        var mouseY01 = cursorPosition.y/Screen.height;
        return curvesIdle.ExpectedHandPositionCurves(mouseX01, mouseY01, transform, mCamera.transform);
        /*
        var cursorRay = mCamera.ScreenPointToRay(cursorPosition);
        idlePlane.Raycast(cursorRay, out float distance);
        var position =  cursorRay.origin + cursorRay.direction * distance;
        //var deltaY = position.y - transform.position.y + handCenterHeightOffset;
        //position -= cameraForwardOnXZPlane * ArmCurveDeltaY(deltaY);
        */

        //return position;
    }

    private float ArmCurveDeltaY(float deltaY) {
        return -handHeightCurveConstant.Value * Mathf.Min(deltaY, 0);
    }

    private Vector3 lastHandTargetPosition;
    private void StartDrag() {
        isDragging = true;
        velocityBuffer = new Vector3[velocityBufferedFrames.Value];
        // TODO: might be helpful to convert target into a `GrabHold` object that can capture component
        // references and simplify some of the logic
        currentHold = currentTarget;
        if(grabType == PlayerGrabType.kinematic) {
            currentHold.isKinematic = true;
        } else {
            var joint = handTarget.GetComponent<Joint>();
            joint.connectedBody = currentHold;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = currentTargetPoint;
        }

        // notify listeners
        NotifyGrabbed(currentHold);

        currentHold.GetComponent<Collider>().isTrigger = !false;
        currentHoldOffset = handTarget.transform.position - currentHold.transform.position;
        lastHandTargetPosition = handTarget.transform.position;
        Drag();
        Cursor.visible = false;
    }

    private void NotifyListeners(Component o, Action<GrabListener> callback) {
        if(o == null) return;
        foreach (var listener in o.GetComponents<GrabListener>())
        {
            callback(listener);
        }
    }

    private void NotifyGrabbed(Component o) => NotifyListeners(o, l => l.OnGrabbed());
    private void NotifyReleased(Component o) => NotifyListeners(o, l => l.OnReleased());
    private void NotifyTargetted(Component o)=> NotifyListeners(o, l => l.OnTargetted());
    private void NotifyUntargetted(Component o)=> NotifyListeners(o, l => l.OnUntargetted());

    [FormerlySerializedAs("curvesThrow")]
    public CurveParametersForHand curvesIdle;
    [FormerlySerializedAs("curvesIdle")]
    public CurveParametersForHand curvesDrag;

    [Serializable]
    public class CurveParametersForHand {
        [SerializeField] private AnimationCurve xCurve;
        [SerializeField] private FloatReference xMultiplier;
        [SerializeField] private AnimationCurve yCurve;
        [SerializeField] private FloatReference yMultiplier;
        [SerializeField] private AnimationCurve zCurve;
        [SerializeField] private FloatReference zMultiplier;
        [SerializeField] private FloatReference curveNormalDelta;

        public Vector3 ExpectedHandPositionCurves(float x, float y, Transform transform, Transform camera) {
            return transform.position + camera.TransformVector(new Vector3(
                xCurve.Evaluate(x) * xMultiplier.Value,
                yCurve.Evaluate(y) * yMultiplier.Value,
                zCurve.Evaluate(y) * zMultiplier.Value));
        }
        
    }

    private void Drag() {
        Vector3 handNormal = handTarget.transform.position - expectedHandPosition;
        if(armMoveMode == PlayerArmMoveMode.curves) {
            var mouseX01 = cursorPosition.x/Screen.width;
            var mouseY01 = cursorPosition.y/Screen.height;
            expectedHandPosition = curvesDrag.ExpectedHandPositionCurves(mouseX01, mouseY01, transform, mCamera.transform);
        } else if(armMoveMode == PlayerArmMoveMode.mesh) {
            //Debug.Log(velocityBuffer[velocityBufferIndex]);
            expectedHandPosition = ExpectedHandPositionMesh();
        }

        velocityBufferIndex = (velocityBufferIndex + 1) % velocityBufferedFrames;
        velocityBuffer[velocityBufferIndex] = (handTarget.transform.position - lastHandTargetPosition)/Time.deltaTime;
        lastHandTargetPosition = handTarget.transform.position;

        if(grabType == PlayerGrabType.kinematic) {
            //var cross = Vector3.Cross(targetNormal, handNormal);
            currentHold.transform.position = handTarget.transform.position + currentHoldOffset;//+ Quaternion.AngleAxis(Mathf.Asin(cross.magnitude), cross) * currentHoldOffset;
            //currentHold.transform.rotation = Quaternion.
        }
    }

    private void ReleaseDrag() {
        isDragging = false;

        if(grabType == PlayerGrabType.kinematic) {
            var finalVelocity = velocityBuffer[(velocityBufferIndex + 1) % velocityBufferedFrames];
            currentHold.isKinematic = false;
            currentHold.transform.parent = null;
            currentHold.velocity = finalVelocity;
        } else {
            handTarget.GetComponent<Joint>().connectedBody = null;
        }

        // notify listeners
        NotifyReleased(currentHold);

        // Move object to JustReleased layer so it won't collide with player
        currentHold.gameObject.layer = justReleasedLayerIndex;
        // Schedule move back to grabbable layer in half a second.
        GameObject obj = currentHold.gameObject;
        Collider c = currentHold.GetComponent<Collider>();
        this.DoAfterTime(justReleasedDuration, () => MoveToGrabbableLayerWhenNotTouchingPlayer(c));

        c.isTrigger = false;
        currentHold = null;
        Cursor.visible = true;
    }


    void MoveToGrabbableLayerWhenNotTouchingPlayer(Collider c) {
        if(!c.bounds.Intersects(playerCollider.bounds)) {
            // Debug.Log(c.bounds);
            // Debug.Log(playerCollider.bounds);
            // Debug.Log("set isTrigger false");
            c.gameObject.layer = grabbableLayerIndex;
        } else {
            // Debug.Log("check next frame");
            this.DoNextFrame(() => MoveToGrabbableLayerWhenNotTouchingPlayer(c));
        }
    }

    // -- queries --
    private Ray Ray() {
        if(grabRaycastMode == GrabRaycastMode.cursorPosition) {
            return mCamera.ScreenPointToRay(cursorPosition);
        }
        else {
            var mouseX01 = cursorPosition.x/Screen.width;
            var mouseY01 = cursorPosition.y/Screen.height;
            var pos = curvesIdle.ExpectedHandPositionCurves(mouseX01, mouseY01, transform, mCamera.transform);
            return new Ray(shoulderTransform.position, (pos - shoulderTransform.position).normalized);
        }
    }

    // -- gizmos --
    private void OnDrawGizmos() {
      var r = new Ray();
      Gizmos.color = Color.magenta;
      Gizmos.DrawLine(r.origin, r.origin + r.direction * maxDistance);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius.Value);
        Gizmos.DrawRay(transform.position, transform.forward * holdDistance.Value);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + transform.forward * holdDistance.Value, 0.08f);
        for(var x = 0.0f; x <= 1; x += 0.02f) {
            for(var y = 0.0f; y <= 1; y += 0.02f) {
                Gizmos.color = Color.yellow;
                var pos = curvesIdle.ExpectedHandPositionCurves(x, y, transform, mCamera.transform);
                Gizmos.DrawSphere(pos, 0.05f);

                Gizmos.color = Color.white;
                pos = curvesDrag.ExpectedHandPositionCurves(x, y, transform, mCamera.transform);
                Gizmos.DrawSphere(pos, 0.05f);
            }
        }
    }
}
