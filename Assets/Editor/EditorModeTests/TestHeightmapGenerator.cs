using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class TestHeightmapGenerator
{
    // [Test]
    public void FrameworkForCheck(){
        // Arrange

        // Act

        // Assert
    }

	public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // default MeshSettings
    //  MeshSetting.supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
    //  index 3 => 120
    //private static int chunkSize = 120
    //  index 0 => 120
    private static int chunkSize = 48;
    public static int defaultSize = chunkSize + 5; 

    public static Vector2 defaultSampleCentre = new Vector2(0f, 0f);
 
    public NoiseSettings minimalNoiseSettings;

    public HeightMapSettings minimalHeightMapSettings;

    [OneTimeSetUp]
    public void Init()
    {
        // minimalNoiseSettings
        minimalNoiseSettings = new NoiseSettings();
     	minimalNoiseSettings.octaves = 1;
    	minimalNoiseSettings.seed = 0;
    	minimalNoiseSettings.offset = new Vector2(0f, 0f);

        // minimalHeightMapSettings
        minimalHeightMapSettings = ScriptableObject.CreateInstance("HeightMapSettings") as HeightMapSettings;
        minimalHeightMapSettings.noiseSettings = minimalNoiseSettings;
        minimalHeightMapSettings.heightMultiplier = 50;
        minimalHeightMapSettings.heightCurve = heightCurve;

    }

    [Test]
    public void CheckAnimationCurve(){
        // Arrange

        int expectedKeys = 2;
        float epsilon = 0.1f;
        float startPoint = 0f;
        float startPlus = startPoint + epsilon;
        float middle = 0.5f;
        float endPoint = 1f;
        float endMinus = endPoint - epsilon;

        // Act

        int actualKeys = heightCurve.length;

        // Assert
        Assert.AreEqual(
            expectedKeys, actualKeys, 
            "Should have 2 keys (start and end of curve)");
        Assert.AreEqual(
            startPoint, heightCurve.Evaluate(startPoint), 
            "Start point and it's evaluation should match");
        Assert.AreEqual(
            endPoint, heightCurve.Evaluate(endPoint), 
            "End point and it's evaluation should match");
        Assert.AreEqual(
            middle, heightCurve.Evaluate(middle), 
            "Middle point and it's evaluation should match");
        Assert.Greater(
            startPlus, heightCurve.Evaluate(startPlus), 
            "Start point plus a bit should be bigger than it's evaluation");
        Assert.Less(
            endMinus, heightCurve.Evaluate(endMinus), 
            "End point minus a bit should be smaller than it's evaluation");
        Assert.Less(
            startPoint, heightCurve.Evaluate(startPlus), 
            "Start point should be smaller than the evaluation of start point plus a bit");
        Assert.Greater(
            endPoint, heightCurve.Evaluate(endMinus), 
            "End point should be bigger than the evaluation of end point minus a bit");

    }

    [Test]
    public void CheckGenerateHeightMapMinimal(){
        // Arrange

        // Act
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(
            defaultSize,
            minimalHeightMapSettings,
            defaultSampleCentre);

        float[,] actual = heightMap.values;

        // Assert
        float maxHeight = float.MinValue;
		float minHeight = float.MaxValue;
		for (int y = 0; y < defaultSize; y++) {
			for (int x = 0; x < defaultSize; x++) {
                float height = actual[x, y];
				if (height > maxHeight) {
					maxHeight = height;
				} 
				if (height < minHeight) {
					minHeight = height;
				}
            }
        }
        Assert.GreaterOrEqual(
            minHeight, -1f * minimalHeightMapSettings.heightMultiplier, 
            "Min Height {0} should be greater than minimal min height {1}", 
            minHeight, -1f * minimalHeightMapSettings.heightMultiplier);
        Assert.LessOrEqual(
            maxHeight, 1f * minimalHeightMapSettings.heightMultiplier, 
            "Max Height {0} should be less than minimal max height {1}", 
            maxHeight, 1f * minimalHeightMapSettings.heightMultiplier);
        Assert.Less(
            minHeight, 1f * minimalHeightMapSettings.heightMultiplier, 
            "Min Height {0} should be less than minimal max height {1}", 
            minHeight, 1f * minimalHeightMapSettings.heightMultiplier);
        Assert.Greater(
            maxHeight, -1f * minimalHeightMapSettings.heightMultiplier, 
            "Max Height {0} should be greater than minimal min height {1}", 
            maxHeight, -1f * minimalHeightMapSettings.heightMultiplier);
    }

}
