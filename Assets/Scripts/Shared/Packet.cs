﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ServerPackets
{
    Welcome = 1,
    SpawnPlayer,
    PlayerState,
    Message
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    WelcomeReceived = 1,
    PlayerControls,
    Message,
    PlayerOrientation
}

public sealed class Packet : IDisposable
{
    private List<byte> _buffer;

    private bool _disposed;
    private byte[] _readableBuffer;
    private int _readPos;

    /// <summary>Creates a new empty packet (without an ID).</summary>
    public Packet()
    {
        _buffer = new List<byte>(); // Initialize buffer
        _readPos = 0; // Set readPos to 0
    }

    /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
    /// <param name="id">The packet ID.</param>
    public Packet(int id)
    {
        _buffer = new List<byte>(); // Initialize buffer
        _readPos = 0; // Set readPos to 0

        Write(id); // Write packet id to the buffer
    }

    /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public Packet(byte[] data)
    {
        _buffer = new List<byte>(data.Length); // Initialize buffer
        _readPos = 0; // Set readPos to 0

        SetBytes(data);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _buffer = null;
                _readableBuffer = null;
                _readPos = 0;
            }

            _disposed = true;
        }
    }

    #region Functions

    /// <summary>Sets the packet's content and prepares it to be read.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public void SetBytes(byte[] data)
    {
        Write(data);
        _readableBuffer = _buffer.ToArray();
    }

    /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
    public void WriteLength()
    {
        _buffer.InsertRange(0,
            BitConverter.GetBytes(_buffer.Count)); // Insert the byte length of the packet at the very beginning
    }

    /// <summary>Inserts the given int at the start of the buffer.</summary>
    /// <param name="value">The int to insert.</param>
    public void InsertInt(int value)
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the buffer
    }

    /// <summary>Gets the packet's content in array form.</summary>
    public byte[] ToArray()
    {
        _readableBuffer = _buffer.ToArray();
        return _readableBuffer;
    }

    /// <summary>Gets the length of the packet's content.</summary>
    public int Length => _buffer.Count;

    /// <summary>Gets the length of the unread data contained in the packet.</summary>
    public int UnreadLength => Length - _readPos;


    /// <summary>Resets the packet instance to allow it to be reused.</summary>
    /// <param name="shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            _buffer.Clear(); // Clear buffer
            _readableBuffer = null;
            _readPos = 0; // Reset readPos
        }
        else
        {
            _readPos -= 4; // "Unread" the last read int
        }
    }

    #endregion

    #region Write Data

    /// <summary>Adds a byte to the packet.</summary>
    /// <param name="value">The byte to add.</param>
    public void Write(byte value)
    {
        _buffer.Add(value);
    }

    /// <summary>Adds an array of bytes to the packet.</summary>
    /// <param name="value">The byte array to add.</param>
    public void Write(byte[] value)
    {
        _buffer.AddRange(value);
    }

    /// <summary>Adds a short to the packet.</summary>
    /// <param name="value">The short to add.</param>
    public void Write(short value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>Adds an int to the packet.</summary>
    /// <param name="value">The int to add.</param>
    public void Write(int value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>Adds a long to the packet.</summary>
    /// <param name="value">The long to add.</param>
    public void Write(long value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>Adds a float to the packet.</summary>
    /// <param name="value">The float to add.</param>
    public void Write(float value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="value">The bool to add.</param>
    public void Write(bool value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    /// <summary>Adds a string to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(string value)
    {
        Write(value.Length); // Add the length of the string to the packet
        _buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
    }

    /// <summary>Adds a Vector3 to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    /// <summary>Adds a Quaternion to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(Quaternion value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
        Write(value.w);
    }

    /// <summary>Adds a Vector2 to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(Vector2 value)
    {
        Write(value.x);
        Write(value.y);
    }

    #endregion

    #region Read Data

    /// <summary>Reads a byte from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte ReadByte(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes
            var value = _readableBuffer[_readPos]; // Get the byte at readPos' position
            if (moveReadPos)
                // If _moveReadPos is true
                _readPos += 1; // Increase readPos by 1
            return value; // Return the byte
        }

        throw new Exception("Could not read value of type 'byte'!");
    }

    /// <summary>Reads an array of bytes from the packet.</summary>
    /// <param name="length">The length of the byte array.</param>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte[] ReadBytes(int length, bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes
            var value =
                _buffer.GetRange(_readPos, length)
                    .ToArray(); // Get the bytes at readPos' position with a range of _length
            if (moveReadPos)
                // If _moveReadPos is true
                _readPos += length; // Increase readPos by _length
            return value; // Return the bytes
        }

        throw new Exception("Could not read value of type 'byte[]'!");
    }

    /// <summary>Reads a short from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public short ReadShort(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes
            var value = BitConverter.ToInt16(_readableBuffer, _readPos); // Convert the bytes to a short
            if (moveReadPos)
                // If _moveReadPos is true and there are unread bytes
                _readPos += 2; // Increase readPos by 2
            return value; // Return the short
        }

        throw new Exception("Could not read value of type 'short'!");
    }

    /// <summary>Reads an int from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public int ReadInt(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes
            var value = BitConverter.ToInt32(_readableBuffer, _readPos); // Convert the bytes to an int
            if (moveReadPos)
                // If _moveReadPos is true
                _readPos += 4; // Increase readPos by 4
            return value; // Return the int
        }

        throw new Exception("Could not read value of type 'int'!");
    }

    /// <summary>Reads a long from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public long ReadLong(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'long'!");
        // If there are unread bytes
        var value = BitConverter.ToInt64(_readableBuffer, _readPos); // Convert the bytes to a long
        if (moveReadPos)
            // If _moveReadPos is true
            _readPos += 8; // Increase readPos by 8
        return value; // Return the long
    }

    /// <summary>Reads a float from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public float ReadFloat(bool moveReadPos = true)
    {
        if (_buffer.Count <= _readPos) throw new Exception("Could not read value of type 'float'!");
        // If there are unread bytes
        var value = BitConverter.ToSingle(_readableBuffer, _readPos); // Convert the bytes to a float
        if (moveReadPos)
            // If _moveReadPos is true
            _readPos += 4; // Increase readPos by 4
        return value; // Return the float
    }

    /// <summary>Reads a bool from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public bool ReadBool(bool moveReadPos = true)
    {
        if (_buffer.Count > _readPos)
        {
            // If there are unread bytes
            var value = BitConverter.ToBoolean(_readableBuffer, _readPos); // Convert the bytes to a bool
            if (moveReadPos)
                // If _moveReadPos is true
                _readPos += 1; // Increase readPos by 1
            return value; // Return the bool
        }

        throw new Exception("Could not read value of type 'bool'!");
    }

    /// <summary>Reads a string from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            var length = ReadInt(); // Get the length of the string
            var value = Encoding.ASCII.GetString(_readableBuffer, _readPos, length); // Convert the bytes to a string
            if (moveReadPos && value.Length > 0)
                // If _moveReadPos is true string is not empty
                _readPos += length; // Increase readPos by the length of the string
            return value; // Return the string
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    /// <summary>Reads a Vector3 from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    /// <summary>Reads a Quaternion from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos),
            ReadFloat(moveReadPos));
    }

    /// <summary>Reads a Vector2 from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Vector2 ReadVector2(bool moveReadPos = true)
    {
        return new Vector2(ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    #endregion
}