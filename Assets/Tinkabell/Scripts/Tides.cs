using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tides : MonoBehaviour
{
    public float maxTideHeight;
    private DayNightController dayNightController;
    public Vector3 position; // our base point
    public float tideHeight;

    // Start is called before the first frame update
    void Start()
    {
        dayNightController = GameManager.Singleton.GetComponent<DayNightController>();
        dayNightController.TideMovedEvent.AddListener(UpdateTide);
        position = transform.position;
    }

    private void OnDestroy(){
        dayNightController.TideMovedEvent.RemoveListener(UpdateTide);
    }

    void UpdateTide(float height)
    {
        // called when the moon has changed the tide height
        // height is between -1 and 1 and represents the fraction of the tide height
        if (gameObject.activeSelf){
            // only if visible
            tideHeight = height * maxTideHeight;
            transform.position = position + Vector3.up * tideHeight; // move up or down by that
        }
    }
}
