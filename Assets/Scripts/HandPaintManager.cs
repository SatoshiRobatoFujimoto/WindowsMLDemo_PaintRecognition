using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class HandPaintManager : MonoBehaviour
{
    public GameObject HandModel;
    public Material LineMaterial;

    private bool isPress = false;
    private List<GameObject> MyLineList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        InteractionManager.InteractionSourceDetected += SourceDetected;
        InteractionManager.InteractionSourceUpdated += SourceUpdated;
        InteractionManager.InteractionSourceLost += SourceLost;
        InteractionManager.InteractionSourcePressed += SourcePressed;
        InteractionManager.InteractionSourceReleased += SourceReleased;
        HandModel.SetActive(false);
    }

    void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= SourceDetected;
        InteractionManager.InteractionSourceUpdated -= SourceUpdated;
        InteractionManager.InteractionSourceLost -= SourceLost;
        InteractionManager.InteractionSourcePressed -= SourcePressed;
        InteractionManager.InteractionSourceReleased -= SourceReleased;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Interaction
    void SourceDetected(InteractionSourceDetectedEventArgs state)
    {
        HandModel.SetActive(true);
    }

    void SourceUpdated(InteractionSourceUpdatedEventArgs state)
    {
        Vector3 pos;
        if (state.state.sourcePose.TryGetPosition(out pos))
        {
            HandModel.transform.SetPositionAndRotation(pos, Quaternion.identity);
            if (isPress == true)
            {
                LineRenderer linerenderer = MyLineList[MyLineList.Count - 1].GetComponent<LineRenderer>();
                linerenderer.positionCount++;
                linerenderer.SetPosition(linerenderer.positionCount - 1, transform.InverseTransformPoint(pos));
            }
        }
    }

    void SourceLost(InteractionSourceLostEventArgs state)
    {
        HandModel.SetActive(false);
    }

    void SourcePressed(InteractionSourcePressedEventArgs state)
    {
        isPress = true;

        GameObject Line = new GameObject(MyLineList.Count.ToString());
        Line.transform.SetParent(transform);
        Line.transform.localPosition = Vector3.zero;
        Line.transform.localRotation = Quaternion.identity;
        Line.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        LineRenderer Linerenderer = Line.AddComponent<LineRenderer>();
        Linerenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Linerenderer.receiveShadows = false;
        Linerenderer.loop = false;
        Linerenderer.material = LineMaterial;
        Linerenderer.widthMultiplier = 0.01f;
        Linerenderer.positionCount = 0;
        Linerenderer.useWorldSpace = false;
        MyLineList.Add(Line);
    }

    void SourceReleased(InteractionSourceReleasedEventArgs obj)
    {
        isPress = false;
    }
}
