using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// displays an effect in the scene for a limited duration
// could include sprites, particles, etc
// used for blood splashes, explosions, bullet sparks

public class EffectTimed : MonoBehaviour
{
    [SerializeField]SpriteRenderer sprite;
    [SerializeField]ParticleSystem particle;
    [SerializeField]float duration;

    public void PlayEffect()
    {
        StartCoroutine(EffectTimer());
    }

    IEnumerator EffectTimer()
    {
        if (particle) particle.Play();
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
