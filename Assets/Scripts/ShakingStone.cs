using UnityEngine;

public class ShakingStone : MonoBehaviour
{
    public float shakeDuration = 3f;                // The duration of the shaking effect
    public float shakeMagnitude = 0.2f;             // The magnitude of the shaking effect
    public float fallSpeed = 2f;                    // The speed at which the stone falls down

    private Vector3 initialPosition;
    private float shakeTimer = 0f;

    public bool isShacking = false;
    private bool isFalling = false;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (shakeTimer > 0f)
        {
            // Shaking effect
            transform.position = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }
        else if (isFalling)
        {
            // Falling effect
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }

        if (transform.position.y < -8)
        {
            Destroy(gameObject);
        }
    }

    public void StartShaking()
    {
        // Start the shaking effect by setting the shakeTimer
        shakeTimer = shakeDuration;

        isShacking = true;
        isFalling = true;
    }
}