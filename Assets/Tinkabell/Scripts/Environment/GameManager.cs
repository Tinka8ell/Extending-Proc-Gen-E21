using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DevionGames.UIWidgets;

public class GameManager : MonoBehaviour
{
    public UnityEvent GameClockTickEvent;
    public bool stillAlive;

    public string WorldName = "Default";

    [Header("Time of day control")]
    public DateTime baseTime = new DateTime(2022, 12, 1);  // then the game started (invented?)
    public Vector3 gameStartTime = new Vector3(6f, 0f, 0f); // temp to offset now from real time
    private long startTime; // actual base time to calcualte from
    public long period = 1; // period for environment changes - may be a minute in real life
    public long speed = 100; // how much faster game time is than lapse time
    public long Speed{
        get{
            return speed;
        }
        set {
            speed = value;
        }
    }
    [SerializeField] private long time; // number of seconds of this day
    public long Seconds{
        get{
            return time;
        }
    }
    [SerializeField] private long days = 0; // number of previous days
    public long Days{
        get{
            return days;
        }
    }

    // constants
    public static long wholeDay = 24 * 60 * 60; // 86400;
    public static long halfDay = wholeDay / 2;  // 43200;
    public static long quarterDay = wholeDay / 4;  // 21600;


    // singleton control
    private static GameObject singleton;
    public static GameObject Singleton{
        get {
            return singleton;
        }
    }
    private static GameManager instance;
    public static GameManager Instance {
        get
        {
            if (instance == null){
                Debug.LogError("GameManager instance is not set!");
            }
            return instance;
        }
    }

    public Repository repository = new Repository();

    public GameObject StoryBox;
    private UIWidget welcome;
    public bool CompletedIntro = false;
    public bool DoneMenu = false;

    void Awake(){
        if (singleton == null){
            singleton = gameObject; // we are the one
            DontDestroyOnLoad(gameObject); // and we are going nowhere
            instance = this;
        } else {
            DestroyImmediate(gameObject); // we only want one!
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // initialise startTime
        startTime = (long) DateTime.Now.TimeOfDay.TotalSeconds; // set to now
        float offset = HMS_to_Time(gameStartTime) / speed; // convert "game start time" to world time
        startTime -= (long) offset; // move back so now appears as time set in editor

        // start periodic clock tick
        stillAlive = true;
        StartCoroutine(GameClockTick(1)); // start soon

        StartGame();
    }

    private void StartGame(){
        if (!CompletedIntro){
            RunIntro();
        }
        if (CompletedIntro && !DoneMenu){
            OpenMenu();
        }
        if (DoneMenu){
            Debug.Log("Done menu, so load game");
            // load the game?
        }
    }

    private void RunIntro(){
        Debug.Log("Running Intro");
        welcome = WidgetUtility.Find<UIWidget>("WelcomeScreen");
        welcome.Show();
    }

    private void NextPage(){
        Debug.Log("Intro Second page");
    }

    private void OpenMenu(){
        Debug.Log("Opening Menu");
        CompletedIntro = true;
        UIWidget menu = WidgetUtility.Find<UIWidget>("Start Menu");
        menu.Show();
    }

    IEnumerator GameClockTick(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // for our clock tick length (world seconds)

        // update when we are
        long currentTime = (long) DateTime.Now.TimeOfDay.TotalSeconds; // get to now in seconds
        long lapseTime = currentTime - startTime; // how long since we started
        long gameTime = lapseTime * speed; // what is game time
        days = gameTime / wholeDay; // which days have gone
        time = gameTime % wholeDay; // game seconds in this day
        // tell the world
        GameClockTickEvent?.Invoke();
        if (stillAlive)
            StartCoroutine(GameClockTick(period)); // keep calling each period seconds
        // exit this iteration end end this coroutine
    }

    public static float HMS_to_Time(Vector3 hms)
    {
        return HMS_to_Time(hms.x, hms.y, hms.z);
    }

    public static float HMS_to_Time(float hour, float minute, float second)
    {
        return 3600 * hour + 60 * minute + second;
    }

}
