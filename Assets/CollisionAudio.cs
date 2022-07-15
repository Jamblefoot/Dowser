using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudio : MonoBehaviour
{
    public AudioSource audioSource;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude > 2)
            audioSource.PlayOneShot(GameControl.instance.GetBounce());
    }

    public void PlayHit()
    {
        audioSource.PlayOneShot(GameControl.instance.GetBounce());
    }
}
