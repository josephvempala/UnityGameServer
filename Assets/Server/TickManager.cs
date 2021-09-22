using System;
using System.Collections.Generic;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    private static readonly List<Action> executeOnTick = new List<Action>();
    private static readonly List<Action> executeOnTickCopy = new List<Action>();
    private static bool actionToExecuteOnTick = false;

    private void FixedUpdate()
    {
        StartTick();
    }

    public static void ExecuteOnTick(Action _action)
    {
        if (_action == null)
        {
            Debug.Log("No action to execute on tick!");
            return;
        }

        lock (executeOnTick)
        {
            executeOnTick.Add(_action);
            actionToExecuteOnTick = true;
        }
    }

    private static void StartTick()
    {
        if (actionToExecuteOnTick)
        {
            executeOnTickCopy.Clear();
            lock (executeOnTick)
            {
                executeOnTickCopy.AddRange(executeOnTick);
                executeOnTick.Clear();
                actionToExecuteOnTick = false;
            }
            for (int i = 0; i < executeOnTickCopy.Count; i++)
            {
                executeOnTickCopy[i]();
            }
        }
    }
}