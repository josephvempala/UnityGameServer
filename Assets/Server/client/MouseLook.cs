using UnityEngine;

public class MouseLook : MonoBehaviour
{
    private InputManager controls;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraHolder;

    void Awake()
    {
        controls = GetComponent<InputManager>();
        controls.YRotation += ctx => { 
            var position = transform.localRotation.eulerAngles;
            position.y = ctx;
            transform.localRotation = Quaternion.Euler(position);
        };
        controls.CameraRotationX += ctx => { 
            var rotation = cameraHolder.localRotation.eulerAngles;
            rotation.x = ctx;
            cameraHolder.localRotation = Quaternion.Euler(rotation);
        };
    }
}
