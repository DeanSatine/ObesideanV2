using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private Vector3 randomOffset = new Vector3(0.5f, 0.5f, 0.5f);

    private Text text;
    private float timer;
    private Vector3 velocity;
    private Camera mainCamera;

    private void Start()
    {
        text = GetComponent<Text>();
        mainCamera = Camera.main;
        
        velocity = new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            moveSpeed + Random.Range(0, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        transform.position += velocity * Time.deltaTime;
        velocity.y -= 9.8f * Time.deltaTime * 0.5f;

        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        if (text != null)
        {
            Color color = text.color;
            color.a = 1f - (timer / lifetime);
            text.color = color;
        }
    }

    public void SetDamage(float damage, bool isCritical = false)
    {
        if (text != null)
        {
            text.text = Mathf.CeilToInt(damage).ToString();
            text.color = isCritical ? Color.yellow : Color.white;
            text.fontSize = isCritical ? 24 : 18;
        }
    }
}
