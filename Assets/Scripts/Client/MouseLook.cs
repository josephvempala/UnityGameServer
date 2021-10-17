using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private Transform cameraHolder;

    private InputManager _controls;

    private void Awake()
    {
        _controls = GetComponent<InputManager>();
        _controls.YRotation += ctx =>
        {
            var position = transform.localRotation.eulerAngles;
            position.y = ctx;
            transform.localRotation = Quaternion.Euler(position);
        };
        _controls.CameraRotationX += ctx =>
        {
            var rotation = cameraHolder.localRotation.eulerAngles;
            rotation.x = ctx;
            cameraHolder.localRotation = Quaternion.Euler(rotation);
        };
    }
}