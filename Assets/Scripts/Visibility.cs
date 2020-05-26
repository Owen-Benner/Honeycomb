using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visibility : MonoBehaviour
{

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

	void SetLeft()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	void SetRight()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	void SetGray()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	void SetExplore()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	void SetGoal()
	{
		GetComponent<MeshRenderer>().enabled = true;
	}

	void Reset()
	{
		GetComponent<MeshRenderer>().enabled = false;
	}

}
