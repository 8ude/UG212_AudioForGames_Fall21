using UnityEngine;

public class RopeHandle: MonoBehaviour {
    // -- constants --
    private const float kHoverTolerance = 20.0f;

    // -- props --
    private Camera mCamera;
    private Vector3 mScreenPos;
    private bool mIsMoving;

    // -- lifecycle --
    protected void Awake() {
        mCamera = Camera.main;
    }

    protected void Update() {
        // if we click on the handle
        if (Input.GetMouseButtonDown(0) && IsHovering()) {
            StartMove();
        }
        // if we move the mouse while clicking the handle
        else if (Input.GetMouseButton(0) && mIsMoving) {
            Move();
        }
        // if we release the mouse after moving the handle
        else if (Input.GetMouseButtonUp(0) && mIsMoving) {
            FinishMove();
        }
    }

    // -- commands/gesture
    private void StartMove() {
        mIsMoving = true;
    }

    private void Move() {
        // calculate mouse position in world space
        var pos = Input.mousePosition;
        pos.z = mScreenPos.z;
        pos = mCamera.ScreenToWorldPoint(pos);

        // update position
        transform.position = pos;
    }

    private void FinishMove() {
        mIsMoving = false;
    }

    // -- queries --
    // check if the mouse is hovering the handle
    private bool IsHovering() {
        mScreenPos = mCamera.WorldToScreenPoint(transform.position);
        return Vector2.Distance(Input.mousePosition, mScreenPos) <= kHoverTolerance;
    }
}
