using UnityEngine;

public static class GameFeel
{
    public static void OnPlayerHit(Vector3 position, float damage)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakePlayerHit();
        }

        if (TimeManager.Instance != null && damage > 50f)
        {
            TimeManager.Instance.FreezeFrame(0.05f);
        }
    }

    public static void OnEnemyKilled(Vector3 position)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.15f, 0.1f);
        }

        if (ComboSystem.Instance != null)
        {
            ComboSystem.Instance.AddKill();
        }

        if (HitMarker.Instance != null)
        {
            HitMarker.Instance.ShowKill();
        }
    }

    public static void OnBossDamaged(Vector3 position, float damage)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeBossHit();
        }

        if (HitMarker.Instance != null)
        {
            HitMarker.Instance.ShowHit(damage > 100f);
        }
    }

    public static void OnBossKilled(Vector3 position)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeBossDeath();
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SlowMotion(2f, 0.1f);
        }
    }

    public static void OnBossAbility(Vector3 position, string abilityName)
    {
        if (CameraShake.Instance != null)
        {
            switch (abilityName)
            {
                case "Roll":
                    CameraShake.Instance.ShakeRoll();
                    break;
                case "JumpSlam":
                    CameraShake.Instance.ShakeJumpSlam();
                    break;
                case "DashSlash":
                    CameraShake.Instance.ShakeDashSlash();
                    break;
                case "Chomp":
                    CameraShake.Instance.ShakeChomp();
                    break;
                case "SubwayBeam":
                    CameraShake.Instance.ShakeBeam();
                    break;
                default:
                    CameraShake.Instance.ShakeBossAbility();
                    break;
            }
        }
    }

    public static void OnBuildingDestroyed(Vector3 position)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.4f, 0.2f);
        }
    }

    public static void OnExplosion(Vector3 position, float force)
    {
        float shakeIntensity = Mathf.Clamp(force / 1000f, 0.2f, 0.8f);
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(shakeIntensity, 0.3f);
        }
    }
}
