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
		else
		{
			SetExplore();
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetLeft()
	{
		GetComponent<MeshRenderer>().material = left;
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void SetRight()
	{
		GetComponent<MeshRenderer>().material = right;
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void SetGray()
	{
		GetComponent<MeshRenderer>().material = gray;
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void SetExplore()
	{
		GetComponent<MeshRenderer>().material = explore;
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void SetGoal()
	{
		GetComponent<MeshRenderer>().material = goal;
		GetComponent<MeshRenderer>().enabled = true;
	}

	public void Reset()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

}
