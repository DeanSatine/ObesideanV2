using System.Collections;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("Ragdoll Settings")]
    [SerializeField] private float ragdollDuration = 0.5f;
    [SerializeField] private float damageThresholdForRagdoll = 10f;
    [SerializeField] private bool enableRagdollOnDamage = true;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody mainRigidbody;
    
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private bool isRagdolling;
    private Coroutine ragdollCoroutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (mainRigidbody == null)
            mainRigidbody = GetComponent<Rigidbody>();

        CollectRagdollParts();
        DisableRagdoll();
    }

    private void CollectRagdollParts()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
    }

    public void TriggerRagdoll(float damageAmount, Vector3 force = default)
    {
        if (!enableRagdollOnDamage) return;
        if (damageAmount < damageThresholdForRagdoll) return;
        
        if (ragdollCoroutine != null)
        {
            StopCoroutine(ragdollCoroutine);
        }
        
        ragdollCoroutine = StartCoroutine(RagdollRoutine(force));
    }

    private IEnumerator RagdollRoutine(Vector3 force)
    {
        EnableRagdoll();
        
        if (force != Vector3.zero)
        {
            ApplyForceToRagdoll(force);
        }
        
        yield return new WaitForSeconds(ragdollDuration);
        
        DisableRagdoll();
        ragdollCoroutine = null;
    }

    private void EnableRagdoll()
    {
        isRagdolling = true;
        
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = true;
        }
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody && rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col.GetComponent<Rigidbody>() != mainRigidbody)
            {
                col.enabled = true;
            }
        }
    }

    public void DisableRagdoll()
    {
        isRagdolling = false;
        
        if (animator != null)
        {
            animator.enabled = true;
        }
        
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = false;
        }
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody && rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
            }
        }
        
        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col.GetComponent<Rigidbody>() != mainRigidbody)
            {
                col.enabled = false;
            }
        }
    }

    public void EnablePermanentRagdoll(Vector3 force = default)
    {
        isRagdolling = true;
        
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = true;
        }
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody && rb != null)
            {
                rb.isKinematic = false;
            }
        }
        
        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col.GetComponent<Rigidbody>() != mainRigidbody)
            {
                col.enabled = true;
            }
        }
        
        if (force != Vector3.zero)
        {
            ApplyForceToRagdoll(force);
        }
    }

    private void ApplyForceToRagdoll(Vector3 force)
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != mainRigidbody && rb != null && !rb.isKinematic)
            {
                rb.AddForce(force, ForceMode.VelocityChange);
            }
        }
    }

    public bool IsRagdolling()
    {
        return isRagdolling;
    }
}
