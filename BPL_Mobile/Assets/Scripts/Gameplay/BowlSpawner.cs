using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowlSpawner : MonoBehaviour
{
    public GameObject bowlPrefab;
    private GameObject currentBowl;

    private Vector3 spawnPoint = new Vector3(0,1,-9);

    // Start is called before the first frame update
    void Start()
    {
        // create the first bowl from the prefab, at the starting point
        currentBowl = Instantiate(bowlPrefab, spawnPoint, Quaternion.identity);
        Transform tf = currentBowl.transform;
        
        Bounds bounds = currentBowl.GetComponent<Renderer>().bounds;
    
        // rescale bowl to be 12.7 cm
        Vector3 currentScale = tf.localScale;
        tf.localScale = tf.localScale * (0.127f/bounds.max.y);

        bounds = currentBowl.GetComponent<Renderer>().bounds;
        // position bowl so the bottom is touching the green
        Vector3 pos = tf.position;
        pos.y = bounds.extents.y;

        tf.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
