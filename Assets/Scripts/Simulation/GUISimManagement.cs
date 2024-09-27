using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class GUISimManagement : MonoBehaviour
{
    Simulation sim;
    Text t;
    HorizontalLayoutGroup hlg;
    RectTransform rectTransform;

    Image leftRect;
    Image middleRect;
    Image rightRect;

    [Range(0, 1)] public float widthBar = 0.03f;
    //[Range(0, 10)] public float widthScaleBar = 1f;
    [Range(0, 1)] public float heightScaleBar = 0.1f;

    float scaleTexture;

    // Use this for initialization
    void Start()
    {

        sim = GameObject.Find("DesirePath").GetComponent<Simulation>();

        scaleTexture = GameObject.Find("DesirePath").transform.Find("Quad").GetComponent<Transform>().localScale.x;

        t = GameObject.Find("UIScaleBar").transform.Find("Text").GetComponent<Text>();
        hlg = GameObject.Find("UIScaleBar").transform.Find("RefScaleBar").GetComponent<HorizontalLayoutGroup>();

        rectTransform = hlg.GetComponent<RectTransform>();

        leftRect = hlg.transform.Find("LeftRect").GetComponent<Image>();
        middleRect = hlg.transform.Find("MiddleRect").GetComponent<Image>();
        rightRect = hlg.transform.Find("RightRect").GetComponent<Image>();

        leftRect.rectTransform.sizeDelta = new Vector2(widthBar, leftRect.rectTransform.sizeDelta.y);
        rightRect.rectTransform.sizeDelta = new Vector2(widthBar, rightRect.rectTransform.sizeDelta.y);
        middleRect.rectTransform.sizeDelta = new Vector2(middleRect.rectTransform.sizeDelta.x, widthBar);

    }

    // Update is called once per frame
    void Update()
    {
        float relativeDepthView = sim.slimeSettings.speciesSettings[0].depthViewOffset * scaleTexture / (sim.generalSettings.width);
        //float relativeDepthView = 1f;
        t.text = sim.slimeSettings.speciesSettings[0].depthViewOffset.ToString() + " px";

        middleRect.rectTransform.sizeDelta = new Vector2(relativeDepthView - 2 * widthBar, widthBar);
        rectTransform.sizeDelta = middleRect.rectTransform.sizeDelta + new Vector2(2 * widthBar, 0);

    }
}
