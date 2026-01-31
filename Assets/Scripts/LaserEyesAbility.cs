using System.Collections;
using UnityEngine;

public class LaserEyesAbility : MonoBehaviour
{
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private float laserDuration = 4f;
    [SerializeField] private float laserRange = 50f;
    [SerializeField] private float coneAngle = 90f;
    [SerializeField] private float damagePerSecond = 500f;
    [SerializeField] private float sweepSpeed = 30f;
    [SerializeField] private LayerMask damageLayer;
    [SerializeField] private LineRenderer leftLaserLine;
    [SerializeField] private LineRenderer rightLaserLine;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        if (leftLaserLine != null) leftLaserLine.enabled = true;
        if (rightLaserLine != null) rightLaserLine.enabled = true;

        float elapsed = 0f;
        float sweepAngle = -coneAngle / 2f;

        while (elapsed < laserDuration)
        {
            elapsed += Time.deltaTime;
            sweepAngle += sweepSpeed * Time.deltaTime;

            if (sweepAngle > coneAngle / 2f)
                sweepAngle = -coneAngle / 2f;

            Vector3 sweepDirection = Quaternion.Euler(0, sweepAngle, 0) * transform.forward;

            FireLaser(leftEye, sweepDirection);
            FireLaser(rightEye, sweepDirection);

            yield return null;
        }

        if (leftLaserLine != null) leftLaserLine.enabled = false;
        if (rightLaserLine != null) rightLaserLine.enabled = false;

        controller.SetAbilityState(false);
    }

    private void FireLaser(Transform eye, Vector3 direction)
    {
        if (eye == null) return;

        RaycastHit hit;
        Vector3 endPoint = eye.position + direction * laserRange;

        if (Physics.Raycast(eye.position, direction, out hit, laserRange))
        {
            endPoint = hit.point;

            DestructibleBuilding building = hit.collider.GetComponent<DestructibleBuilding>();
            if (building != null)
            {
                building.ApplyDamage(direction * damagePerSecond * Time.deltaTime, hit.point);
            }

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * damagePerSecond * Time.deltaTime, ForceMode.Force);
            }
        }

        LineRenderer laser = (eye == leftEye) ? leftLaserLine : rightLaserLine;
        if (laser != null)
        {
            laser.SetPosition(0, eye.position);
            laser.SetPosition(1, endPoint);
        }
    }
}
