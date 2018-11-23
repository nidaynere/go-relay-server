using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General variables of networked objects.
/// </summary>
public class Variables : MonoBehaviour
{
    public virtual int Health () { return 0; }
    public virtual int Damage () { return 0; }
    public virtual float AvoidDistance () { return 0; }
    public virtual float MoveSpeed() { return 0; }
    public virtual float HitDistance() { return 0; }
}
