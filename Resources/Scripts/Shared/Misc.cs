﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Misc
{

    public static Vector3 MaxVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public static Color lightGreen = new Color(0.34f, 1, 0.44f, 0.75f);
    public static Color darkGreen = new Color(0.11f, 0.35f, 0.13f, 0.75f);

    public static Vector3 Lerp3(Vector3 start, Vector3 center, Vector3 end, float time)
    {
        return Mathf.Pow(1 - time, 2) * start + 2 * (1 - time) * time * center + Mathf.Pow(time, 2) * end;
    }

    public static Vector3 Lerp3CenterHeight(Vector3 start, Vector3 center, Vector3 end, float time)
    {
        Vector3 position = Lerp3(start, center, end, time);
        position.y = Mathf.Lerp(start.y, end.y, time);
        return position;
    }

    public static Vector3 Round(Vector3 toRound)
    {
        return new Vector3(Mathf.Round(toRound.x), Mathf.Round(toRound.y), Mathf.Round(toRound.z));
    }

    public static float CalculateDistance(Vector3 startPosition, Vector3 controlPosition, Vector3 endPosition)
    {
        float distance = 0;
        Vector3 lastPosition = startPosition;
        for (float t = 0.01f; t <= 1f; t += 0.01f)
        {
            Vector3 currentPosition = Lerp3(startPosition, controlPosition, endPosition, t);
            distance += Vector3.Distance(new Vector3(lastPosition.x, 0, lastPosition.z), new Vector3(currentPosition.x, 0, currentPosition.z));
            lastPosition = currentPosition;
        }

        return distance;
    }

    public static Vector3 CalculateLeft(Vector3 point, Vector3 nextPoint)
    {
        Vector3 forward = (nextPoint - point);
        return new Vector3(-forward.z, 0, forward.x).normalized;
    }

    public static Vector3 CalculateLeft(Vector3 forward)
    {
        return new Vector3(-forward.z, 0, forward.x).normalized;
    }

    public static Vector3 CalculateLeft(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 prevoiusPoint, int index)
    {
        Vector3 forward;
        if (index < points.Length - 1)
        {
            if (index == 0 && prevoiusPoint != MaxVector3)
            {
                forward = points[0] - prevoiusPoint;
            }
            else
            {
                forward = points[index + 1] - points[index];
            }
        }
        else
        {
            // Last vertices
            if (nextSegmentPoints != null)
            {
                if (nextSegmentPoints.Length > 1)
                {
                    forward = nextSegmentPoints[1] - nextSegmentPoints[0];
                }
                else
                {
                    forward = nextSegmentPoints[0] - points[points.Length - 1];
                }
            }
            else
            {
                forward = points[index] - points[index - 1];
            }
        }

        return new Vector3(-forward.z, 0, forward.x).normalized;
    }

    public static Vector3 GetCenter(Vector3 one, Vector3 two)
    {
        Vector3 difference = two - one;
        return (one + (difference / 2));
    }

    public static float GetCenter(float one, float two)
    {
        float difference = two - one;
        return (one + (difference / 2));
    }

    public static Vector3 FindPointInCircle(float radius, int i, float degreesPerStep)
    {
        return Quaternion.AngleAxis(degreesPerStep * i, Vector3.up) * (Vector3.right * radius);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Vector3 GetNearestGuidelinePoint(Vector3 hitPosition)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        Vector3 nearest = MaxVector3;
        float nearestDistance = float.MaxValue;
        Vector2 mousePosition = new Vector3(hitPosition.x, hitPosition.z);

        for (int i = 0; i < roadSegments.Length; i++)
        {
            RoadGuideline roadGuideline = roadSegments[i].startGuidelinePoints;

            if (roadGuideline != null)
            {
                Vector3 nearestOnLine = CheckGuideline(nearestDistance, mousePosition, roadGuideline, roadSegments[i].transform.parent.parent.GetComponent<RoadCreator>().settings);
                if (nearestOnLine != MaxVector3)
                {
                    nearest = nearestOnLine;
                    nearestDistance = Vector2.Distance(mousePosition, new Vector2(nearestOnLine.x, nearestOnLine.z));
                }
            }

            roadGuideline = roadSegments[i].centerGuidelinePoints;
            if (roadGuideline != null)
            {
                Vector3 nearestOnLine = CheckGuideline(nearestDistance, mousePosition, roadGuideline, roadSegments[i].transform.parent.parent.GetComponent<RoadCreator>().settings);
                if (nearestOnLine != MaxVector3)
                {
                    nearest = nearestOnLine;
                    nearestDistance = Vector2.Distance(mousePosition, new Vector2(nearestOnLine.x, nearestOnLine.z));
                }
            }

            roadGuideline = roadSegments[i].endGuidelinePoints;
            if (roadGuideline != null)
            {
                Vector3 nearestOnLine = CheckGuideline(nearestDistance, mousePosition, roadGuideline, roadSegments[i].transform.parent.parent.GetComponent<RoadCreator>().settings);
                if (nearestOnLine != MaxVector3)
                {
                    nearest = nearestOnLine;
                    nearestDistance = Vector2.Distance(mousePosition, new Vector2(nearestOnLine.x, nearestOnLine.z));
                }
            }
        }

        return nearest;
    }

#if UNITY_EDITOR
    private static Vector3 CheckGuideline(float nearestDistance, Vector3 mousePosition, RoadGuideline roadGuideline, SerializedObject settings)
    {
        Vector3 nearestLinePoint = Misc.FindNearestPointOnLine(new Vector2(roadGuideline.startPoint.x, roadGuideline.startPoint.z), new Vector2(roadGuideline.endPoint.x, roadGuideline.endPoint.z), mousePosition);
        float distance = Vector2.Distance(mousePosition, nearestLinePoint);

        if (distance < nearestDistance && distance < settings.FindProperty("roadGuidelinesSnapDistance").floatValue)
        {
            nearestDistance = distance;
            return new Vector3(nearestLinePoint.x, roadGuideline.centerPoint.y, nearestLinePoint.y);
        }

        return MaxVector3;
    }
#endif

    public static void DrawRoadGuidelines(Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i].transform.GetChild(0).childCount == 3)
            {
                if (roadSegments[i].transform.GetSiblingIndex() == 0)
                {
                    DrawRoadGuidelines(roadSegments[i].startGuidelinePoints, 0, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                }

                DrawRoadGuidelines(roadSegments[i].centerGuidelinePoints, 1, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                DrawRoadGuidelines(roadSegments[i].endGuidelinePoints, 2, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
            }
        }
    }

    private static void DrawRoadGuidelines(RoadGuideline guidelines, int child, RoadSegment roadSegment, Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        if (roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings == null)
        {
            roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings = RoadCreatorSettings.GetSerializedSettings();
        }

        if (child == 1)
        {
            Handles.color = roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings.FindProperty("roadControlGuidelinesColour").colorValue;
        }
        else
        {
            Handles.color = roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings.FindProperty("roadGuidelinesColour").colorValue;
        }

        if (guidelines != null && roadSegment.transform.GetChild(0).GetChild(child).gameObject != objectToMove && roadSegment.transform.GetChild(0).GetChild(child).gameObject != extraObjectToMove)
        {
            Vector2 mousePositionXZ = new Vector3(mousePosition.x, mousePosition.z);
            Vector2 nereastPoint = Misc.FindNearestPointOnLine(new Vector2(guidelines.startPoint.x, guidelines.startPoint.z), new Vector2(guidelines.endPoint.x, guidelines.endPoint.z), mousePositionXZ);

            if (Vector2.Distance(mousePositionXZ, nereastPoint) < roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings.FindProperty("roadGuidelinesDistance").floatValue)
            {
                Handles.DrawLine(guidelines.centerPoint, guidelines.startPoint);
                Handles.DrawLine(guidelines.centerPoint, guidelines.endPoint);
                Handles.DrawSolidDisc(guidelines.centerPoint, Vector3.up, roadSegment.transform.parent.parent.GetComponent<RoadCreator>().settings.FindProperty("pointSize").floatValue * 0.75f);

                Vector3 left = CalculateLeft(guidelines.startPoint, guidelines.endPoint);
                Handles.DrawLine(guidelines.startPoint - left * 0.5f, guidelines.startPoint + left * 0.5f);
                Handles.DrawLine(guidelines.endPoint - left * 0.5f, guidelines.endPoint + left * 0.5f);
            }
            
        }
    }

    public static Vector3 InverseX(Vector3 vector)
    {
        return new Vector3(-vector.x, vector.y, vector.z);
    }

    // Source: http://projectperko.blogspot.com/2016/08/multi-material-mesh-merge-snippet.html, with changes
    public static void ConvertToMesh(GameObject gameObject, string name)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>(gameObject.GetComponentsInChildren<MeshFilter>());
        List<Material> materials = new List<Material>();
        List<string> materialNames = new List<string>();
        MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        // Support objects with more materials than meshes
        for (int i = 0; i < meshFilters.Count; i++)
        {
            if (meshFilters[i].sharedMesh != null)
            {
                if (meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials.Length > meshFilters[i].sharedMesh.subMeshCount)
                {
                    meshFilters[i].sharedMesh.subMeshCount += 1;
                    meshFilters[i].sharedMesh.SetTriangles(meshFilters[i].sharedMesh.triangles, 1);
                }
            }
            else
            {
                meshFilters.RemoveAt(i);
            }
        }

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Material[] localMaterials = meshRenderer.sharedMaterials;

            foreach (Material localMaterial in localMaterials)
            {
                // if (!materials.Contains(localMaterial))
                if (!materialNames.Contains(localMaterial.ToString()))
                {
                    materials.Add(localMaterial);
                    materialNames.Add(localMaterial.ToString());
                }
            }
        }

        List<Mesh> subMeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                Material[] localMaterials = meshRenderer.sharedMaterials;

                for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    // if (localMaterials[materialIndex] != material)
                    if (localMaterials[materialIndex].ToString() != material.ToString())
                    {
                        continue;
                    }

                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = meshFilter.sharedMesh;
                    combineInstance.subMeshIndex = materialIndex;
                    Matrix4x4 matrix = Matrix4x4.identity;
                    matrix.SetTRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
                    combineInstance.transform = matrix;
                    combineInstances.Add(combineInstance);
                }
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineInstances.ToArray(), true);
            subMeshes.Add(mesh);
        }

        List<CombineInstance> finalCombiners = new List<CombineInstance>();
        foreach (Mesh mesh in subMeshes)
        {
            CombineInstance combineInstance = new CombineInstance();
            combineInstance.mesh = mesh;
            combineInstance.subMeshIndex = 0;
            combineInstance.transform = Matrix4x4.identity;
            finalCombiners.Add(combineInstance);
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);

        GameObject newMesh = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(newMesh, "Create Combined Mesh");
        newMesh.AddComponent<MeshFilter>();
        newMesh.AddComponent<MeshRenderer>();
        newMesh.AddComponent<MeshCollider>();
        newMesh.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        newMesh.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        newMesh.GetComponent<MeshCollider>().sharedMesh = finalMesh;
        Selection.activeGameObject = newMesh;
        Undo.DestroyObjectImmediate(gameObject.gameObject);
    }

    public static Vector2 FindNearestPointOnLine(Vector2 start, Vector2 end, Vector2 point)
    {
        //Get heading
        Vector2 heading = (end - start);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector2 lhs = point - start;
        float dotProduct = Vector2.Dot(lhs, heading);
        dotProduct = Mathf.Clamp(dotProduct, 0f, magnitudeMax);
        return start + heading * dotProduct;
    }

    public static Vector3 GetLineIntersection(Vector3 point1, Vector3 direction1, Vector3 point2, Vector3 direction2, float length1 = float.MaxValue, float lenght2 = float.MaxValue)
    {
        float originalY = point1.y;
        point1.y = 0;
        direction1.y = 0;
        point2.y = 0;
        direction2.y = 0;

        Vector3 lineDirection = point2 - point1;
        Vector3 crossVector1and2 = Vector3.Cross(direction1, direction2);
        Vector3 crossVector3and2 = Vector3.Cross(lineDirection, direction2);

        float planarFactor = Vector3.Dot(lineDirection, crossVector1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.01f && crossVector1and2.sqrMagnitude > 0.01f)
        {
            float distance = Vector3.Dot(crossVector3and2, crossVector1and2) / crossVector1and2.sqrMagnitude;
            Vector3 point = point1 + (direction1 * distance);
            distance = Vector3.Distance(point, point1);
            float distance2 = Vector3.Distance(point, point2);

            // Check if they intersection in front of the points and not behind
            if (point2 + distance2 * direction2 == point && point1 + distance * direction1 == point && distance < length1 && distance2 < lenght2)
            {
                return new Vector3(point.x, originalY, point.z);
            }
            else
            {
                return MaxVector3;
            }
        }
        else
        {
            return MaxVector3;
        }
    }

    public static void DrawPoint(RoadCreatorSettings.PointShape pointShape, Vector3 position, float size, bool showMoveHandle = true)
    {
        if (pointShape == RoadCreatorSettings.PointShape.Cube)
        {
            Handles.CubeHandleCap(0, position, Quaternion.Euler(-90, 0, 0), size, EventType.Repaint);
        }
        else if (pointShape == RoadCreatorSettings.PointShape.Cylinder)
        {
            Handles.CylinderHandleCap(0, position, Quaternion.Euler(-90, 0, 0), size, EventType.Repaint);
        }
        else if (pointShape == RoadCreatorSettings.PointShape.Sphere)
        {
            Handles.SphereHandleCap(0, position, Quaternion.Euler(-90, 0, 0), size, EventType.Repaint);
        }
        else if (pointShape == RoadCreatorSettings.PointShape.Cone)
        {
            Handles.ConeHandleCap(0, position, Quaternion.Euler(-90, 0, 0), size, EventType.Repaint);
        }

        /*if (showMoveHandle == true)
        {
            Handles.Slider(15, position, Vector3.forward, size * 3, Handles.ArrowHandleCap, 1);
            return position;// Handles.FreeMoveHandle(position, Quaternion.Euler(0, 90, 0), size * 10, Vector3.one, Handles.ArrowHandleCap);
            //return Handles.PositionHandle(position + new Vector3(0, size, 0), Quaternion.identity) - new Vector3(0, size, 0);
        }
        else
        {
            return position;
        }*/
    }
}
#endif