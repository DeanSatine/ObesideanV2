using UnityEngine;

public interface IAbilityUser
{
    Rigidbody GetRigidbody();
    Animator GetAnimator();
    void SetAbilityState(bool active);
}
