using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * Taken from the sample code of ReCogMission, and heavily modified.
 * License in /ReCogMissionLicense
 * Requires a Directional Light Source called Sun
 * Keeps track, and makes public, the Game Time (float - seconds) and Day (int - whole days)
 * Uses Vector3 for input HMS values (x = hours, y = minutes, z = seconds)
 * Mods:
 *    Use computer time * speed to get time
 *    Change Update() to be a coroutine, so we can reduce the number of updates per second!
 *    Initialise the sun driection using localEulerAngles, so we can put it in a box and turn it.
 *       The system works fine moving the sun from North to South, but we want to go East to West
 *       By mouning the "Sun" in a GameObject (EastWest), and rotating that object (0, -90, 0) we now go E-W!
 *    Lighting is funny.  
 *       Change the intensity of the sun like Survival Game and remove it when after dusk / before dawn
 *       Added a dusk / dawn length about the sunrise and sunset
 *       Some items glow in the dark (e.g. bushes and the ground) and others go black!
 *       Want to modularise the motion so we can add moon with different cycle lenght
 *    Added the Sky at Night too, but can't see it!
 *       Using pub / sub to notify ScatterMyStars when they have moved
 */

public class DayNightController : MonoBehaviour
{
    [Header("Sun")]
    [SerializeField] private LightInSky sunData;
    [SerializeField] private Vector3 hmsSunSet = new Vector3(18f, 0f, 0f);
    [SerializeField] private Color fogColorDay = Color.grey, fogColorNight = Color.black;

    [Header("Moon")]
    [SerializeField] private LightInSky moonData;

    [Header("Stars")]
    [SerializeField] private Transform starsTransform;
    [SerializeField] private Vector3 hmsStarsLight = new Vector3(19f, 30f, 0f), hmsStarsExtinguish = new Vector3(03f, 30f, 0f);
    [SerializeField] private float starsFadeInTime = 7200f, starsFadeOutTime = 7200f;


    private float intensity, rotation = 0f, prev_rotation = 0f, sunSet, sunRise, sunDayRatio, fade, timeLight, timeExtinguish;
    private Color tintColor = new Color(1f, 1f, 1f, 1f);
    private Renderer rend;
    private long startTime;

    // constants
    private static float timeToDegrees =  360f / GameManager.wholeDay;
    private static float moonToSunRatio = GameManager.HMS_to_Time(24, 50, 0) / GameManager.wholeDay; // how much slower the mmon is

    public UnityEvent<float> WorldSpunEvent;
    public UnityEvent<float> TideMovedEvent;

    // internals
    private float speed;
    private long time;
    private long days;

    void Start()
    {
        var lights = FindObjectsOfType<Light>();
        // set up sun
        sunData.SetDir();
        sunData.SetLight(lights, Vector3.zero);

        // set up moon
        moonData.SetDir();
        moonData.SetLight(lights, Vector3.right * 180f);

        // set up stars
        if (starsTransform == null) {
            var stars = GetComponent<ParticleSystem>();
            starsTransform = stars.transform;
            rend = stars.GetComponent<Renderer>();
        }

        // set up times
        sunSet = GameManager.HMS_to_Time(hmsSunSet);
        timeLight = GameManager.HMS_to_Time(hmsStarsLight);
        timeExtinguish = GameManager.HMS_to_Time(hmsStarsExtinguish);

        // claculate special times
        sunRise = GameManager.wholeDay - sunSet;
        sunDayRatio = (sunSet - sunRise) / GameManager.halfDay;
        speed = GameManager.Instance.Speed;
        starsFadeInTime /= speed;
        starsFadeOutTime /= speed;

        GameManager.Instance.GameClockTickEvent.AddListener(WorldTurns);
    }

    void WorldTurns(){
        time = GameManager.Instance.Seconds;
        days = GameManager.Instance.Days;
        
        rotation = (time - GameManager.quarterDay) * timeToDegrees;

        // do the sun
        intensity = RotateLightInSky(sunData, rotation, prev_rotation);

        // do fog
        RenderSettings.fogColor = Color.Lerp(fogColorNight, fogColorDay, intensity * intensity);

        // do the stars
        if (starsTransform != null) {
            //spin round same axis as sun
            starsTransform.Rotate(sunData.dir, rotation - prev_rotation);
        }

        float fade = 0f; // assume no stars (daytime
        if (time > timeLight){
            // after time to come out
            fade = Math.Clamp((time - timeLight) / starsFadeInTime, 0f, 1f); 
        }
        else if (time < timeExtinguish){
            // before time to go in
            fade = Math.Clamp((timeExtinguish - time) / starsFadeOutTime, 0f, 1f); 
        }
        tintColor.a = fade;
        if (rend != null) 
            rend.material.SetColor("_TintColor", tintColor);
        
        WorldSpunEvent?.Invoke(rotation);

        // do the moon
        // convert rotation and days to moon rotation
        float moonRotation = ConvertForMoon(rotation);
        float moonPrevious = ConvertForMoon(prev_rotation);
        RotateLightInSky(moonData, moonRotation, moonPrevious); // don't need the intensity
        TideMovedEvent?.Invoke(MathF.Sin(Mathf.Deg2Rad * moonRotation * 2)); // as two tides in a day

        // prep for next iteration
        prev_rotation = rotation;
    }

    private float RotateLightInSky (LightInSky lightInSky, float rotation, float prev_rotation){
        lightInSky.transform.Rotate(lightInSky.dir, rotation - prev_rotation);

        float intensity;
        if (time < sunRise) intensity = lightInSky.intensityAtLowest * time / sunRise;
        else if (time < GameManager.halfDay) intensity = lightInSky.intensityAtLowest + (lightInSky.intensityAtHighest - lightInSky.intensityAtLowest) * (time - sunRise) / (GameManager.halfDay - sunRise);
        else if (time < sunSet) intensity = lightInSky.intensityAtHighest - (lightInSky.intensityAtHighest - lightInSky.intensityAtLowest) * (time - GameManager.halfDay) / (sunSet - GameManager.halfDay);
        else intensity = lightInSky.intensityAtLowest - (1f - lightInSky.intensityAtLowest) * (time - sunSet) / (GameManager.wholeDay - sunSet);

        lightInSky.light.intensity = intensity;
        return intensity;
    }

    private float ConvertForMoon(float rotation){
        // moon rotates about the earth about every 24 hours and 50 minutes
        // it is therefore slightly slower than the sun which does it in 2h hours
        // adjust the rotation
        rotation *= moonToSunRatio;
        // allow for the days offset 
        rotation += days * moonToSunRatio;
        return rotation;
    }


}

[Serializable]
public class LightInSky{

    public string name = "Sun";
    public Light light;
    public Transform transform;
    public float angleFromHighest;
    [NonSerialized] public Vector3 dir; // calculated from angleFromHighest
    public float intensityAtHighest = 1f, intensityAtLowest = 0.5f;

    public void SetDir(){
        float angle = Mathf.Deg2Rad * angleFromHighest;
        dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
    }

    public void SetLight(Light[] lights, Vector3 startDir){
        if (light == null){
            light = Array.Find(lights, light => light.name.StartsWith(name));
            transform = light.transform;
        }
        if (transform == null){
            transform = light.transform;
        }
        // initialise the previous position
        transform.localEulerAngles = startDir; // may be ? eulerAngles
    }

}
