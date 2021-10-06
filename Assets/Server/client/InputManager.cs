using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Controls controls;
    private Controls newControls;
    private Orientation orientation;
    private Orientation newOrientation;

    public event Action<Vector2> HorizontalMovement;
    public event Action<float> YRotation;
    public event Action<float> CameraRotationX;
    public event Action<bool> Jump;
    public event Action<bool> Walk;
    public event Action<bool> Crouch;
    public event Action<uint> tick;

    public void RecieveControls(byte[] Buffer)
    {
        newControls.Deserialize(Buffer);
        if(controls.horizontalMovement != newControls.horizontalMovement)
        {
            controls.horizontalMovement = newControls.horizontalMovement;
            HorizontalMovement.Invoke(newControls.horizontalMovement);
        }
        if (controls.crouch != newControls.crouch)
        {
            controls.crouch = newControls.crouch;
            Crouch.Invoke(newControls.crouch);
        }
        if (controls.tick != newControls.tick)
        {
            controls.tick = newControls.tick;
            tick.Invoke(newControls.tick);
        }
        if (controls.walk != newControls.walk)
        {
            controls.walk = newControls.walk;
            Walk.Invoke(newControls.walk);
        }
        if (newControls.jump)
            Jump.Invoke(newControls.jump);
    }
    
    public void RecieveOrientation(byte[] buffer)
    {
        newOrientation.Deserialize(buffer);
        if(orientation.CameraRotationX != newOrientation.CameraRotationX)
        {
            orientation.CameraRotationX = newOrientation.CameraRotationX;
            CameraRotationX.Invoke(newOrientation.CameraRotationX);
        }
        if(orientation.CharacterRotationY != newOrientation.CharacterRotationY)
        {
            orientation.CharacterRotationY = newOrientation.CharacterRotationY;
            YRotation.Invoke(newOrientation.CharacterRotationY);
        }
    }
}
