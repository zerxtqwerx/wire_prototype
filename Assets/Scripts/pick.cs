using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pick : MonoBehaviour
{
    [SerializeField] Text text;
    [SerializeField] GameObject stuff;
    GameObject hand;
    bool Pick;

    void Start()
    {
        text.enabled = false;
        hand = GameObject.FindWithTag("Hand");
        Pick = false;
        Debug.Log("sdsdsdsdswd");
    }

    void Update()
    {
        if (Pick)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                text.enabled = false;
                Pick = false;
                stuff.transform.SetParent(hand.transform);
                stuff.transform.localPosition = Vector3.zero;
                stuff.transform.rotation = Quaternion.identity;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hand.transform.childCount == 0)
        {
            text.enabled = true;
            Pick = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        text.enabled = false;
        Pick = false;
    }

}
