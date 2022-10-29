using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Welcome : MonoBehaviour
{
    public Text text;
    public Button next;
    public Button previous;
    public Button skip;
    public Button cont;
    public int page = 0;
    public List<string> pages;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Welcome.Start() called");
        page = 0;
        ShowPage();
    }

    public void NextPage()
    {
        page ++;
        if (page > pages.Count - 1){
            page = pages.Count - 1;
        }
        ShowPage();
    }

    public void PreviousPage()
    {
        page --;
        if (page < 0){
            page = 0;
        }
        ShowPage();
    }

    void ShowPage()
    {
        if (text == null){
            Debug.LogWarning("Welcome.ShowPage() called without a text field!");
        } else {
            // update text
            text.text = pages[page].Replace("\\n", "\n");
            // enable previous
            previous.gameObject.SetActive(page > 0);
            bool isLastPage = page < pages.Count - 1;
            // enable next
            next.gameObject.SetActive(isLastPage);
            // switch between skip and continue
            cont.gameObject.SetActive(!isLastPage);
            skip.gameObject.SetActive(isLastPage);
        }
    }
}
