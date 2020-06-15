using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogWriter : MonoBehaviour
{

	public Transform player;
	public SimpleMovement move;
	public MazeLogic maze;

	public string fileName;

	public int frameFreq = 24;

    StreamReader lastRunReader;
	StreamWriter writer;

	private float lastFrame;
	private float frameTime;

	private string spc = " ";

	private int frame = 0;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            lastRunReader = new StreamReader(fileName);
            Debug.LogError("Repeated log filename! Update Config.txt or remove"
                    + " last log file from directory.");
            Application.Quit();
            return;
        }
        catch {}

        writer = new StreamWriter(fileName);
		frameTime = 1f / (float) frameFreq;

		WriteFrame();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - lastFrame >= frameTime)
        {
			WriteFrame();
            lastFrame += frameTime;
        }
    }

	private void WriteFrame()
	{
		writer.WriteLine("Frame " + (frame++).ToString() + ":" + spc
			+ string.Format("{0:N3}", player.position.x) + spc
			+ string.Format("{0:N3}", player.position.z) + spc
			+ string.Format("{0:N3}", player.eulerAngles.y) + spc
			+ string.Format("{0:N3}", Time.time));
	}

}
