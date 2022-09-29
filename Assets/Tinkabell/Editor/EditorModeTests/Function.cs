using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function 
{
    // For quadratic equation value of x where f(x) = x^2 - 4x + 4
    public float Value (float x)
    {
        return (Mathf.Pow (x,2) - (4f*x) + 4f);
    }
} 

