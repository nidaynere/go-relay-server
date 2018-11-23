using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentVariables : Variables
{
    public int _Health = 100;
    public float _MoveSpeed = 1;
    public int _Damage = 1;

    public override float MoveSpeed ()
    {
        return _MoveSpeed;
    }

    public override int Damage ()
    {
        return _Damage;
    }

    public override int Health ()
    {
        return _Health;
    }
}
