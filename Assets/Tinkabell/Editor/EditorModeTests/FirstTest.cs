using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class FirstTest
{
    // For quadratic equation value of x where f(x) = x^2 - 4x + 4
    public Function function = new Function();

    [Test]
    public void T00_PassingTest () 
    {
        Assert.AreEqual (1, 1);
    }

    [Test]
    public void T01_X2Y0()
    {
        Assert.AreEqual(function.Value(2f) ,0f);
    }

    [Test]
    public void T02_X0Y4()
    {
        Assert.AreEqual(function.Value(0f) ,4f);
    }

    [Test]
    public void T03_X1Y1()
    {
        Assert.AreEqual(function.Value(1f) ,1f);
    }

    [Test]
    public void T04_Xn1Y9()
    {
        Assert.AreEqual(function.Value(-1f) ,9f);
    }

    [Test]
    public void T05_Xn2Y16()
    {
        Assert.AreEqual(function.Value(-2f) ,16f);
    }

    [Test]
    public void T06_X3Y1()
    {
        Assert.AreEqual(function.Value(3f) ,1f);
    }
}


