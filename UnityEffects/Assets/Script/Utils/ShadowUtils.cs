using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script.Utils
{
	//two function can be used to set lightcamera
    public static class ShadowUtils
    {
        private static List<Vector4> _vList = new List<Vector4>();

        /// <summary>
        /// Set the light camera according to the main camera
        /// </summary>
        /// <param name="mainCamera"></param>
        /// <param name="lightCamera"></param>
        public static void SetLightCamera(Camera mainCamera, Camera lightCamera)
        {
            //1、	Figure out the 8 vertices of the view volume (in the main camera space) n plane（aspect * y, tan(r/2)* n,n）  f plane （aspect*y, tan(r/2) * f, f）
            float r = (mainCamera.fieldOfView / 180f) * Mathf.PI;
            //near plane
            Vector4 nLeftUp = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
            Vector4 nRightUp = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
            Vector4 nLeftDonw = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, -Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);
            Vector4 nRightDonw = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.nearClipPlane, -Mathf.Tan(r / 2) * mainCamera.nearClipPlane, mainCamera.nearClipPlane, 1);

            //far plane
            Vector4 fLeftUp = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
            Vector4 fRightUp = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
            Vector4 fLeftDonw = new Vector4(-mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, -Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);
            Vector4 fRightDonw = new Vector4(mainCamera.aspect * Mathf.Tan(r / 2) * mainCamera.farClipPlane, -Mathf.Tan(r / 2) * mainCamera.farClipPlane, mainCamera.farClipPlane, 1);

            //2、Transform 8 vertices into world space

            Matrix4x4 mainv2w = mainCamera.transform.localToWorldMatrix;
            Vector4 wnLeftUp = mainv2w * nLeftUp;
            Vector4 wnRightUp = mainv2w * nRightUp;
            Vector4 wnLeftDonw = mainv2w * nLeftDonw;
            Vector4 wnRightDonw = mainv2w * nRightDonw;
            //
            Vector4 wfLeftUp = mainv2w * fLeftUp;
            Vector4 wfRightUp = mainv2w * fRightUp;
            Vector4 wfLeftDonw = mainv2w * fLeftDonw;
            Vector4 wfRightDonw = mainv2w * fRightDonw;

            //Set the light camera to the center of the mainCamera's view volume 
            Vector4 nCenter = (wnLeftUp + wnRightUp + wnLeftDonw + wnRightDonw) / 4f;
            Vector4 fCenter = (wfLeftUp + wfRightUp + wfLeftDonw + wfRightDonw) / 4f;

            lightCamera.transform.position = (nCenter + fCenter) / 2f;
            //3、	calculate the view matrix for light
            Matrix4x4 lgihtw2v = lightCamera.transform.worldToLocalMatrix;
            //4、	Transforms vertices from world space to view space of light
            Vector4 vnLeftUp = lgihtw2v * wnLeftUp;
            Vector4 vnRightUp = lgihtw2v * wnRightUp;
            Vector4 vnLeftDonw = lgihtw2v * wnLeftDonw;
            Vector4 vnRightDonw = lgihtw2v * wnLeftDonw;
            //
            Vector4 vfLeftUp = lgihtw2v * wfLeftUp;
            Vector4 vfRightUp = lgihtw2v * wfRightUp;
            Vector4 vfLeftDonw = lgihtw2v * wfLeftDonw;
            Vector4 vfRightDonw = lgihtw2v * wfRightDonw;

            _vList.Clear();
            _vList.Add(vnLeftUp);
            _vList.Add(vnRightUp);
            _vList.Add(vnLeftDonw);
            _vList.Add(vnRightDonw);

            _vList.Add(vfLeftUp);
            _vList.Add(vfRightUp);
            _vList.Add(vfLeftDonw);
            _vList.Add(vfRightDonw);
            //5、	bounding box
            float maxX = -float.MaxValue;
            float maxY = -float.MaxValue;
            float maxZ = -float.MaxValue;
            float minZ = float.MaxValue;
            for (int i = 0; i < _vList.Count; i++)
            {
                Vector4 v = _vList[i];
                if (Mathf.Abs(v.x) > maxX)
                {
                    maxX = Mathf.Abs(v.x);
                }
                if (Mathf.Abs(v.y) > maxY)
                {
                    maxY = Mathf.Abs(v.y);
                }
                if (v.z > maxZ)
                {
                    maxZ = v.z;
                }
                else if (v.z < minZ)
                {
                    minZ = v.z;
                }
            }
            //5.5 Avoid objects being cut off by the light view volume near plane
            if (minZ < 0)
            {
                lightCamera.transform.position += -lightCamera.transform.forward.normalized * Mathf.Abs(minZ);
                maxZ = maxZ - minZ;
            }

            //6、	Determine the projection setting according to the bounding box
            lightCamera.orthographic = true;
            lightCamera.aspect = maxX / maxY;
            lightCamera.orthographicSize = maxY;
            lightCamera.nearClipPlane = 0.0f;
            lightCamera.farClipPlane = Mathf.Abs(maxZ);
        }
        /// <summary>
        /// et the light camera according to a bounding box
        /// </summary>
        /// <param name="b"></param>
        /// <param name="lightCamera"></param>
        public static void SetLightCamera(Bounds b, Camera lightCamera)
        {
            //1、setting the position of lightcamera
            lightCamera.transform.position = b.center;
            //2、	calculate the view matrix for light
            Matrix4x4 lgihtw2v = lightCamera.transform.worldToLocalMatrix;
            //3、	Transforms vertices from world space to view space of light
            Vector4 vnLeftUp = lgihtw2v * new Vector3(b.max.x, b.max.y, b.max.z);
            Vector4 vnRightUp = lgihtw2v * new Vector3(b.max.x, b.min.y, b.max.z);
            Vector4 vnLeftDonw = lgihtw2v * new Vector3(b.max.x, b.max.y, b.min.z);
            Vector4 vnRightDonw = lgihtw2v * new Vector3(b.min.x, b.max.y, b.max.z);
            //
            Vector4 vfLeftUp = lgihtw2v * new Vector3(b.min.x, b.min.y, b.min.z); ;
            Vector4 vfRightUp = lgihtw2v * new Vector3(b.min.x, b.max.y, b.min.z); ;
            Vector4 vfLeftDonw = lgihtw2v * new Vector3(b.min.x, b.min.y, b.max.z); ;
            Vector4 vfRightDonw = lgihtw2v * new Vector3(b.max.x, b.min.y, b.min.z); ;

            _vList.Clear();
            _vList.Add(vnLeftUp);
            _vList.Add(vnRightUp);
            _vList.Add(vnLeftDonw);
            _vList.Add(vnRightDonw);

            _vList.Add(vfLeftUp);
            _vList.Add(vfRightUp);
            _vList.Add(vfLeftDonw);
            _vList.Add(vfRightDonw);
            //4、calculate bounding box
            float maxX = -float.MaxValue;
            float maxY = -float.MaxValue;
            float maxZ = -float.MaxValue;
            float minZ = float.MaxValue;
            for (int i = 0; i < _vList.Count; i++)
            {
                Vector4 v = _vList[i];
                if (Mathf.Abs(v.x) > maxX)
                {
                    maxX = Mathf.Abs(v.x);
                }
                if (Mathf.Abs(v.y) > maxY)
                {
                    maxY = Mathf.Abs(v.y);
                }
                if (v.z > maxZ)
                {
                    maxZ = v.z;
                }
                else if (v.z < minZ)
                {
                    minZ = v.z;
                }
            }
            //4.5 Avoid objects being cut off by the light view volume near plane
            if (minZ < 0)
            {
                lightCamera.transform.position += -lightCamera.transform.forward.normalized * Mathf.Abs(minZ);
                maxZ = maxZ - minZ;
            }

            //5、	Determine the projection setting according to the bounding box
            lightCamera.orthographic = true;
            lightCamera.aspect = maxX / maxY;
            lightCamera.orthographicSize = maxY;
            lightCamera.nearClipPlane = 0.0f;
            lightCamera.farClipPlane = Mathf.Abs(maxZ);
        }

    }
}
