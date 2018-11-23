using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIVariables : AgentVariables
{
    public float _HitDistance = 1;
    public float _AvoidDistance = 5;

    public override float HitDistance()
    {
        return _HitDistance;
    }

    public override float AvoidDistance()
    {
        return _AvoidDistance;
    }
}