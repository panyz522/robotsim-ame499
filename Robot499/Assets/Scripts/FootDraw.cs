using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FootDraw : MonoBehaviour {
    public GameObject gameSystem;
    public float frontFootOffset;
    public float rearFootOffset;
    public float totalHeight;
    public float stableHeight;

    private float[] offsets;
    private List<Image> footImages;
    private IRobotController robot;
    private List<GameObject> foots;
    private List<RectTransform> footPoss;

    // Use this for initialization
    void Start () {
        string nameOfDiaplay = this.name;
        foots = new List<GameObject>();
        footPoss = new List<RectTransform>();
        footImages = new List<Image>();
        offsets = new float[] { frontFootOffset, rearFootOffset };

        for (int i = 0; i < 4; i++)
        {
            foots.Add(GameObject.Find(nameOfDiaplay + "/Foot" + i));
            footPoss.Add(foots[i].GetComponent<RectTransform>());
            footImages.Add(foots[i].GetComponent<Image>());
        }
        
        robot = gameSystem.GetComponent<GlobalScript>().workingRobot.GetComponent<IRobotController>();
    }
	
	// Update is called once per frame
	void Update () {
        var rawPoss = robot.GetFootPositions();
        Vector2[] newPoss = new Vector2[4];
        float[] colors = new float[4];
        for (int i = 0; i < 4; i++)
        {
            newPoss[i] = new Vector2(40 * ((i % 2 == 0) ? -1 : 1), (rawPoss[i].x + offsets[(i < 2) ? 0 : 1]) * 10);
            colors[i] = (rawPoss[i].y > stableHeight - 0.1) ? 1 : 0.5f;
        }
        SetFootsPos(newPoss);
        SetFootHeight(colors);
	}

    public void SetFootsPos(Vector2[] poss)
    {
        for (int i = 0; i < 4; i++)
        {
            footPoss[i].anchoredPosition = poss[i];
        }
    }

    private void SetFootHeight(float[] colors)
    {
        for (int i = 0; i < 4; i++)
        {
            footImages[i].color = new Color(colors[i], colors[i], colors[i]);
        }
    }
}
