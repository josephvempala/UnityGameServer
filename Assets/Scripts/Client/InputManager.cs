using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Controls _controls;
    private Controls _newControls;
    private Orientation _newOrientation;
    private Orientation _orientation;

    public event Action<Vector2> HorizontalMovement;
    public event Action<float> YRotation;
    public event Action<float> CameraRotationX;
    public event Action<bool> Jump;
    public event Action<bool> Walk;
    public event Action<bool> Crouch;
    public event Action<uint> Tick;

    public void ReceiveControls(byte[] buffer)
    {
        _newControls.Deserialize(buffer);
        if (_controls.HorizontalMovement != _newControls.HorizontalMovement)
        {
            _controls.HorizontalMovement = _newControls.HorizontalMovement;
            HorizontalMovement?.Invoke(_newControls.HorizontalMovement);
        }

        if (_controls.Crouch != _newControls.Crouch)
        {
            _controls.Crouch = _newControls.Crouch;
            Crouch?.Invoke(_newControls.Crouch);
        }

        if (_controls.Tick != _newControls.Tick)
        {
            _controls.Tick = _newControls.Tick;
            Tick?.Invoke(_newControls.Tick);
        }

        if (_controls.Walk != _newControls.Walk)
        {
            _controls.Walk = _newControls.Walk;
            Walk?.Invoke(_newControls.Walk);
        }

        if (_newControls.Jump)
            Jump?.Invoke(_newControls.Jump);
    }

    public void ReceiveOrientation(byte[] buffer)
    {
        _newOrientation.Deserialize(buffer);
        if (_orientation.CameraRotationX != _newOrientation.CameraRotationX)
        {
            _orientation.CameraRotationX = _newOrientation.CameraRotationX;
            CameraRotationX?.Invoke(_newOrientation.CameraRotationX);
        }

        if (_orientation.CharacterRotationY != _newOrientation.CharacterRotationY)
        {
            _orientation.CharacterRotationY = _newOrientation.CharacterRotationY;
            YRotation?.Invoke(_newOrientation.CharacterRotationY);
        }
    }
}