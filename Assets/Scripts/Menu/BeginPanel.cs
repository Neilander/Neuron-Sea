using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeginPanel : MonoBehaviour
{
    public string LevelOneName;
    public string aboutUs;

    public GameObject volume;

    public bool isRed;

    private Image img;
    // Start is called before the first frame update
    void Start()
    {
         img= volume.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame(){
        SceneManager.LoadScene(LevelOneName);
    }

    public void AboutUs(){
        
    }

    public void Quit(){
        Application.Quit();
    }

    public void Volume(){
        volumeCanvas.Instance.OpenCanvas();

        /*
        if(isRed){
            img.color = new Color(255, 255, 255);
        }
        else {
            img.color = new Color(255, 192, 203);
        }*/
    }
}
