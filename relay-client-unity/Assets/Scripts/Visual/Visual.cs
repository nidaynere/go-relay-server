using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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

    /// <summary>
    /// Use this before FadeIn()
    /// </summary>
    public void Born ()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetFloat("_Alpha", 1);
            }
        }
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
                Color c = m.GetColor("_Color");
                c = Color.Lerp(c, Color.red, Time.deltaTime* 10);
                m.SetColor("_Color", c);

                float t = m.GetFloat("_Damage") + Time.deltaTime * 5;
                m.SetFloat("_Damage", t);

                if (t >= 1)
                {
                    m.SetColor("_Color", Color.red);
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
                Color c = m.GetColor("_Color");
                c = Color.Lerp(c, Color.white, Time.deltaTime * 10);

                m.SetColor("_Color", c);

                float t = m.GetFloat("_Damage") - Time.deltaTime * 5;
                m.SetFloat("_Damage", t);

                if (t <= 0)
                {
                    m.SetColor("_Color", Color.white);
                    OnUpdate -= DamageOut;
                }
            }
        }
    }
}
