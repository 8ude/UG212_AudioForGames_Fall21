using System;
using System.Linq;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// An object of desire
public class DesireTarget: MonoBehaviour, GrabListener {
  // -- fields --
  [Header("Category Desirability")]
  [SerializeField]
  [Tooltip("The desire category.")]
  private Desire fDesire;

  [SerializeField]
  [Tooltip("The category's base desirability value.")]
  private FloatReference fBaseDesirability;

  [SerializeField]
  [Tooltip("The category's desirability multiplier for moving objects.")]
  private FloatReference fSpeedDesirability;

  [Header("Target Desirability")]
  [SerializeField]
  [Tooltip("The target's desirability multiplier relative to its category (e.g., 1.0f is neutral)")]
  private FloatReference fTargetDesirability;

  [SerializeField]
  [Tooltip("The target's desirability multiplier for grabbed objects.")]
  private FloatReference fGrabDesirability;

  [SerializeField]
  [Tooltip("The number of frames to buffer when smoothing speed.")]
  private IntReference fSpeedSmoothingFrames;

  [SerializeField]
  [Tooltip("The maximum speed when calculating desirability of moving objects.")]
  private FloatReference fSpeedSmoothingMax;

  [Header("Debug")]
  [SerializeField] private Color gizmoColor;
  [SerializeField] private float speed;

  // -- props --
  // buffer a number of previous speeds to smooth them out
  private float[] speedBuffer;
  private int speedBufferPosition;
  private Vector3 lastPosition;
  private bool mIsGrabbed;
  private IDisposable mDisposable;

  private void Start()
  {
    // initialize the lastPosition to the objects current position
    lastPosition = transform.position;

    // creates the speedBuffer with the given size
    mDisposable = fSpeedSmoothingFrames.Subscribe((i) => speedBuffer = new float[i]);
  }

  private void OnDestroy()
  {
    mDisposable?.Dispose();
  }

  private void OnDrawGizmos()
  {
    // Draw a line to show how Desireable an object is
    Gizmos.color = Color.magenta;
    Gizmos.DrawRay(transform.position, Vector3.up * CalcDesirability() * 2);
  }

  private void FixedUpdate()
  {
    Debug.Assert(speedBuffer.Length != 0, $"{name} has an empty speed buffer!");

    // Add current speed to current position
    speedBuffer[speedBufferPosition] = Mathf.Min((transform.position - lastPosition).magnitude / Time.deltaTime, fSpeedSmoothingMax.Value);

    // Increase speedBufferIndex by one (constraining it to SpeedBufferSize)
    speedBufferPosition = (speedBufferPosition + 1) % fSpeedSmoothingFrames.Value;

    // Save current position for next frame
    lastPosition = transform.position;

    // calculate the smoothed speed
    speed = (speedBuffer?.Sum() ?? 0) / fSpeedSmoothingFrames.Value;
  }

  // -- queries --
  /// This target's category of desire.
  public Desire Desire => fDesire;

  public float CalcDesirability() {
    // calculate base desirability
    var d = fBaseDesirability + fSpeedDesirability * speed * speed;

    // apply local multiplier
    d *= fTargetDesirability;

    // apply grab multiplier if grabbed
    if (mIsGrabbed) {
      d *= fGrabDesirability;
    }

    return d;
  }

  // -- GrabListener --
  public void OnTargetted() {}
  public void OnUntargetted() {}

  public void OnGrabbed() {
    // TODO: maybe grabbable is passed an id for the player that grabbed, pets are extra excited
    // by objects their player grabs.
    mIsGrabbed = true;
  }

  public void OnReleased() {
    mIsGrabbed = false;
  }
}
