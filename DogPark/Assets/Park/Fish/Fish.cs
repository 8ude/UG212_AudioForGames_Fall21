using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutCommon;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Rigidbody))]
public class Fish : MonoBehaviour
{
    [Header("Breathing")]
    [SerializeField] private FloatReference totalBreathingTime;
    [SerializeField] private LayerMask water;
    [Header("Flopping")]
    [SerializeField] private FloatReference flopInterval;
    [SerializeField] private FloatReference flopForceUp;
    [SerializeField] private FloatReference flopForceBackToWater;
    [Header("Animation")]
    [SerializeField] private FloatReference witheringDuration;
    [SerializeField] private AnimationCurve witheringCurve;
    [Header("Event Callbacks")]
    [SerializeField] private UnityEvent onDie;


    [Header("debug")]
    private Rigidbody rb;
    private Vector3 lastWaterPosition;
    [SerializeField] private float currentBreathingTime;
    private bool isUnderWater;
    private bool isDead = false;

    private float flopTimer;

    private void Awake() {
        currentBreathingTime = totalBreathingTime.Value;
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        if(isDead) return;
        if(isUnderWater) {
            flopTimer = 0;
        } else {
            // flop
            flopTimer += Time.deltaTime;
            if(flopTimer > flopInterval.Value) {
                var flopForce = Vector3.up * flopForceUp.Value + (lastWaterPosition - transform.position).normalized * flopForceBackToWater;
                rb.AddForce(flopForce, ForceMode.VelocityChange);
                flopTimer = 0.0f;
            }
            currentBreathingTime -= Time.deltaTime;
            if(currentBreathingTime < 0) Die();
        }
    }

    private void Die() {
        isDead = true;
        onDie?.Invoke();
        var initialScale = transform.localScale;
        StartCoroutine(CoroutineHelpers.InterpolateByTime(
            witheringDuration.Value,
            (k) => {
                transform.localScale = initialScale * witheringCurve.Evaluate(k);
            },
            () => {
                transform.localScale = Vector3.zero;
                Destroy(this.gameObject);
            }));
    }


    private void OnTriggerStay(Collider other) {
        if(water.Contains(other.gameObject.layer)) {
            isUnderWater = true;
            currentBreathingTime = totalBreathingTime.Value;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(water.Contains(other.gameObject.layer)) {
            isUnderWater = false;
            lastWaterPosition = transform.position;
        }
    }



}
