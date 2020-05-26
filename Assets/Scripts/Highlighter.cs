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

	// Awake is called when the script instance is being loaded
	void Awake()
	{

	}

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
		gameObject.SetActive(true);
	}

	public void SetRight()
	{
		GetComponent<MeshRenderer>().material = right;
		gameObject.SetActive(true);
	}

	public void SetGray()
	{
		GetComponent<MeshRenderer>().material = gray;
		gameObject.SetActive(true);
	}

	public void SetExplore()
	{
		GetComponent<MeshRenderer>().material = explore;
		gameObject.SetActive(true);
	}

	public void SetGoal()
	{
		GetComponent<MeshRenderer>().material = goal;
		gameObject.SetActive(true);
	}

	public void Reset()
	{
		gameObject.SetActive(false);
	}

}
