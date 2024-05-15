using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class use : MonoBehaviour
{
    [SerializeField] Text text;
    GameObject hand;
    bool Use;


    void Start()
    {
        text.enabled = false;
        hand = GameObject.FindWithTag("Hand");
        Use = false;
        Debug.Log("sdsdsdsdswd d");
    }

    void Update()
    {
        if (Use)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                text.enabled = false;
                Use = false;
                Transform stuff = hand.transform.GetChild(0);
                stuff.SetParent(gameObject.transform);
                stuff.transform.localPosition = Vector3.zero;
                stuff.transform.rotation = Quaternion.identity;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hand.transform.childCount != 0)
        {
            text.enabled = true;
            Use = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        text.enabled = false;
        Use = false;
    }

    
}
