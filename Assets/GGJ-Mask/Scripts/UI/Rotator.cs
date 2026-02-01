using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float rotationSpeed = 50.0f;
    private bool isRotating = true; // Keep it private

    void Update()
    {
        if (isRotating)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    public void StopRotation()
    {
        isRotating = false;
    }

    public void SetIsRotating(bool rotating)
    {
        isRotating = rotating;
    }
}

