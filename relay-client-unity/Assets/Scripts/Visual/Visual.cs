using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Visual : MonoBehaviour
{
    Renderer[] renderers;
    public void UpdateMaterials()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public Action OnUpdate;

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    public void FadeIn()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                float t = m.GetFloat("_Alpha") - Time.deltaTime;
                m.SetFloat("_Alpha", t);

                if (t <= 0)
                {
                    OnUpdate -= FadeIn;
                }
            }
        }
    }

    public void FadeOut()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                float t = m.GetFloat("_Alpha") + Time.deltaTime;
                m.SetFloat("_Alpha", t);

                if (t >= 1)
                {
                    OnUpdate -= FadeOut;
                }
            }
        }
    }

    public void DamageIn ()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                float t = m.GetFloat("_Damage") + Time.deltaTime * 10;
                m.SetFloat("_Damage", t);

                if (t >= 1)
                {
                    OnUpdate -= DamageIn;
                    OnUpdate += DamageOut;
                }
            }
        }
    }

    public void DamageOut()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                float t = m.GetFloat("_Damage") - Time.deltaTime * 10;
                m.SetFloat("_Damage", t);

                if (t <= 0)
                {
                    OnUpdate -= DamageOut;
                }
            }
        }
    }
}
