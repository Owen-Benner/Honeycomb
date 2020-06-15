using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDLogic : MonoBehaviour
{

	public Text gemCounter;
	public Text goalText;

	public Image gemPile;

	private int gems = 0;

    // Start is called before the first frame update
    void Start()
    {
		gemCounter.text = gems.ToString();
		ClearGoal();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	// Display goal text and gems
	public void SetGoal()
	{
		goalText.enabled = true;
		gemPile.enabled = true;
	}

	// Hide goal text and gems
	public void ClearGoal()
	{
		goalText.enabled = false;
		gemPile.enabled = false;
	}

	// Increment gem counter
	public void AddGem()
	{
		++gems;
		gemCounter.text = gems.ToString();
	}

}
