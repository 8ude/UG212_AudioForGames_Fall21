using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetTheDog : MonoBehaviour, GrabListener
{
    public MovingPet pet;
    private bool defaultState;
    private void Start() {
        defaultState = pet.enabled;
    }
    public void OnGrabbed()
    {
    }

    public void OnReleased()
    {
    }

    public void OnTargetted()
    {
        pet.enabled = false;
    }

    public void OnUntargetted()
    {
        pet.enabled = defaultState;
    }
}
