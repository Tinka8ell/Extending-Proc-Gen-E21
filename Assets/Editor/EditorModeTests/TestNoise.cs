using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class TestNoise : MonoBehaviour
{
    // [Test]
    public void FrameworkForCheck(){
        // Arrange

        // Act

        // Assert
    }

    // default MeshSettings
    //  MeshSetting.supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
    //  index 3 => 120
    //private static int chunkSize = 120
    //  index 0 => 120
    private static int chunkSize = 48;
    public static int defaultSize = chunkSize + 5; 

    public static Vector2 defaultSampleCentre = new Vector2(0f, 0f);
 
    public NoiseSettings minimalNoiseSettings;
    public NoiseSettings textureNoiseSettings;
    public NoiseSettings islandNoiseSettings;
    public NoiseSettings deepIslandNoiseSettings;

    [OneTimeSetUp]
    public void Init()
    {
        // minimalNoiseSettings
        minimalNoiseSettings = new NoiseSettings();
     	minimalNoiseSettings.octaves = 1;
    	minimalNoiseSettings.seed = 0;
    	minimalNoiseSettings.offset = new Vector2(0f, 0f);

        // textureNoiseSettings;
        textureNoiseSettings = new NoiseSettings();
     	// leave textureNoiseSettings.octaves at 6
    	textureNoiseSettings.seed = 0;
    	textureNoiseSettings.offset = new Vector2(0f, 0f);

        // islandNoiseSettings;
        islandNoiseSettings = new NoiseSettings();
     	// leave textureNoiseSettings.octaves at 6
    	islandNoiseSettings.scale = 2000;
    	islandNoiseSettings.seed = 0;
    	islandNoiseSettings.offset = new Vector2(0f, 0f);

        // deepIslandNoiseSettings;
        deepIslandNoiseSettings = new NoiseSettings();
     	// leave textureNoiseSettings.octaves at 6
    	deepIslandNoiseSettings.scale = 2000;
    	deepIslandNoiseSettings.seed = 0;
    	deepIslandNoiseSettings.offset = new Vector2(0f, 0f);
        deepIslandNoiseSettings.seaGradient = 2f;

    }

    [Test]
    public void CheckGenerateNoiseMapMinimal(){
        // Arrange

        // Act

        // Assert
        CheckGenerateNoiseMap(minimalNoiseSettings, -1, 1);
    }

    [Test]
    public void CheckGenerateNoiseMapTexture(){
        // Arrange

        // Act

        // Assert
        CheckGenerateNoiseMap(textureNoiseSettings, -1, 1);
    }

    [Test]
    public void CheckGenerateNoiseMapIsland(){
        // Arrange

        // Act

        // Assert
        CheckGenerateNoiseMap(islandNoiseSettings, -1, 1);
    }

    [Test]
    public void CheckGenerateNoiseMapIslandPlus(){
        // Arrange

        // Act

        // Assert
        CheckGenerateNoiseMap(deepIslandNoiseSettings, -deepIslandNoiseSettings.seaGradient, 1);
    }

    public void CheckGenerateNoiseMap(NoiseSettings noiseSettings, float minHeight, float maxHeight){
        // Act
        float[,] actual = Noise.GenerateNoiseMap(
            defaultSize,
            noiseSettings, 
            defaultSampleCentre);

        // Assert
        float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;
		for (int y = 0; y < defaultSize; y++) {
			for (int x = 0; x < defaultSize; x++) {
                float height = actual[x, y];
				if (height > maxNoiseHeight) {
					maxNoiseHeight = height;
				} 
				if (height < minNoiseHeight) {
					minNoiseHeight = height;
				}
            }
        }
        Assert.GreaterOrEqual(
            minNoiseHeight, minHeight, 
            "Min Height {0} should be greater than minimal min height {1}", 
            minNoiseHeight, minHeight);
        Assert.LessOrEqual(
            maxNoiseHeight, maxHeight, 
            "Max Height {0} should be less than minimal max height {1}", 
            maxNoiseHeight, maxHeight);
        Assert.Less(
            minNoiseHeight, maxHeight, 
            "Min Height {0} should be less than minimal max height {1}", 
            minNoiseHeight, maxHeight);
        Assert.Greater(
            maxNoiseHeight, minHeight, 
            "Max Height {0} should be greater than minimal min height {1}", 
            maxNoiseHeight, minHeight);
    }
}
