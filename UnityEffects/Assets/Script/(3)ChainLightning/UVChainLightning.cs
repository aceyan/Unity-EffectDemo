using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// attaching to a Gameobject to implement lightning effect by using UV movement tech
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class UVChainLightning : MonoBehaviour
{
   
    public float detail = 1;//As you increase, the number of lines decreases and each line is longer.
    public float displacement = 15;//The displacement, that is, the maximum deviation in the direction of the line value
    //adjusting parameters above in art assest
	
    public Transform target;
    public Transform start;
    public float yOffset = 0;
    private LineRenderer _lineRender;
    private List<Vector3> _linePosList;


    private void Awake()
    {
        _lineRender = GetComponent<LineRenderer>();
        _linePosList = new List<Vector3>();
    }

    private void Update()
    {
        if(Time.timeScale != 0)
        {
            _linePosList.Clear();
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            if (target != null)
            {
                endPos = target.position + Vector3.up * yOffset;
            }
            if(start != null)
            {
                startPos = start.position + Vector3.up * yOffset;
            }

            CollectLinPos(startPos, endPos, displacement);
            _linePosList.Add(endPos);

            _lineRender.SetVertexCount(_linePosList.Count);
            for (int i = 0, n = _linePosList.Count; i < n; i++)
            {
                _lineRender.SetPosition(i, _linePosList[i]);
            }
        }
    }

    //using algorithm of  midpoint displacement to generate subdivisions, which helps simulating shape of lightning
    private void CollectLinPos(Vector3 startPos, Vector3 destPos, float displace)
    {
        if (displace < detail)
        {
            _linePosList.Add(startPos);
        }
        else
        {

            float midX = (startPos.x + destPos.x) / 2;
            float midY = (startPos.y + destPos.y) / 2;
            float midZ = (startPos.z + destPos.z) / 2;

            midX += (float)(UnityEngine.Random.value - 0.5) * displace;
            midY += (float)(UnityEngine.Random.value - 0.5) * displace;
            midZ += (float)(UnityEngine.Random.value - 0.5) * displace;

            Vector3 midPos = new Vector3(midX,midY,midZ);

            CollectLinPos(startPos, midPos, displace / 2);
            CollectLinPos(midPos, destPos, displace / 2);
        }
    }


}    
