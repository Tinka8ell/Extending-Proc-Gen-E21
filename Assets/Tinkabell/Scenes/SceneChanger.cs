using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public int nextSceneNumber = 0;

    public void ChangeScene(){
        ChangeScene(nextSceneNumber);
    }

    public void ChangeScene(int sceneNumber){
        SceneManager.LoadScene(sceneNumber, LoadSceneMode.Single);
    }
}
