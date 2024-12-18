using UnityEngine;

public class OrbitElectron : MonoBehaviour
{
    public float orbitSpeed = 50f; // Speed of the electron orbit

    void Update()
    {
        // Rotate the electron around the nucleus
        transform.Rotate(Vector3.up, orbitSpeed * Time.deltaTime);
    }
}
