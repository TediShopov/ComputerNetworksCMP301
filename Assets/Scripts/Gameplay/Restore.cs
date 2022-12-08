using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Restore : MonoBehaviour
{
    public GameObject FighterPrefab;
    public GameObject GameState;

  

    void ReplaceObject(GameObject to)
    {
        Transform[] Replaces;
        Replaces = to.GetComponentsInChildren<Transform>();

        foreach (Transform t in Replaces)
        {
            GameObject newObject;
            newObject = Instantiate(FighterPrefab);
            Debug.LogError("Instantiated  NEW Player Fighter");

            newObject.transform.position = t.position;
            newObject.transform.rotation = t.rotation;
            newObject.transform.parent = t.parent;
            newObject.GetComponent<FighterController>().isEnemy = to.GetComponent<FighterController>().isEnemy;


            Destroy(t.gameObject);
            Debug.LogError("Destroyed Original Player Fighter Object");

        }
    }

        // Update is called once per frame
        void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (StaticBuffers.Instance.Player != null)
            {
                ReplaceObject(StaticBuffers.Instance.Player);
            }
        }
    }
}
