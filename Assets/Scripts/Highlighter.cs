using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{

	public Material left;
	public Material right;
	public Material gray;
	public Material explore;
	public Material goal;

    // Start is called before the first frame update
    void Start()
    {
		if(GameObject.FindWithTag("Player").GetComponent<SimpleMovement>().
			mode == 1)
		{
			Reset();
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetLeft()
	{
		GetComponent<MeshRenderer>().material = left;
	}

	public void SetRight()
	{
		GetComponent<MeshRenderer>().material = right;
	}

	public void Reset()
	{
		GetComponent<MeshRenderer>().material = gray;
	}

	public void SetExplore()
	{
		GetComponent<MeshRenderer>().material = explore;
	}

	public void SetGoal()
	{
		GetComponent<MeshRenderer>().material = goal;
	}

}
