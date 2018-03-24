using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenConsoleController : MonoBehaviourSingleton<ScreenConsoleController> {

    private Text text;
    public int messageBufferSize = 10;
    Queue stringQueue;

    private void Start(){
        text = GetComponent<Text>();
        text.text = "";
        stringQueue = new Queue();
    }

    public void Append(string _input) {
        if (stringQueue.Count + 1 > messageBufferSize) {
            stringQueue.Dequeue();
        }
        stringQueue.Enqueue(_input);

        string str = "";
        foreach (string s in stringQueue) {
            str += s + "\n";
        }

        text.text = str;
    }

    public void Clear() {
        text.text = "";
    }
}
