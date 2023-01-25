using UnityEngine;

public class CrosshairTarget : MonoBehaviour
{
    Ray ray;
    RaycastHit hitInfo;
    Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        ray.origin = mainCamera.position;
        ray.direction = mainCamera.forward;

        if (Physics.Raycast(ray, out hitInfo))
        {
            transform.position = hitInfo.point;
        }
        else
        {
            transform.position = ray.origin + ray.direction * 1000.0f;
        }
    }
}
