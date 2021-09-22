using System;
using UnityEngine;

public class InputManager
{
    private Controls controls;
    private Controls newControls;

    public event Action<Vector2> HorizontalMovement;
    public event Action<float> YRotation;
    public event Action<bool> Jump;
    public event Action<bool> Walk;
    public event Action<bool> Crouch;

    public void RecieveControls(byte[] Buffer)
    {
        newControls.Deserialize(Buffer);
        if(controls.horizontalMovement != newControls.horizontalMovement)
        {
            controls.horizontalMovement = newControls.horizontalMovement;
            HorizontalMovement.Invoke(newControls.horizontalMovement);
        }
        if(controls.Yrotation != newControls.Yrotation)
        {
            controls.Yrotation = newControls.Yrotation;
            YRotation.Invoke(newControls.Yrotation);
        }
        if (controls.crouch != newControls.crouch)
        {
            controls.crouch = newControls.crouch;
            Crouch.Invoke(newControls.crouch);
        }
        if (controls.walk != newControls.walk)
        {
            controls.walk = newControls.walk;
            Walk.Invoke(newControls.walk);
        }
        if (newControls.jump)
            Jump.Invoke(newControls.jump);
    }
    
}
