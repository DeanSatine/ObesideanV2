using System.Collections;
using UnityEngine;

public class SubwayBeamAbility : MonoBehaviour
{
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private float beamDuration = 4f;
    [SerializeField] private float beamRange = 50f;
    [SerializeField] private float coneAngle = 90f;
    [SerializeField] private float damagePerSecond = 500f;
    [SerializeField] private float sweepSpeed = 30f;
    [SerializeField] private LineRenderer leftBeamLine;
    [SerializeField] private LineRenderer rightBeamLine;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        if (leftBeamLine != null) leftBeamLine.enabled = true;
        if (rightBeamLine != null) rightBeamLine.enabled = true;

        float elapsed = 0f;
        float sweepAngle = -coneAngle / 2f;

        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            sweepAngle += sweepSpeed * Time.deltaTime;

            if (sweepAngle > coneAngle / 2f)
                sweepAngle = -coneAngle / 2f;

            Vector3 sweepDirection = Quaternion.Euler(0, sweepAngle, 0) * transform.forward;

            FireBeam(leftEye, sweepDirection);
            FireBeam(rightEye, sweepDirection);

            yield return null;
        }

        if (leftBeamLine != null) leftBeamLine.enabled = false;
        if (rightBeamLine != null) rightBeamLine.enabled = false;

        controller.SetAbilityState(false);
    }

    private void FireBeam(Transform eye, Vector3 direction)
    {
        if (eye == null) return;

        RaycastHit hit;
        Vector3 endPoint = eye.position + direction * beamRange;

        if (Physics.Raycast(eye.position, direction, out hit, beamRange))
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

        LineRenderer beam = (eye == leftEye) ? leftBeamLine : rightBeamLine;
        if (beam != null)
        {
            beam.SetPosition(0, eye.position);
            beam.SetPosition(1, endPoint);
        }
    }
}
