using UnityEngine;

public class Rotateable : MonoBehaviour
{
    public float RotationSpeed = 20.0f;

    void Update()
    {
        transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime), Space.World);
    }
}
