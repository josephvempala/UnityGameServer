using System;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    private static readonly List<Action> ToExecuteOnTick = new List<Action>();
    private static readonly List<Action> ToExecuteOnTickCopy = new List<Action>();
    private static bool _actionToExecuteOnTickPresent;

    public void Update()
    {
        StartTick();
    }

    public static void ExecuteOnTick(Action action)
    {
        if (action == null)
        {
            Debug.Log("No action to execute on tick!");
            return;
        }

        lock (ToExecuteOnTick)
        {
            ToExecuteOnTick.Add(action);
            _actionToExecuteOnTickPresent = true;
        }
    }

    private static void StartTick()
    {
        if (!_actionToExecuteOnTickPresent) return;
        ToExecuteOnTickCopy.Clear();
        lock (ToExecuteOnTick)
        {
            ToExecuteOnTickCopy.AddRange(ToExecuteOnTick);
            ToExecuteOnTick.Clear();
            _actionToExecuteOnTickPresent = false;
        }

        foreach (var t in ToExecuteOnTickCopy)
            t();
    }
}