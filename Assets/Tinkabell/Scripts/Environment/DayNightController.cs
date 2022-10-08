using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
 *    Need to add the Sky at Night too.
 */

public class DayNightController : MonoBehaviour
{
    [SerializeField] private Transform sunTransform;
    [SerializeField] private Light sun;
    [SerializeField] private float angleAtNoon;
    [SerializeField] private Vector3 hourMinuteSecond = new Vector3(6f, 0f, 0f), hmsSunSet = new Vector3(18f, 0f, 0f);
    [SerializeField] public long days = 0;
    [SerializeField] public float speed = 100;
    [SerializeField] private float intensityAtNoon = 1f, intensityAtSunSet = 0.5f;
    [SerializeField] private Color fogColorDay = Color.grey, fogColorNight = Color.black;
    [SerializeField] private Transform starsTransform;
    [SerializeField] private Vector3 hmsStarsLight = new Vector3(19f, 30f, 0f), hmsStarsExtinguish = new Vector3(03f, 30f, 0f);
    [SerializeField] private float starsFadeInTime = 7200f, starsFadeOutTime = 7200f;

    [SerializeField] private float period = 1f; // period for environment changes
    //[NonSerialized] 
    public float time;

    [SerializeField] private LightInSky sunData;
    [SerializeField] private LightInSky moonData;


    private float intensity, rotation, prev_rotation = -1f, sunSet, sunRise, sunDayRatio, fade, timeLight, timeExtinguish;
    private Color tintColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private Vector3 dir;
    private Renderer rend;
    private long startTime;
    public bool timeRunning = true;

    // constants
    private static long seconsInDay = 24 * 60 * 60; // 86400;
    private static float timeToDegrees =  360f / seconsInDay;
    private static float halfDay = seconsInDay / 2;  // 43200;
    private static float quarterDay = seconsInDay / 4;  // 21600;
    private static float moonToSunRatio = HMS_to_Time(24, 50, 0) / seconsInDay; // how much slower the mmon is

    void Start()
    {
        if (starsTransform != null) 
            rend = starsTransform?.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        time = HMS_to_Time(hourMinuteSecond);
        sunSet = HMS_to_Time(hmsSunSet);
        sunRise = seconsInDay - sunSet;
        sunDayRatio = (sunSet - sunRise) / halfDay;
        dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angleAtNoon), Mathf.Sin(Mathf.Deg2Rad * angleAtNoon), 0f);
        // dir = new Vector3(1f, 0f, 0f); 
        starsFadeInTime /= speed;
        starsFadeOutTime /= speed;
        fade  = 0;
        timeLight = HMS_to_Time(hmsStarsLight);
        timeExtinguish = HMS_to_Time(hmsStarsExtinguish);

        // initialise the previous position
        sunTransform.localEulerAngles = Vector3.zero; // may be ? eulerAngles
        prev_rotation = 0f;
        startTime = (long) DateTime.Now.Date.TimeOfDay.TotalSeconds; // set to the beginning of today!
        
        StartCoroutine(SlowUpdate(1)); // start soon
    }

    IEnumerator SlowUpdate(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // for first iteration

        while(timeRunning){
            // update when we are
            GetTime();
            
            rotation = (time - quarterDay) * timeToDegrees;
            // Debug.LogFormat("Rotation: {0}, prev: {1}, delta: {2}", rotation, prev_rotation, (rotation - prev_rotation));

            // do the sun
            intensity = RotateLightInSky(sunData, rotation, prev_rotation);

            // do the moon
            // convert rotation and days to moon rotation
            float moonRotation = ConvertForMoon(rotation);
            float moonPrevious = ConvertForMoon(prev_rotation);
            intensity = RotateLightInSky(moonData, moonRotation, moonPrevious);

            // do the stars
            if (starsTransform != null) 
                starsTransform.Rotate(dir, rotation - prev_rotation);

            RenderSettings.fogColor = Color.Lerp(fogColorNight, fogColorDay, intensity * intensity);

            if (Time_Falls_Between(time, timeLight, timeExtinguish))
            {
                fade += Time.deltaTime / starsFadeInTime;
                if (fade > 1f) fade = 1f;
            }
            else
            {
                fade -= Time.deltaTime / starsFadeOutTime;
                if (fade < 0f) fade = 0f;
            }
            tintColor.a = fade;
            if (rend != null) 
                rend.material.SetColor("_TintColor", tintColor);

            // prep for next iteration
            prev_rotation = rotation;
            yield return new WaitForSecondsRealtime(period); // so we can control how often things change
        }
    }

    private float RotateLightInSky (LightInSky lightInSky, float rotation, float prev_rotation){
        lightInSky.transform.Rotate(dir, rotation - prev_rotation);

        float intensity;
        if (time < sunRise) intensity = intensityAtSunSet * time / sunRise;
        else if (time < 43200f) intensity = intensityAtSunSet + (intensityAtNoon - intensityAtSunSet) * (time - sunRise) / (43200f - sunRise);
        else if (time < sunSet) intensity = intensityAtNoon - (intensityAtNoon - intensityAtSunSet) * (time - 43200f) / (sunSet - 43200f);
        else intensity = intensityAtSunSet - (1f - intensityAtSunSet) * (time - sunSet) / (86400f - sunSet);

        if (lightInSky.light != null) lightInSky.light.intensity = intensity;
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

    private void GetTime(){
        var now = DateTime.Now;
        long currentTime = (long) now.TimeOfDay.TotalSeconds;
        long lapseTime = currentTime - startTime; // how long since we started
        long gameTime = lapseTime * (long) speed; // what is game time
        days = gameTime / seconsInDay; // which daye we are
        time = (float) (gameTime % seconsInDay); // game seconds in this day
    }

    private static float HMS_to_Time(Vector3 hms)
    {
        return HMS_to_Time(hms.x, hms.y, hms.z);
    }

    private static float HMS_to_Time(float hour, float minute, float second)
    {
        return 3600 * hour + 60 * minute + second;
    }

    private bool Time_Falls_Between(float currentTime, float startTime, float endTime)
    {
        if (startTime<endTime)
        {
            if (currentTime >= startTime && currentTime <= endTime) return true;
            else return false;
        }
        else
        {
            if (currentTime < startTime && currentTime > endTime) return false;
            else return true;
        }
        
    }

}

[Serializable]
public class LightInSky{

    public Transform transform;
    public Light light;
    public float angleFromHighest;
    public float intensityAtHighest = 1f, intensityAtLowest = 0.5f;
    public Color fogColorUp = Color.grey, fogColorDown = Color.black;
}
