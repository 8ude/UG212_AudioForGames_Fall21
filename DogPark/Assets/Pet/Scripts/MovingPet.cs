using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class MovingPet : NetworkBehaviour
{
    [Header("Tunables")]
    [Tooltip("The base multiplier for the pets moving force")]
    [SerializeField] private FloatReference baseMoveForce;

    [Tooltip("The multiplier for the pet's idle movement force")]
    [SerializeField] private FloatReference idleMoveForce;

    [Tooltip("Max dog maxSpeed")]
    [SerializeField] private FloatReference maxSpeed;

    [Tooltip("The radius of the smell/sound sphere")]
    [FormerlySerializedAs("InterestRadius")]
    [SerializeField] private FloatReference interestRadius;

    [Tooltip("Radius where the pet is satisfied to be close enough to its interest")]
    [SerializeField] private FloatReference closeToInsterestRadius;

    [Tooltip("The radius of the smell/sound sphere")]
    [FormerlySerializedAs("NewObjectInterestMultiplier")]
    [SerializeField] private FloatReference initialObjectDesirabilityMultiplier;

    [Tooltip("The time it takes for the desirabilityMultiplier to be half it's current value")]
    [SerializeField] private FloatReference desirabilityMultiplierHalfTime;

    [Tooltip("Total Interest Mass")]
    [SerializeField] private FloatReference totalInterestMass;

    [Tooltip("How much the interest balances per frame")]
    [SerializeField] private FloatReference interestBalanceFactor;

    [Tooltip("The direction the pet idly moves toward")]
    [SerializeField] private Vector3Reference trendingDirection;


    [FormerlySerializedAs("_currentObjectOfDesire")]
    [Header("Debug")]
    [SerializeField] private DesireTarget currentDesireTarget;


    [Header("FMOD")]
    [Tooltip("optional - continuous movement sound with velocity params")]
    public FMODUnity.StudioEventEmitter fmodMovementEmitter;
    public string velocityParam;
    public string yPosParam;

    [Tooltip("dog sounds with interest param")]
    public FMODUnity.StudioEventEmitter fmodDogSoundsEmitter;
    public string petInterestParam;
    public float maxDesireability = 1f;

    private DesireTarget CurrentDesireTarget
    {
        get
        {
            return currentDesireTarget;
        }
        set
        {
            // Set the last noticed time when a new object is set
            if (value != currentDesireTarget)
            {
                // When pet changes target, decrease the categorical preference for the previous OoD
                if (currentDesireTarget != null && currentDesireTarget.Desire != value.Desire)
                {
                    mPreferences.ScaleBy(currentDesireTarget.Desire, interestBalanceFactor.Value);
                }
                timeLastObjectNoticed = Time.time;
            }
            currentDesireTarget = value;
        }
    }
    [SerializeField] private float currentTargetDesirabilityDebug;

    private float timeLastObjectNoticed;
    private float timeSinceLastObjectNoticed => Time.time - timeLastObjectNoticed;

    // the new keyword here is because unity already has a rigidbody field in the class
    // but it's being deprecated
    private new Rigidbody rigidbody;

    // this pet's randomized desires
    private PetPreferences mPreferences;

    // this value hold the currentInterestMultiplier for the object
    // how much the pet is itself interested in the current object that fades over time
    // It is currently a half time
    // TODO: this could be a thing that is per object basis, making previous objects less interesting as well
    // sort of like a memory for all the objects the dog recently desired
    private float desirabilityMultiplier =>
      initialObjectDesirabilityMultiplier * Mathf.Pow(0.5f, timeSinceLastObjectNoticed / desirabilityMultiplierHalfTime.Value);

    // the desirability of the current object, negative if no object desired
    private float currentTargetDesirability =>
      CalcDesirability(CurrentDesireTarget) * desirabilityMultiplier;

    // Gets the direction of the current object of interest, projected to the XZ plane (Vector3.up)
    private Vector3 interestDirection => interestVector.normalized;
    private float interestDistance => interestVector.magnitude;

    private Vector3 interestVector => CurrentDesireTarget == null
      ? Vector3.zero
      : Vector3.ProjectOnPlane(CurrentDesireTarget.transform.position - transform.position, Vector3.up);

    // Calculate force multiplier based on desirability
    private float desirabilityForceMultiplier => baseMoveForce * currentTargetDesirability;

    private Vector3 currentMoveForce =>
        ((Vector3.ProjectOnPlane(trendingDirection?.Value ?? transform.forward, Vector3.up).normalized) * idleMoveForce.Value
        + interestDirection * desirabilityForceMultiplier) * Mathf.InverseLerp(0, closeToInsterestRadius.Value, interestDistance);

    // calculates the idle direction the pet will move towards

    // -- lifecycle --
    // Start is called before the first frame update
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        mPreferences = new PetPreferences();
    }

    private void OnDrawGizmos()
    {
        if (CurrentDesireTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.up * currentTargetDesirability);
            Gizmos.DrawLine(transform.position, CurrentDesireTarget.transform.position);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, 10 * currentMoveForce);


            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, Vector3.up * currentTargetDesirability * 2);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draws the view pyramid of the pet's interest
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interestRadius.Value);
        Gizmos.DrawWireSphere(transform.position, closeToInsterestRadius.Value);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // TODO: maybe cute to have the pet spin around if sitting on top of its current target,
        // sort of happens organically now but not consistent
        // Add the movement force in the direction of the current object of desire

        // how fast are you already moving in the direction of input?
        var moveForce = currentMoveForce;
        float directionalSpeed = Vector3.Dot(rigidbody.velocity, moveForce.normalized);

        if (directionalSpeed < maxSpeed.Value)
        {
            rigidbody.AddForce(moveForce, ForceMode.Force);
        }

        var t = transform;
        var mostDesirableTargetNearby = Physics
          // Check for interesting objects nearby using a spherecast
          .SphereCastAll(t.position, interestRadius.Value, t.forward, 0.01f)
          // Get only objects that have the ObjectOfDesire component
          .SelectNonNull(FindDesireTarget)
          // Get the most desirable target
          .MaxBy(CalcDesirability);

        // If the current objects desirability is less the the most desirable object nearby
        if (mostDesirableTargetNearby && currentTargetDesirability < CalcDesirability(mostDesirableTargetNearby))
        {
            // Change desirability of the old thing
            CurrentDesireTarget = mostDesirableTargetNearby;
        }
        currentTargetDesirabilityDebug = currentTargetDesirability;

        fmodMovementEmitter.SetParameter(velocityParam, directionalSpeed / 9f);
        fmodMovementEmitter.SetParameter(yPosParam, transform.position.y - 10.5f);


        fmodDogSoundsEmitter.SetParameter(petInterestParam, Mathf.InverseLerp(0, maxDesireability, currentTargetDesirability));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        var player = GetComponent<Owner>();
        if (player != null && !player.IsLocalPlayer)
        {
            enabled = false;
        }
    }

    // -- queries --
    private float CalcDesirability(DesireTarget target)
    {
        // a non-target is not desirable
        if (!target)
        {
            return 0.0f;
        }

        // get inherent desirability of target
        var d = target.CalcDesirability();
        // scale by this pet's categorical preference
        d *= mPreferences.GetScale(target.Desire) * totalInterestMass.Value;

        return d;
    }

    private DesireTarget FindDesireTarget(RaycastHit hit)
    {
        var target = hit.transform.GetComponent<DesireTarget>();

        // ignore raycasts that don't hit a desire target
        if (target == null)
        {
            return null;
        }

        // ignore raycasts that hit this pet
        if (target.gameObject == gameObject)
        {
            return null;
        }

        return target;
    }
}
