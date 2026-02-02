using UnityEngine;

public class MovementTrail : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private float emissionThreshold = 5f;
    [SerializeField] private float dashEmissionMultiplier = 3f;

    private Rigidbody rb;
    private ParticleSystem.EmissionModule emission;
    private float baseEmissionRate = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (trailParticles != null)
        {
            emission = trailParticles.emission;
            baseEmissionRate = emission.rateOverTime.constant;
        }
    }

    private void Update()
    {
        if (rb == null || trailParticles == null) return;

        float speed = rb.linearVelocity.magnitude;
        
        if (speed > emissionThreshold)
        {
            float emissionRate = baseEmissionRate * (speed / emissionThreshold);
            emission.rateOverTime = emissionRate;
            
            if (!trailParticles.isPlaying)
            {
                trailParticles.Play();
            }
        }
        else
        {
            if (trailParticles.isPlaying)
            {
                trailParticles.Stop();
            }
        }
    }

    public void BurstDash()
    {
        if (trailParticles != null)
        {
            trailParticles.Emit((int)(baseEmissionRate * dashEmissionMultiplier));
        }
    }
}
