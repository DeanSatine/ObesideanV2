using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private DashSlashAbility dashSlash;
    private JumpSlamAbility jumpSlam;
    private SubwayBeamAbility subwayBeam;
    private ChompAbility chomp;
    private RollAbility roll;
    private AudioEvents audioEvents;
    private bool isBoss;

    private void Awake()
    {
        dashSlash = GetComponent<DashSlashAbility>();
        jumpSlam = GetComponent<JumpSlamAbility>();
        subwayBeam = GetComponent<SubwayBeamAbility>();
        chomp = GetComponent<ChompAbility>();
        roll = GetComponent<RollAbility>();
        
        BossController bossController = GetComponent<BossController>();
        isBoss = bossController != null;
        
        if (isBoss && bossController != null)
        {
            audioEvents = bossController.GetComponent<BossController>()?.GetAudioEvents();
        }
        else
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                audioEvents = playerController.GetAudioEvents();
            }
        }
    }

    public void OnDashSlashHit()
    {
        if (dashSlash != null)
            dashSlash.TriggerDamage();
    }

    public void OnJumpSlamHit()
    {
        if (jumpSlam != null)
            jumpSlam.TriggerShockwave();
    }

    public void OnChompBite()
    {
        if (chomp != null)
            chomp.OnChompAnimationEvent();
    }

    public void OnRollDamage()
    {
        if (roll != null)
            roll.TriggerDamage();
    }

    public void OnFootstep()
    {
        if (audioEvents != null)
        {
            audioEvents.PlayFootstep(transform.position, isBoss);
        }
    }
}
