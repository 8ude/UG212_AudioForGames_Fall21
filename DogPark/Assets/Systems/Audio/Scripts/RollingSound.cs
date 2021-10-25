// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityAtoms.BaseAtoms;

// [RequireComponent(typeof(Rigidbody))]
// [RequireComponent(typeof(AudioSource))]
// public class RollingSound : MonoBehaviour
// {
// 	public AudioSource audioSource;

// 	public float maxVelocity = 2f;
// 	public float velocity;

//     public float volumeScale = 1f;

//     public float smoothTime = 1f;
//     private float _smoothVelocity;

//     [SerializeField] private float groundSpherecastLengthLocal = 0.51f;
//     [SerializeField] private LayerMask groundLayer;

//     // Nifty Unity thing that allows you to draw debugging gizmos in the editor
//     void OnDrawGizmos()
//     {
//         Gizmos.color = Color.magenta;
//         Gizmos.DrawRay(transform.position, Vector3.down * GroundSpherecastLength);
//     }

//     private float GroundSpherecastLength => groundSpherecastLengthLocal*transform.lossyScale.y;
//     private bool IsGrounded => Sphere.Raycast(transform.position, Vector3.down, GroundSpherecastLength, groundLayer);

//     // Start is called before the first frame update
//     void Start()
//     {
//         if (!audioSource) {
//         	audioSource = GetComponent<AudioSource>();
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         velocity = GetComponent<Rigidbody>().velocity.magnitude;

//         float targetVolume;
//         if (IsGrounded) {
//             targetVolume = volumeScale*velocity/maxVelocity;
//         } else {
//             targetVolume = 0f;
//         }
//         audioSource.volume = Mathf.SmoothDamp(audioSource.volume, targetVolume, ref _smoothVelocity, smoothTime);

//     }
// }
