using System;
using UnityEngine;
using UnityAtoms.BaseAtoms;

// MUT: I just copied this code over from the multiplayer project
public class CameraController : MonoBehaviour {
  [SerializeField] private GameObjectReference target;
  [SerializeField] private Transform childCamera;

  [Header("Input Settings")]
  [SerializeField] private FloatReference mouseSensitivity;
  [SerializeField] private FloatReference scrollSensitivity;
  [SerializeField] private BoolReference invertedY;

  [Header("Cursor Settings")]
  [SerializeField] private BoolReference cursorVisible;

  [Header("Camera settings")]
  [SerializeField] private FloatReference pitchMin;
  [SerializeField] private FloatReference pitchMax;

  [Header("Distance from target settings")]
  [SerializeField] private FloatReference camInitialDistance;
  [SerializeField] private FloatReference camMinDistance;
  [SerializeField] private FloatReference camMaxDistance;
  [SerializeField] private FloatReference camDistanceChangeSmooth;

  [Header("Follow Settings")]
  [SerializeField] private FloatReference camFollowSmoothRate;
  [SerializeField] private Vector3Reference targetOffset;

  [Header("Free Settings")]
  [SerializeField] private FloatReference freeSpeed;
  [SerializeField] private FloatReference freeFastSpeed;

  private Vector3 targetSmoothVelocity;
  private float pitch, yaw;
  private IDisposable mDisposable;

  // The distance we are currently interpolating
  private float currentDistanceFromTarget;
  // The target of the previous value's interpolation;
  private float expectedDistanceFromTarget;

  private Vector3 initialRotationEuler;

  // The camera initial offset from it's target;
  private Vector3 _cameraBaseOffset;
  private Vector3 cameraInitialOffset {
    get {
      // Makes sure whenever we want to access cameraBaseOffset it checks if there's a target
      // TODO: _cameraBaseOffset == Vector3.zero?
      if (_cameraBaseOffset == null) {
        if (target.Value == null) return Vector3.zero;
        // If there is a target and the base offset was never set, calculate the initial offset from the target
        _cameraBaseOffset = transform.position - target.Value.transform.position;
      }

      return _cameraBaseOffset;
    }
  }

  private void Awake() {
    mDisposable = cursorVisible.Subscribe(ShowCursor);
  }

  private void Start() {
    currentDistanceFromTarget = camInitialDistance;
    expectedDistanceFromTarget = camInitialDistance;

    initialRotationEuler = transform.eulerAngles;
  }

  private void LateUpdate() {
    if (Inputs.Mode == InputMode.Play) {
      MoveFollowCamera();
    } else { // ControlsMode.Free
      MoveFreeCamera();
    }
  }

  private void OnDestroy() {
    mDisposable?.Dispose();
  }

  // -- commands --
  private void ShowCursor(bool isVisible) {
    Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    Cursor.visible = isVisible;
  }

  private void MoveFollowCamera() {
    // if there's no target, do nothing;
    if (!target.Value) {
      return;
    }

    var input = Inputs.Play;

    if (input.GetKey(KeyCode.Mouse1)) {
      RotateCameraViewport(input);
    }

    // added immediate feedback for pressing Y to re-enable mouse  - Cosmo D
    // if (!input.GetKey(KeyCode.Mouse1)) {
    //   Cursor.lockState = CursorLockMode.Locked;
    //   Cursor.visible = cursorVisible;
    // } else if (input.GetKeyUp(KeyCode.Mouse1)) {
    //   Cursor.lockState = CursorLockMode.Locked;
    //   Cursor.visible = cursorVisible;
    // }

    if (Mathf.Abs(input.mouseScrollDelta.y) > 0) {
      expectedDistanceFromTarget -= input.mouseScrollDelta.y * scrollSensitivity.Value;
      expectedDistanceFromTarget = Mathf.Clamp(expectedDistanceFromTarget, camMinDistance.Value, camMaxDistance.Value);
      currentDistanceFromTarget = Mathf.Lerp(currentDistanceFromTarget, expectedDistanceFromTarget, camDistanceChangeSmooth.Value);
    }

    // set the local forward distance of childed camera
    Vector3 cameraBoomOffset = new Vector3(0.0f, 0.0f, -currentDistanceFromTarget);
    childCamera.localPosition = cameraBoomOffset;

    // SmoothDamp for camera lag
    Vector3 newPosition = target.Value.transform.position + cameraInitialOffset + targetOffset.Value.x * transform.right + targetOffset.Value.y * transform.up;
    transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref targetSmoothVelocity, camFollowSmoothRate.Value);
  }

  private void MoveFreeCamera() {
    var input = Inputs.Free;

    if (input.GetKey(KeyCode.Mouse1)) {
      RotateCameraViewport(input);
    }

    var dir = GetFreeMoveDirection(input);
    var spd = GetFreeMoveSpeed(input);
    transform.position += spd * Time.deltaTime * dir;
  }

  private void RotateCameraViewport(InputBridge input) {
    Cursor.visible = false;

    // yaw for looking side to side
    yaw += input.GetAxis("Mouse X") * mouseSensitivity.Value;

    // pitch for looking up and down
    if (invertedY.Value) {
      pitch += input.GetAxis("Mouse Y") * mouseSensitivity.Value;
    } else {
      pitch -= input.GetAxis("Mouse Y") * mouseSensitivity.Value;
    }

    pitch = Mathf.Clamp(pitch, pitchMin.Value, pitchMax.Value);

    transform.eulerAngles = new Vector3(pitch, yaw) + initialRotationEuler; // starting offset
  }

  // -- queries --
  private Vector3 GetFreeMoveDirection(InputBridge input) {
    var direction = Vector3.zero;

    if (input.GetKey(KeyCode.W)) {
      direction += transform.forward;
    } else if (input.GetKey(KeyCode.S)) {
      direction -= transform.forward;
    }

    if (input.GetKey(KeyCode.D)) {
      direction += transform.right;
    } else if (input.GetKey(KeyCode.A)) {
      direction -= transform.right;
    }

    if (input.GetKey(KeyCode.Q)) {
      direction += Vector3.up;
    } else if (input.GetKey(KeyCode.E)) {
      direction -= Vector3.down;
    }

    return direction.normalized;
  }

  private float GetFreeMoveSpeed(InputBridge input) {
    if (input.GetKey(KeyCode.LeftShift)) {
      return freeFastSpeed;
    } else {
      return freeSpeed;
    }
  }
}
