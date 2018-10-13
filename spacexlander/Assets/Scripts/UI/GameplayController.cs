using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class GameplayController : MonoBehaviour {

    public Text pauseButtontext;

    void Start() {
        pauseButtontext.text = Time.timeScale + "x";
    }

    public void Faster() {
        if (Time.timeScale == 0) {
            NormalSpeed();
        }
        else {
            Time.timeScale = Time.timeScale * 2;
        }

        pauseButtontext.text = Time.timeScale + "x";
    }

    public void Slower() {
        Time.timeScale = Time.timeScale / 2;

        pauseButtontext.text = Time.timeScale + "x";
    }

    float prevSpeed = 0;
    public void Pause() {
        if (Time.timeScale != 0) {
            prevSpeed = Time.timeScale;
            Time.timeScale = 0;
        }
        else {
            Time.timeScale = prevSpeed;
        }

        pauseButtontext.text = Time.timeScale + "x";
    }

    public void NormalSpeed() {
        Time.timeScale = 1;

        pauseButtontext.text = Time.timeScale + "x";
    }
}
