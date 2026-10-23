using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageInfo", menuName = "ScriptableObjects/StageInfo")]
public class StageInfo : ScriptableObject
{
    public PinsInfo pinsInfo;
    public PassingLine passingLine;
    public float lanePositionZ;
}

[System.Serializable]
public class PassingLine
{
    [Range(0, 10)] public int pinsCount;
}

[System.Serializable]
public class PinsInfo
{
    public enum Status
    {
        None,
        Standing,
    }
    public Status[] status;

    public int PinsCount()
    {
        var ret = 0;
        foreach (var pin in this.status)
        {
            if (pin != Status.None) ret++;
        }
        return ret;
    }

    public int StandingPinsCount()
    {
        var ret = 0;
        foreach (var pin in this.status)
        {
            if (pin == Status.Standing) ret++;
        }
        return ret;
    }

    public int KnockedOutPinsCount()
    {
        var ret = 0;
        foreach (var pin in this.status)
        {
            if (pin == Status.None) ret++;
        }
        return ret;
    }
}