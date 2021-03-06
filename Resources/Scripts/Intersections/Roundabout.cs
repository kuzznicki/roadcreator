﻿#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class Roundabout
{

    public static void GenerateRoundabout(Intersection intersection)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> bridgeVertices = new List<Vector3>();
        List<Vector3> connectionSectionsVertices = new List<Vector3>();

        List<int> triangles = new List<int>();
        List<int> bridgeTriangles = new List<int>();
        List<int> connectionTriangles = new List<int>();
        List<int> connectionSectionsTriangles = new List<int>();

        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uvs2 = new List<Vector2>();
        List<Vector2> bridgeUvs = new List<Vector2>();
        List<Vector2> connectionSectionsUvs = new List<Vector2>();
        List<Vector2> connectionSectionsUvs2 = new List<Vector2>();

        List<int> nearestLeftPoints = new List<int>();
        List<int> nearestRightPoints = new List<int>();
        int addedVertices = 0;

        if (intersection.outerExtraMeshesAsRoads == true)
        {
            ResetNonCenterExtraMeshes(intersection);
            CreateExtraMeshesFromRoads(intersection);
        }

        CreateRoundaboutVertices(intersection, ref vertices, ref uvs, ref uvs2);
        CreateRoadConnections(intersection, ref vertices, ref triangles, ref uvs, ref uvs2, ref bridgeVertices, ref bridgeTriangles, ref bridgeUvs, ref connectionTriangles, ref nearestLeftPoints, ref nearestRightPoints, ref addedVertices);
        CreateRoadSections(intersection, ref vertices, ref triangles, ref connectionSectionsVertices, ref connectionSectionsTriangles, ref connectionSectionsUvs, ref connectionSectionsUvs2, ref bridgeVertices, ref bridgeTriangles, ref bridgeUvs, addedVertices, nearestLeftPoints, nearestRightPoints);

        CreateCenterExtraMeshes(intersection);
        SetupRoundaboutMesh(intersection, vertices, triangles, connectionTriangles, connectionSectionsVertices, connectionSectionsTriangles, connectionSectionsUvs, connectionSectionsUvs2, uvs, uvs2);
        SetupBridgeMesh(intersection, bridgeVertices, bridgeTriangles, bridgeUvs);

        if (intersection.generateBridge == true && intersection.placePillars == true)
        {
            BridgeGeneration.PlaceRoundaboutPillars(intersection);
        }
    }

    private static void CreateRoadSections(Intersection intersection, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector3> connectionSectionsVertices, ref List<int> connectionSectionsTriangles, ref List<Vector2> connectionSectionsUvs, ref List<Vector2> connectionSectionsUvs2, ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, ref List<Vector2> bridgeUvs, int addedVertices, List<int> nearestLeftPoints, List<int> nearestRightPoints)
    {
        float textureRepeations = Mathf.PI * intersection.roundaboutRadius * intersection.textureTilingY * 0.5f;
        float rightStartWidth = 0;
        float rightEndWidth = 0;

        for (int j = 0; j < intersection.extraMeshes.Count; j++)
        {
            if (intersection.extraMeshes[j].index == 0)
            {
                rightStartWidth += intersection.extraMeshes[j].startWidth;
                rightEndWidth += intersection.extraMeshes[j].endWidth;
            }
        }

        for (int i = 0; i < intersection.connections.Count + 1; i++)
        {
            if (i < intersection.connections.Count || intersection.connections.Count == 0)
            {
                int startIndex;
                int endIndex;
                float leftStartWidth = 0;
                float leftEndWidth = 0;

                for (int j = 0; j < intersection.extraMeshes.Count; j++)
                {
                    if ((intersection.extraMeshes[j].index - 1) % 3 == 0 && (intersection.extraMeshes[j].index - 1) / 3 == i)
                    {
                        if (intersection.extraMeshes[j].startWidth == 0 && intersection.extraMeshes[j].endWidth == 0 && intersection.extraMeshes[j].yOffset == 0)
                        {
                            intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                            intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;
                        }
                        else
                        {
                            leftStartWidth += intersection.extraMeshes[j].startWidth;
                            leftEndWidth += intersection.extraMeshes[j].endWidth;
                        }
                    }
                }

                if (intersection.connections.Count == 0)
                {
                    startIndex = 0;
                    endIndex = vertices.Count - 2 - addedVertices;
                }
                else
                {
                    startIndex = nearestLeftPoints[i];

                    if (i == intersection.connections.Count - 1)
                    {
                        endIndex = nearestRightPoints[0];
                    }
                    else
                    {
                        endIndex = nearestRightPoints[i + 1];
                    }
                }

                List<RoundaboutExtraMesh> leftExtraMeshes = new List<RoundaboutExtraMesh>();
                float startOffsetLeft = 0;
                float endOffsetLeft = 0;
                float yOffsetLeft = 0;

                for (int j = 0; j < intersection.extraMeshes.Count; j++)
                {
                    if (intersection.extraMeshes[j].index > 0 && (intersection.extraMeshes[j].index - 1) % 3 == 0 && (intersection.extraMeshes[j].index - 1) / 3 == i)
                    {
                        if (intersection.extraMeshes[j].startWidth == 0 && intersection.extraMeshes[j].endWidth == 0 && intersection.extraMeshes[j].yOffset == 0)
                        {
                            intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                            intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;
                        }
                        else
                        {
                            if (leftExtraMeshes.Count > 0)
                            {
                                startOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.startWidth;
                                endOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.endWidth;
                                yOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.yOffset;
                            }

                            leftExtraMeshes.Add(new RoundaboutExtraMesh(intersection.extraMeshes[j], j, startOffsetLeft, endOffsetLeft, yOffsetLeft));
                        }
                    }
                }

                if (startIndex % 2f == 0 && endIndex % 2f == 0)
                {
                    CreateRoadSectionsTriangles(intersection, startIndex, endIndex, addedVertices, vertices, ref triangles, leftStartWidth, leftEndWidth, rightStartWidth, rightEndWidth, ref bridgeVertices, ref bridgeTriangles, ref bridgeUvs, ref leftExtraMeshes, textureRepeations);

                    if (intersection.connections.Count > 0)
                    {
                        int nextSegmentIndex = i + 1;
                        if (nextSegmentIndex > intersection.connections.Count - 1)
                        {
                            nextSegmentIndex = 0;
                        }

                        CreateConnectionSectionsTriangles(intersection, vertices, ref connectionSectionsVertices, ref connectionSectionsTriangles, ref connectionSectionsUvs, ref connectionSectionsUvs2, nearestLeftPoints[nextSegmentIndex], endIndex, addedVertices);
                    }
                }
                else
                {
                    Debug.LogError("For some reason a roundabout connection's start/end index is uneven");
                }
            }
        }
    }

    private static void CreateRoadSectionsTriangles(Intersection intersection, int startIndex, int endIndex, int addedVertices, List<Vector3> vertices, ref List<int> triangles, float leftStartWidth, float leftEndWidth, float rightStartWidth, float rightEndWidth, ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, ref List<Vector2> bridgeUvs, ref List<RoundaboutExtraMesh> leftExtraMeshes, float textureRepeations)
    {
        int vertexIndex = startIndex;
        int verticesLooped = 0;
        int verticesToLoop = 0;
        float distanceBetweenVertices = Vector3.Distance(vertices[0], vertices[2]);
        int extraMeshIndexOffset = 0;

        if (endIndex > startIndex)
        {
            verticesToLoop = endIndex - startIndex;
        }
        else
        {
            verticesToLoop = vertices.Count - 2 - startIndex - addedVertices;
            verticesToLoop += endIndex;
        }

        while ((vertexIndex - 2 != endIndex) && triangles.Count < 10000)
        {
            if (vertexIndex > vertices.Count - 4 - addedVertices)
            {
                vertexIndex = 0;
                extraMeshIndexOffset = 1;
            }

            if (vertexIndex != endIndex)
            {
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);

                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
            }

            // Bridge
            if (intersection.generateBridge == true)
            {
                float leftWidth = intersection.roundaboutWidth + Mathf.Lerp(leftStartWidth, leftEndWidth, (float)verticesLooped / verticesToLoop);
                float rightWidth = intersection.roundaboutWidth + Mathf.Lerp(rightStartWidth, rightEndWidth, (float)verticesLooped / verticesToLoop);

                Vector3 centerPoint = Misc.GetCenter(vertices[vertexIndex], vertices[vertexIndex + 1]);
                centerPoint.y -= intersection.yOffset;
                BridgeGeneration.AddRoadSegmentBridgeVertices(intersection, ref bridgeVertices, ref bridgeTriangles, ref bridgeUvs, centerPoint, leftWidth, rightWidth, (vertices[vertexIndex] - vertices[vertexIndex + 1]).normalized, (float)verticesLooped / verticesToLoop, textureRepeations * verticesToLoop / (vertices.Count - addedVertices - 1));
            }

            // Extra meshes
            if (vertexIndex > 0)
            {
                float progress = (float)(verticesLooped - extraMeshIndexOffset) / (verticesToLoop - extraMeshIndexOffset);
                leftExtraMeshes = AddTrianglesToSegmentExtraMeshes(intersection, leftExtraMeshes, vertices, vertexIndex, progress, verticesLooped, verticesToLoop * distanceBetweenVertices);
            }

            verticesLooped += 2;
            vertexIndex += 2;
        }

        AssignExtraMeshes(intersection, leftExtraMeshes);
    }

    private static void CreateConnectionSectionsTriangles(Intersection intersection, List<Vector3> vertices, ref List<Vector3> connectionSectionsVertices, ref List<int> connectionSectionsTriangles, ref List<Vector2> connectionSectionsUvs, ref List<Vector2> connectionSectionsUvs2, int startIndex, int endIndex, int addedVertices)
    {
        float textureRepeations = Mathf.PI * intersection.roundaboutRadius * intersection.textureTilingY * 0.5f;
        float indexProgress = (float)endIndex / (vertices.Count - 2 - addedVertices);
        float uvOffset = (indexProgress * textureRepeations) % 1;

        int vertexIndex = endIndex;
        int verticesLooped = 0;
        int verticesToLoop = 0;

        if (startIndex > endIndex)
        {
            verticesToLoop = startIndex - endIndex;
        }
        else
        {
            verticesToLoop += vertices.Count - 2 - endIndex - addedVertices;
            verticesToLoop += startIndex;
        }

        textureRepeations *= ((float)verticesToLoop / (vertices.Count - 2 - addedVertices));
        startIndex += 2;

        while (vertexIndex != startIndex && connectionSectionsTriangles.Count < 10000)
        {
            if (vertexIndex > vertices.Count - 4 - addedVertices)
            {
                vertexIndex = 0;
            }

            // Vertices
            connectionSectionsVertices.Add(vertices[vertexIndex] + new Vector3(0, 0.01f, 0));
            connectionSectionsVertices.Add(vertices[vertexIndex + 1] + new Vector3(0, 0.01f, 0));

            connectionSectionsUvs.Add(new Vector2(0, (verticesLooped / (float)verticesToLoop) * textureRepeations + uvOffset));
            connectionSectionsUvs.Add(new Vector2(1, (verticesLooped / (float)verticesToLoop) * textureRepeations + uvOffset));
            connectionSectionsUvs2.Add(Vector2.one);
            connectionSectionsUvs2.Add(Vector2.one);

            // Triangles
            if (vertexIndex != endIndex)
            {
                connectionSectionsTriangles.Add(connectionSectionsVertices.Count - 2);
                connectionSectionsTriangles.Add(connectionSectionsVertices.Count);
                connectionSectionsTriangles.Add(connectionSectionsVertices.Count - 1);

                connectionSectionsTriangles.Add(connectionSectionsVertices.Count);
                connectionSectionsTriangles.Add(connectionSectionsVertices.Count + 1);
                connectionSectionsTriangles.Add(connectionSectionsVertices.Count - 1);
            }

            verticesLooped += 2;
            vertexIndex += 2;
        }
    }

    private static void CreateRoadConnections(Intersection intersection, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs, ref List<Vector2> uvs2, ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, ref List<Vector2> bridgeUvs, ref List<int> connectionTriangles, ref List<int> nearestLeftPoints, ref List<int> nearestRightPoints, ref int addedVertices)
    {
        int addedBridgeVertices = 0;

        for (int i = 0; i < intersection.connections.Count; i++)
        {
            Vector3 forward = Misc.CalculateLeft(intersection.connections[i].rightPoint - intersection.connections[i].leftPoint);
            Vector3 leftPoint = CalculateNearestIntersectionPoint(intersection, intersection.connections[i].leftPoint, forward);
            Vector3 rightPoint = CalculateNearestIntersectionPoint(intersection, intersection.connections[i].rightPoint, forward);

            nearestLeftPoints.Add(0);
            nearestRightPoints.Add(0);
            FindNearestPoints(intersection, vertices, ref nearestLeftPoints, ref nearestRightPoints, leftPoint, rightPoint, i);

            if (nearestRightPoints[i] < 0)
            {
                nearestRightPoints[i] += vertices.Count;
            }

            if (nearestRightPoints[i] > vertices.Count - 2)
            {
                nearestRightPoints[i] -= vertices.Count - 3;
            }

            Vector3 centerPoint = (vertices[nearestLeftPoints[i]] + vertices[nearestLeftPoints[i] + 1] + vertices[nearestRightPoints[i]] + vertices[nearestRightPoints[i] + 1] + intersection.connections[i].lastPoint - intersection.transform.position) / 5;
            centerPoint.y = intersection.yOffset;

            // Set curve points
            int previousIndex = nearestLeftPoints[i] - 2;
            if (previousIndex < 0)
            {
                previousIndex = vertices.Count - addedVertices - 1;
            }

            int nextIndex = nearestRightPoints[i] + 2;
            if (nextIndex > vertices.Count - addedVertices - 1)
            {
                nextIndex = 2;
            }

            intersection.connections[i].defaultCurvePoint = intersection.RecalculateDefaultRoundaboutCurvePoints(intersection.connections[i].leftPoint, Misc.CalculateLeft(intersection.connections[i].rightPoint - intersection.connections[i].leftPoint), vertices[nearestLeftPoints[i]] + intersection.transform.position, (vertices[previousIndex] - vertices[nearestLeftPoints[i]]).normalized);
            intersection.connections[i].defaultCurvePoint2 = intersection.RecalculateDefaultRoundaboutCurvePoints(intersection.connections[i].rightPoint, Misc.CalculateLeft(intersection.connections[i].rightPoint - intersection.connections[i].leftPoint), vertices[nearestRightPoints[i]] + intersection.transform.position, (vertices[nextIndex] - vertices[nearestRightPoints[i]]).normalized);

            if (intersection.connections[i].curvePoint == null)
            {
                intersection.connections[i].curvePoint = intersection.connections[i].defaultCurvePoint;
            }

            if (intersection.connections[i].curvePoint2 == null)
            {
                intersection.connections[i].curvePoint2 = intersection.connections[i].defaultCurvePoint2;
            }

            List<RoundaboutExtraMesh> leftExtraMeshes = new List<RoundaboutExtraMesh>();
            List<RoundaboutExtraMesh> rightExtraMeshes = new List<RoundaboutExtraMesh>();
            AddConnectionExtraMeshes(intersection, ref leftExtraMeshes, ref rightExtraMeshes, i);

            int actualSegments = 0;
            AddNewRoadConnectionVertices(intersection, ref vertices, ref uvs, ref uvs2, ref bridgeVertices, ref bridgeTriangles, ref bridgeUvs, i, centerPoint, nearestLeftPoints, nearestRightPoints, ref addedVertices, leftExtraMeshes, rightExtraMeshes, ref actualSegments, ref addedBridgeVertices);
            connectionTriangles.AddRange(AddConnectionTriangles(vertices, triangles, actualSegments));
            AssignExtraMeshes(intersection, leftExtraMeshes);
            AssignExtraMeshes(intersection, rightExtraMeshes);
        }
    }

    private static void FindNearestPoints(Intersection intersection, List<Vector3> vertices, ref List<int> nearestLeftPoints, ref List<int> nearestRightPoints, Vector3 leftPoint, Vector3 rightPoint, int i)
    {
        float nearestLeftDistance = float.MaxValue;
        float nearestRightDistance = float.MaxValue;

        for (int j = 0; j < vertices.Count; j += 2)
        {
            float currentDistance = Vector3.Distance(leftPoint, vertices[j] + intersection.transform.position);

            if (currentDistance < nearestLeftDistance)
            {
                nearestLeftDistance = currentDistance;
                nearestLeftPoints[i] = j;
            }

            currentDistance = Vector3.Distance(rightPoint, vertices[j] + intersection.transform.position);

            if (currentDistance < nearestRightDistance)
            {
                nearestRightDistance = currentDistance;
                nearestRightPoints[i] = j;
            }
        }

        nearestLeftPoints[i] += 2 * intersection.settings.FindProperty("roundaboutConnectionIndexOffset").intValue;
        nearestRightPoints[i] -= 2 * intersection.settings.FindProperty("roundaboutConnectionIndexOffset").intValue;
    }

    private static void AddConnectionExtraMeshes(Intersection intersection, ref List<RoundaboutExtraMesh> leftExtraMeshes, ref List<RoundaboutExtraMesh> rightExtraMeshes, int i)
    {
        float startOffsetLeft = 0;
        float endOffsetLeft = 0;
        float yOffsetLeft = 0;

        float startOffsetRight = 0;
        float endOffsetRight = 0;
        float yOffsetRight = 0;

        for (int j = 0; j < intersection.extraMeshes.Count; j++)
        {
            if (intersection.extraMeshes[j].index > 0)
            {
                if ((intersection.extraMeshes[j].index - 1) % 3 == 1 && (intersection.extraMeshes[j].index - 2) / 3 == i)
                {
                    if (intersection.extraMeshes[j].startWidth == 0 && intersection.extraMeshes[j].endWidth == 0 && intersection.extraMeshes[j].yOffset == 0)
                    {
                        intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                        intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;
                    }
                    else
                    {
                        if (leftExtraMeshes.Count > 0)
                        {
                            startOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.startWidth;
                            endOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.endWidth;
                            yOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.yOffset;
                        }

                        leftExtraMeshes.Add(new RoundaboutExtraMesh(intersection.extraMeshes[j], j, startOffsetLeft, endOffsetLeft, yOffsetLeft));
                    }
                }
                else if ((intersection.extraMeshes[j].index - 1) % 3 == 2 && (intersection.extraMeshes[j].index - 3) / 3 == i)
                {
                    if (intersection.extraMeshes[j].startWidth == 0 && intersection.extraMeshes[j].endWidth == 0 && intersection.extraMeshes[j].yOffset == 0)
                    {
                        intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                        intersection.transform.GetChild(0).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;
                    }
                    else
                    {
                        if (rightExtraMeshes.Count > 0)
                        {
                            startOffsetRight += rightExtraMeshes[rightExtraMeshes.Count - 1].extraMesh.startWidth;
                            endOffsetRight += rightExtraMeshes[rightExtraMeshes.Count - 1].extraMesh.endWidth;
                            yOffsetRight += rightExtraMeshes[rightExtraMeshes.Count - 1].extraMesh.yOffset;
                        }

                        rightExtraMeshes.Add(new RoundaboutExtraMesh(intersection.extraMeshes[j], j, startOffsetRight, endOffsetRight, yOffsetRight));
                    }
                }
            }
        }
    }

    private static void AddNewRoadConnectionVertices(Intersection intersection, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<Vector2> uvs2, ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, ref List<Vector2> bridgeUvs, int i, Vector3 centerPoint, List<int> nearestLeftPoints, List<int> nearestRightPoints, ref int addedVertices, List<RoundaboutExtraMesh> leftExtraMeshes, List<RoundaboutExtraMesh> rightExtraMeshes, ref int actualSegments, ref int addedBridgeVertices)
    {
        int segments = (int)Mathf.Max(3, intersection.settings.FindProperty("resolution").floatValue * intersection.resolutionMultiplier * Vector3.Distance(intersection.connections[i].lastPoint, centerPoint + intersection.transform.position - new Vector3(0, intersection.yOffset, 0)) * 10);
        float distancePerSegment = 1f / segments;
        float degreesStartInner = Quaternion.LookRotation(vertices[nearestLeftPoints[i] + 1]).eulerAngles.y;
        float degreesEndInner = Quaternion.LookRotation(vertices[nearestRightPoints[i] + 1]).eulerAngles.y;

        float leftStartWidth = 0;
        float leftEndWidth = 0;
        float rightStartWidth = 0;
        float rightEndWidth = 0;
        float innerStartWidth = 0;
        float innerEndWidth = 0;

        for (int j = 0; j < intersection.extraMeshes.Count; j++)
        {
            if (intersection.extraMeshes[j].index == 0)
            {
                innerStartWidth += intersection.extraMeshes[j].startWidth;
                innerEndWidth += intersection.extraMeshes[j].endWidth;
            }
            else if ((intersection.extraMeshes[j].index - 1) % 3 == 1 && (intersection.extraMeshes[j].index - 2) / 3 == i)
            {
                leftStartWidth += intersection.extraMeshes[j].startWidth;
                leftEndWidth += intersection.extraMeshes[j].endWidth;
            }
            else if ((intersection.extraMeshes[j].index - 1) % 3 == 2 && (intersection.extraMeshes[j].index - 3) / 3 == i)
            {
                rightStartWidth = intersection.extraMeshes[j].startWidth;
                rightEndWidth = intersection.extraMeshes[j].endWidth;
            }
        }

        // Bridges
        List<Vector3> bridgeVerticesLeft = new List<Vector3>();
        List<int> bridgeTrianglesLeft = new List<int>();
        List<Vector2> bridgeUvsLeft = new List<Vector2>();
        List<Vector3> bridgeVerticesRight = new List<Vector3>();
        List<int> bridgeTrianglesRight = new List<int>();
        List<Vector2> bridgeUvsRight = new List<Vector2>();
        List<Vector3> bridgeVerticesInner = new List<Vector3>();
        List<int> bridgeTrianglesInner = new List<int>();
        List<Vector2> bridgeUvsInner = new List<Vector2>();

        int bridgeVerticesAdded = 0;
        int segmentsAdded = 0;

        for (float f = 0; f < 1 + distancePerSegment; f += distancePerSegment)
        {
            segmentsAdded += 1;
        }

        for (float f = 0; f < 1 + distancePerSegment; f += distancePerSegment)
        {
            float modifiedF = f;
            actualSegments += 1;

            if (modifiedF > 0.5f && modifiedF < 0.5f + distancePerSegment)
            {
                modifiedF = 0.5f;
            }
            else if (modifiedF > 1f)
            {
                modifiedF = 1f;
            }

            // Left, right and inner
            vertices.Add(Misc.Lerp3CenterHeight(intersection.connections[i].leftPoint - intersection.transform.position + new Vector3(0, intersection.yOffset, 0), intersection.connections[i].curvePoint - intersection.transform.position, vertices[nearestLeftPoints[i]], modifiedF));
            vertices.Add(Misc.Lerp3CenterHeight(intersection.connections[i].rightPoint - intersection.transform.position + new Vector3(0, intersection.yOffset, 0), intersection.connections[i].curvePoint2 - intersection.transform.position, vertices[nearestRightPoints[i]], modifiedF));

            Vector3 lerpedPoint = Vector3.Lerp(vertices[nearestLeftPoints[i] + 1], vertices[nearestRightPoints[i] + 1], modifiedF);
            vertices.Add(Quaternion.Euler(new Vector3(0, Quaternion.LookRotation(lerpedPoint).eulerAngles.y, 0)) * Vector3.forward * (intersection.roundaboutRadius - intersection.roundaboutWidth));

            vertices[vertices.Count - 1] += new Vector3(0, intersection.yOffset, 0);
            vertices[vertices.Count - 2] = new Vector3(vertices[vertices.Count - 2].x, intersection.yOffset, vertices[vertices.Count - 2].z);
            vertices[vertices.Count - 3] = new Vector3(vertices[vertices.Count - 3].x, intersection.yOffset, vertices[vertices.Count - 3].z);

            if (modifiedF < 0.5f)
            {
                vertices.Add(Vector3.Lerp(intersection.connections[i].lastPoint + new Vector3(0, intersection.yOffset, 0) - intersection.transform.position, centerPoint, modifiedF * 2));
                vertices.Add(Vector3.Lerp(intersection.connections[i].lastPoint + new Vector3(0, intersection.yOffset, 0) - intersection.transform.position, centerPoint, modifiedF * 2));
                vertices.Add(Vector3.Lerp(Misc.GetCenter(vertices[nearestLeftPoints[i]], vertices[nearestLeftPoints[i] + 1]), centerPoint, modifiedF * 2));
            }
            else
            {
                vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestLeftPoints[i]], vertices[nearestLeftPoints[i] + 1]), (modifiedF - 0.5f) * 2));
                vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestRightPoints[i]], vertices[nearestRightPoints[i] + 1]), (modifiedF - 0.5f) * 2));
                vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestRightPoints[i]], vertices[nearestRightPoints[i] + 1]), (modifiedF - 0.5f) * 2));
            }

            // Bridges
            if (intersection.generateBridge == true)
            {
                float width = Vector3.Distance(vertices[vertices.Count - 6], vertices[vertices.Count - 3]) + Mathf.Lerp(leftStartWidth, leftEndWidth, modifiedF);
                AddRoadConnectionBridgeVertices(intersection, ref bridgeVerticesLeft, ref bridgeTrianglesLeft, ref bridgeUvsLeft, vertices[vertices.Count - 3], (vertices[vertices.Count - 6] - vertices[vertices.Count - 3]).normalized, width, modifiedF, addedBridgeVertices);

                width = Vector3.Distance(vertices[vertices.Count - 5], vertices[vertices.Count - 2]) + Mathf.Lerp(rightStartWidth, rightEndWidth, modifiedF);
                AddRoadConnectionBridgeVertices(intersection, ref bridgeVerticesRight, ref bridgeTrianglesRight, ref bridgeUvsRight, vertices[vertices.Count - 2], (vertices[vertices.Count - 5] - vertices[vertices.Count - 2]).normalized, width, modifiedF, segmentsAdded * 6 + addedBridgeVertices, true);

                width = Vector3.Distance(vertices[vertices.Count - 4], vertices[vertices.Count - 1]) + Mathf.Lerp(innerStartWidth, innerEndWidth, modifiedF);
                AddRoadConnectionBridgeVertices(intersection, ref bridgeVerticesInner, ref bridgeTrianglesInner, ref bridgeUvsInner, vertices[vertices.Count - 1], (vertices[vertices.Count - 4] - vertices[vertices.Count - 1]).normalized, width, modifiedF, segmentsAdded * 12 + addedBridgeVertices);

                bridgeVerticesAdded += 18;
            }

            vertices[vertices.Count - 2] = new Vector3(vertices[vertices.Count - 2].x, intersection.yOffset, vertices[vertices.Count - 2].z);
            vertices[vertices.Count - 3] = new Vector3(vertices[vertices.Count - 3].x, intersection.yOffset, vertices[vertices.Count - 3].z);

            AddTrianglesToConnectionExtraMeshes(leftExtraMeshes, rightExtraMeshes, modifiedF, vertices);
            addedVertices += 6;

            if (intersection.stretchTexture == true)
            {
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(1, modifiedF));
                uvs.Add(new Vector2(1, modifiedF));
                uvs.Add(new Vector2(1, modifiedF));
            }
            else
            {
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(0, modifiedF));
                uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 3], vertices[vertices.Count - 6]) / intersection.textureScale, modifiedF));
                uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 2], vertices[vertices.Count - 5]) / intersection.textureScale, modifiedF));
                uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 1], vertices[vertices.Count - 4]) / intersection.textureScale, modifiedF));
            }

            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
        }

        addedBridgeVertices += bridgeVerticesAdded;

        // Combine bridge meshes
        if (intersection.generateBridge == true)
        {
            bridgeVertices.AddRange(bridgeVerticesLeft);
            bridgeVertices.AddRange(bridgeVerticesRight);
            bridgeVertices.AddRange(bridgeVerticesInner);
            bridgeTriangles.AddRange(bridgeTrianglesLeft);
            bridgeTriangles.AddRange(bridgeTrianglesRight);
            bridgeTriangles.AddRange(bridgeTrianglesInner);
            bridgeUvs.AddRange(bridgeUvsLeft);
            bridgeUvs.AddRange(bridgeUvsRight);
            bridgeUvs.AddRange(bridgeUvsInner);
        }
    }

    private static void AddRoadConnectionBridgeVertices(Intersection intersection, ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, ref List<Vector2> bridgeUvs, Vector3 innerPoint, Vector3 outwards, float width, float modifiedF, int addedVertices, bool flipNormals = false)
    {
        // |_   
        //   \_

        bridgeVertices.Add(innerPoint + outwards * (intersection.bridgeSettings.extraWidth + width) - new Vector3(0, intersection.yOffset, 0));
        bridgeVertices.Add(innerPoint + outwards * (intersection.bridgeSettings.extraWidth + width) - new Vector3(0, intersection.yOffset + intersection.bridgeSettings.yOffsetFirstStep, 0));
        bridgeVertices.Add(innerPoint + outwards * (intersection.bridgeSettings.extraWidth + width * intersection.bridgeSettings.widthPercentageFirstStep) - new Vector3(0, intersection.yOffset + intersection.bridgeSettings.yOffsetFirstStep, 0));
        bridgeVertices.Add(innerPoint + outwards * (intersection.bridgeSettings.extraWidth + width * intersection.bridgeSettings.widthPercentageFirstStep * intersection.bridgeSettings.widthPercentageSecondStep) - new Vector3(0, intersection.yOffset + intersection.bridgeSettings.yOffsetFirstStep + intersection.bridgeSettings.yOffsetSecondStep, 0));
        bridgeVertices.Add(innerPoint - new Vector3(0, intersection.yOffset + intersection.bridgeSettings.yOffsetFirstStep + intersection.bridgeSettings.yOffsetSecondStep, 0));
        bridgeVertices.Add(innerPoint - new Vector3(0, intersection.yOffset, 0));

        bridgeUvs.Add(new Vector2(0, modifiedF));
        bridgeUvs.Add(new Vector2(1, modifiedF));
        bridgeUvs.Add(new Vector2(0, modifiedF));
        bridgeUvs.Add(new Vector2(1, modifiedF));
        bridgeUvs.Add(new Vector2(0, modifiedF));
        bridgeUvs.Add(new Vector2(1, modifiedF));

        if (modifiedF > 0)
        {
            for (int j = 0; j < 4; j++)
            {
                if (flipNormals == false)
                {
                    bridgeTriangles.Add(bridgeVertices.Count - 6 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 11 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 5 + j + addedVertices);

                    bridgeTriangles.Add(bridgeVertices.Count - 6 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 12 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 11 + j + addedVertices);
                }
                else
                {
                    bridgeTriangles.Add(bridgeVertices.Count - 6 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 5 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 11 + j + addedVertices);

                    bridgeTriangles.Add(bridgeVertices.Count - 6 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 11 + j + addedVertices);
                    bridgeTriangles.Add(bridgeVertices.Count - 12 + j + addedVertices);
                }
            }

            // Top part
            if (flipNormals == false)
            {
                bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 7 + addedVertices);

                bridgeTriangles.Add(bridgeVertices.Count - 12 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 7 + addedVertices);
            }
            else
            {
                bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 7 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);

                bridgeTriangles.Add(bridgeVertices.Count - 12 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 7 + addedVertices);
                bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
            }

            if (modifiedF == 1)
            {
                // End cap
                if (flipNormals == false)
                {
                    GenerateCapFlipped(ref bridgeVertices, ref bridgeTriangles, addedVertices);
                }
                else
                {
                    GenerateCap(ref bridgeVertices, ref bridgeTriangles, addedVertices);
                }
            }
        }
        else
        {
            // Start cap
            if (flipNormals == false)
            {
                GenerateCap(ref bridgeVertices, ref bridgeTriangles, addedVertices);
            }
            else
            {
                GenerateCapFlipped(ref bridgeVertices, ref bridgeTriangles, addedVertices);
            }
        }
    }

    private static void GenerateCap(ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, int addedVertices)
    {
        bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 5 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 5 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 4 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 4 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 3 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 2 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 3 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
    }

    private static void GenerateCapFlipped(ref List<Vector3> bridgeVertices, ref List<int> bridgeTriangles, int addedVertices)
    {
        bridgeTriangles.Add(bridgeVertices.Count - 6 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 5 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 5 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 4 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 4 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 3 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);

        bridgeTriangles.Add(bridgeVertices.Count - 2 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 1 + addedVertices);
        bridgeTriangles.Add(bridgeVertices.Count - 3 + addedVertices);
    }

    private static void CreateRoundaboutVertices(Intersection intersection, ref List<Vector3> vertices, ref List<Vector2> uvs, ref List<Vector2> uvs2)
    {
        int segments = (int)Mathf.Max(6, intersection.settings.FindProperty("resolution").floatValue * intersection.resolutionMultiplier * intersection.roundaboutRadius * 30f);
        float degreesPerSegment = 1f / segments;
        float textureRepeations = Mathf.PI * intersection.roundaboutRadius * intersection.textureTilingY * 0.5f;

        for (float f = 0; f < 1 + degreesPerSegment; f += degreesPerSegment)
        {
            float modifiedF = f;
            if (f > 1)
            {
                modifiedF = 1;
            }

            vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (intersection.roundaboutRadius + intersection.roundaboutWidth));
            vertices[vertices.Count - 1] += new Vector3(0, intersection.yOffset, 0);
            vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (intersection.roundaboutRadius - intersection.roundaboutWidth));
            vertices[vertices.Count - 1] += new Vector3(0, intersection.yOffset, 0);

            uvs.Add(new Vector2(0, modifiedF * textureRepeations));
            uvs.Add(new Vector2(1, modifiedF * textureRepeations));
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
        }
    }

    private static void AddTrianglesToConnectionExtraMeshes(List<RoundaboutExtraMesh> leftExtraMeshes, List<RoundaboutExtraMesh> rightExtraMeshes, float modifiedF, List<Vector3> vertices)
    {
        for (int j = 0; j < leftExtraMeshes.Count; j++)
        {
            leftExtraMeshes[j].vertices.Add(vertices[vertices.Count - 6] + (vertices[vertices.Count - 6] - vertices[vertices.Count - 3]).normalized * Mathf.Lerp(leftExtraMeshes[j].startOffset, leftExtraMeshes[j].endOffset, modifiedF));
            leftExtraMeshes[j].vertices.Add(vertices[vertices.Count - 6] + (vertices[vertices.Count - 6] - vertices[vertices.Count - 3]).normalized * Mathf.Lerp(leftExtraMeshes[j].startOffset + leftExtraMeshes[j].extraMesh.startWidth, leftExtraMeshes[j].endOffset + leftExtraMeshes[j].extraMesh.endWidth, modifiedF));
            leftExtraMeshes[j].vertices[leftExtraMeshes[j].vertices.Count - 1] += new Vector3(0, leftExtraMeshes[j].extraMesh.yOffset + leftExtraMeshes[j].yOffset, 0);
            leftExtraMeshes[j].vertices[leftExtraMeshes[j].vertices.Count - 2] += new Vector3(0, leftExtraMeshes[j].yOffset, 0);

            if (leftExtraMeshes[j].extraMesh.flipped == true)
            {
                leftExtraMeshes[j].uvs.Add(new Vector2(1, modifiedF));
                leftExtraMeshes[j].uvs.Add(new Vector2(0, modifiedF));
            }
            else
            {
                leftExtraMeshes[j].uvs.Add(new Vector2(0, modifiedF));
                leftExtraMeshes[j].uvs.Add(new Vector2(1, modifiedF));
            }

            leftExtraMeshes[j].uvs2.Add(Vector2.one);
            leftExtraMeshes[j].uvs2.Add(Vector2.one);

            if (modifiedF > 0)
            {
                AddTrianglesToConnectionExtraMesh(leftExtraMeshes[j]);
            }
        }

        for (int j = 0; j < rightExtraMeshes.Count; j++)
        {
            rightExtraMeshes[j].vertices.Add(vertices[vertices.Count - 5] + (vertices[vertices.Count - 5] - vertices[vertices.Count - 2]).normalized * Mathf.Lerp(rightExtraMeshes[j].extraMesh.startWidth + rightExtraMeshes[j].startOffset, rightExtraMeshes[j].extraMesh.endWidth + rightExtraMeshes[j].endOffset, modifiedF));
            rightExtraMeshes[j].vertices.Add(vertices[vertices.Count - 5] + (vertices[vertices.Count - 5] - vertices[vertices.Count - 2]).normalized * Mathf.Lerp(rightExtraMeshes[j].startOffset, rightExtraMeshes[j].endOffset, modifiedF));
            rightExtraMeshes[j].vertices[rightExtraMeshes[j].vertices.Count - 2] += new Vector3(0, rightExtraMeshes[j].yOffset + rightExtraMeshes[j].extraMesh.yOffset, 0);
            rightExtraMeshes[j].vertices[rightExtraMeshes[j].vertices.Count - 1] += new Vector3(0, rightExtraMeshes[j].yOffset, 0);

            if (rightExtraMeshes[j].extraMesh.flipped == true)
            {
                rightExtraMeshes[j].uvs.Add(new Vector2(0, modifiedF));
                rightExtraMeshes[j].uvs.Add(new Vector2(1, modifiedF));
            }
            else
            {
                rightExtraMeshes[j].uvs.Add(new Vector2(1, modifiedF));
                rightExtraMeshes[j].uvs.Add(new Vector2(0, modifiedF));
            }

            rightExtraMeshes[j].uvs2.Add(Vector2.one);
            rightExtraMeshes[j].uvs2.Add(Vector2.one);

            if (modifiedF > 0)
            {
                AddTrianglesToConnectionExtraMesh(rightExtraMeshes[j]);
            }
        }
    }

    private static void AddTrianglesToConnectionExtraMesh(RoundaboutExtraMesh extraMesh)
    {
        extraMesh.triangles.Add(extraMesh.vertices.Count - 1);
        extraMesh.triangles.Add(extraMesh.vertices.Count - 2);
        extraMesh.triangles.Add(extraMesh.vertices.Count - 3);

        extraMesh.triangles.Add(extraMesh.vertices.Count - 2);
        extraMesh.triangles.Add(extraMesh.vertices.Count - 4);
        extraMesh.triangles.Add(extraMesh.vertices.Count - 3);
    }

    private static void CreateCenterExtraMeshes(Intersection intersection)
    {
        List<ExtraMesh> centerExtraMeshes = new List<ExtraMesh>();
        List<int> indexes = new List<int>();
        int segments = (int)Mathf.Max(6, intersection.settings.FindProperty("resolution").floatValue * intersection.resolutionMultiplier * intersection.roundaboutRadius * 30f);

        float degreesPerSegment = 1f / segments;
        float textureRepeations = Mathf.PI * intersection.roundaboutRadius * intersection.textureTilingY * 0.5f;

        for (int i = 0; i < intersection.extraMeshes.Count; i++)
        {
            if (intersection.extraMeshes[i].index == 0)
            {
                centerExtraMeshes.Add(intersection.extraMeshes[i]);
                indexes.Add(i);
            }
        }

        float currentOffset = 0;
        float currentYOffset = intersection.yOffset;

        for (int i = 0; i < centerExtraMeshes.Count; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> uvs2 = new List<Vector2>();
            int vertexIndex = 0;

            if (centerExtraMeshes[i].startWidth > 0 || centerExtraMeshes[i].yOffset != 0)
            {
                for (float f = 0; f < 1 + degreesPerSegment; f += degreesPerSegment)
                {
                    float modifiedF = f;

                    if (f > 1)
                    {
                        modifiedF = 1;
                    }

                    vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (intersection.roundaboutRadius - intersection.roundaboutWidth - currentOffset));
                    vertices[vertices.Count - 1] += new Vector3(0, currentYOffset, 0);
                    vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (intersection.roundaboutRadius - intersection.roundaboutWidth - currentOffset - centerExtraMeshes[i].startWidth));
                    vertices[vertices.Count - 1] += new Vector3(0, currentYOffset + centerExtraMeshes[i].yOffset, 0);

                    if (centerExtraMeshes[i].flipped == false)
                    {
                        uvs.Add(new Vector2(0, modifiedF * textureRepeations));
                        uvs.Add(new Vector2(1, modifiedF * textureRepeations));
                    }
                    else
                    {
                        uvs.Add(new Vector2(1, modifiedF * textureRepeations));
                        uvs.Add(new Vector2(0, modifiedF * textureRepeations));
                    }

                    uvs2.Add(Vector2.one);
                    uvs2.Add(Vector2.one);

                    if (vertexIndex > 0)
                    {
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex - 1);
                        triangles.Add(vertexIndex - 2);

                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex - 1);
                    }

                    vertexIndex += 2;
                }

                // Update offsets
                currentOffset += centerExtraMeshes[i].startWidth;
                currentYOffset += centerExtraMeshes[i].yOffset;

                // Assign mesh
                AssignCenterExtraMesh(intersection, vertices, triangles, uvs, uvs2, centerExtraMeshes, i, indexes);
            }
            else
            {
                intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshFilter>().sharedMesh = null;
                intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshCollider>().sharedMesh = null;
            }
        }
    }

    private static void AssignCenterExtraMesh(Intersection intersection, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector2> uvs2, List<ExtraMesh> centerExtraMeshes, int i, List<int> indexes)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.RecalculateNormals();

        intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshFilter>().sharedMesh = mesh;
        intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshCollider>().sharedMesh = mesh;
        intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshCollider>().sharedMaterial = centerExtraMeshes[i].physicMaterial;

        if (centerExtraMeshes[i].overlayMaterial == null)
        {
            intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { centerExtraMeshes[i].baseMaterial };
        }
        else
        {
            intersection.transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { centerExtraMeshes[i].baseMaterial, centerExtraMeshes[i].overlayMaterial };
        }
    }

    private static List<RoundaboutExtraMesh> AddTrianglesToSegmentExtraMeshes(Intersection intersection, List<RoundaboutExtraMesh> extraMeshes, List<Vector3> vertices, int vertexIndex, float progress, int verticesLooped, float totalDistance)
    {
        for (int j = 0; j < extraMeshes.Count; j++)
        {
            Vector3 left = (vertices[vertexIndex] - vertices[vertexIndex + 1]).normalized;

            extraMeshes[j].vertices.Add(vertices[vertexIndex] + left * Mathf.Lerp(extraMeshes[j].startOffset, extraMeshes[j].endOffset, progress));
            extraMeshes[j].vertices.Add(vertices[vertexIndex] + left * Mathf.Lerp(extraMeshes[j].extraMesh.startWidth + extraMeshes[j].startOffset, extraMeshes[j].extraMesh.endWidth + extraMeshes[j].endOffset, progress));
            extraMeshes[j].vertices[extraMeshes[j].vertices.Count - 2] += new Vector3(0, extraMeshes[j].yOffset, 0);
            extraMeshes[j].vertices[extraMeshes[j].vertices.Count - 1] += new Vector3(0, extraMeshes[j].extraMesh.yOffset + extraMeshes[j].yOffset, 0);

            if (extraMeshes[j].extraMesh.flipped == false)
            {
                extraMeshes[j].uvs.Add(new Vector3(0, progress * intersection.textureTilingY * totalDistance * 0.13f));
                extraMeshes[j].uvs.Add(new Vector3(1, progress * intersection.textureTilingY * totalDistance * 0.13f));
            }
            else
            {
                extraMeshes[j].uvs.Add(new Vector3(1, progress * intersection.textureTilingY * totalDistance * 0.13f));
                extraMeshes[j].uvs.Add(new Vector3(0, progress * intersection.textureTilingY * totalDistance * 0.13f));
            }

            extraMeshes[j].uvs2.Add(Vector2.one);
            extraMeshes[j].uvs2.Add(Vector2.one);

            if (progress > 0)
            {
                AddTrianglesToConnectionExtraMesh(extraMeshes[j]);
            }
        }

        return extraMeshes;
    }

    private static void AssignExtraMeshes(Intersection intersection, List<RoundaboutExtraMesh> extraMeshes)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = extraMeshes[i].vertices.ToArray();
            mesh.triangles = extraMeshes[i].triangles.ToArray();
            mesh.uv = extraMeshes[i].uvs.ToArray();
            mesh.uv2 = extraMeshes[i].uvs2.ToArray();
            mesh.RecalculateNormals();

            intersection.transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshFilter>().sharedMesh = mesh;
            intersection.transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshCollider>().sharedMesh = mesh;
            intersection.transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].extraMesh.physicMaterial;

            if (extraMeshes[i].extraMesh.overlayMaterial == null)
            {
                intersection.transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].extraMesh.baseMaterial };
            }
            else
            {
                intersection.transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].extraMesh.baseMaterial, extraMeshes[i].extraMesh.overlayMaterial };
            }
        }
    }

    private static void SetupRoundaboutMesh(Intersection intersection, List<Vector3> vertices, List<int> triangles, List<int> connectionTriangles, List<Vector3> connectionSectionVertices, List<int> connectionSectionsTriangles, List<Vector2> connectionSectionsUvs, List<Vector2> connectionSectionsUvs2, List<Vector2> uvs, List<Vector2> uvs2)
    {
        // Combine data
        for (int i = 0; i < connectionSectionsTriangles.Count; i++)
        {
            connectionSectionsTriangles[i] += vertices.Count - 2;
        }

        vertices.AddRange(connectionSectionVertices);
        uvs.AddRange(connectionSectionsUvs);
        uvs2.AddRange(connectionSectionsUvs2);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        int materialIndex = 0;

        List<Material> materials = new List<Material>();
        materials.Add(intersection.baseMaterial);
        mesh.SetTriangles(triangles, materialIndex);
        materialIndex += 1;

        if (intersection.overlayMaterial != null)
        {
            materials.Add(intersection.overlayMaterial);
            mesh.subMeshCount += 1;
            mesh.SetTriangles(triangles, materialIndex);
            materialIndex += 1;
        }

        materials.Add(intersection.connectionBaseMaterial);
        mesh.subMeshCount += 1;
        mesh.SetTriangles(connectionTriangles, materialIndex);
        materialIndex += 1;

        if (intersection.connectionOverlayMaterial != null)
        {
            materials.Add(intersection.connectionOverlayMaterial);
            mesh.subMeshCount += 1;
            mesh.SetTriangles(connectionTriangles, materialIndex);
            materialIndex += 1;
        }

        if (connectionSectionsTriangles.Count > 0)
        {
            materials.Add(intersection.connectionSectionMaterial);
            mesh.subMeshCount += 1;
            mesh.SetTriangles(connectionSectionsTriangles, materialIndex);
            materialIndex += 1;
        }

        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.RecalculateNormals();

        intersection.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        intersection.GetComponent<MeshFilter>().sharedMesh = mesh;
        intersection.GetComponent<MeshCollider>().sharedMesh = mesh;
        intersection.GetComponent<MeshCollider>().sharedMaterial = intersection.physicMaterial;
    }

    private static List<int> AddConnectionTriangles(List<Vector3> vertices, List<int> inputTriangles, int segments)
    {
        List<int> triangles = new List<int>(inputTriangles);

        for (int j = vertices.Count - segments * 6; j < vertices.Count - 7; j += 6)
        {
            // Left
            triangles.Add(j);
            triangles.Add(j + 6);
            triangles.Add(j + 3);

            triangles.Add(j + 3);
            triangles.Add(j + 6);
            triangles.Add(j + 9);

            // Right
            triangles.Add(j + 1);
            triangles.Add(j + 4);
            triangles.Add(j + 7);

            triangles.Add(j + 10);
            triangles.Add(j + 7);
            triangles.Add(j + 4);

            // Inner
            triangles.Add(j + 2);
            triangles.Add(j + 11);
            triangles.Add(j + 5);

            triangles.Add(j + 2);
            triangles.Add(j + 8);
            triangles.Add(j + 11);
        }

        return triangles;
    }

    private static Vector3 CalculateNearestIntersectionPoint(Intersection intersection, Vector3 originalPoint, Vector3 forward)
    {
        Vector3 point = originalPoint;

        for (float d = 0; d < 10; d += 0.1f)
        {
            point += forward * d;

            if (Vector3.Distance(originalPoint, point) > intersection.roundaboutRadius)
            {
                return originalPoint + forward * 2;
            }

            if (Vector3.Distance(point, intersection.transform.position) < intersection.roundaboutRadius + intersection.roundaboutWidth / 2)
            {
                return point;
            }
        }

        return originalPoint + forward * 2;
    }

    private static void SetupBridgeMesh(Intersection intersection, List<Vector3> bridgeVertices, List<int> bridgeTriangles, List<Vector2> bridgeUvs)
    {
        Transform bridge = intersection.transform.Find("Bridge");

        if (bridge != null)
        {
            GameObject.DestroyImmediate(bridge.gameObject);
        }

        if (intersection.generateBridge == true)
        {
            GameObject bridgeObject = new GameObject("Bridge");
            BridgeGeneration.CreateBridge(bridgeObject, intersection.transform, bridgeVertices.ToArray(), bridgeTriangles.ToArray(), bridgeUvs.ToArray(), null, intersection.bridgeSettings.bridgeMaterials, intersection.settings);
        }
    }

    private static void ResetNonCenterExtraMeshes(Intersection intersection)
    {
        for (int i = intersection.extraMeshes.Count - 1; i >= 0; i--)
        {
            if (intersection.extraMeshes[i].index != 0)
            {
                intersection.extraMeshes.RemoveAt(i);
                GameObject.DestroyImmediate(intersection.transform.GetChild(0).GetChild(i).gameObject);
            }
        }
    }

    private static void CreateExtraMeshesFromRoads(Intersection intersection)
    {
        List<float> currentWidths = new List<float>();
        List<float> currentYOffsets = new List<float>();
        List<float> lastWidths = new List<float>();
        List<float> lastYOffsets = new List<float>();

        // Generate first widths
        if (intersection.connections.Count > 0)
        {
            RoadSegment lastRoadSegment;
            if (intersection.connections.Count > 1)
            {
                lastRoadSegment = intersection.connections[intersection.connections.Count - 2].road.transform.parent.parent.GetComponent<RoadSegment>();
            }
            else
            {
                lastRoadSegment = intersection.connections[intersection.connections.Count - 1].road.transform.parent.parent.GetComponent<RoadSegment>();
            }

            RoadSegment roadSegment = intersection.connections[0].road.transform.parent.parent.GetComponent<RoadSegment>();

            for (int j = 0; j < roadSegment.extraMeshes.Count; j++)
            {
                if (roadSegment.extraMeshes[j].left == false && intersection.connections[0].road.name == "End Point")
                {
                    lastWidths.Add(roadSegment.extraMeshes[j].endWidth);
                    lastYOffsets.Add(roadSegment.extraMeshes[j].yOffset);
                }
                else if (roadSegment.extraMeshes[j].left == true && intersection.connections[0].road.name == "Start Point")
                {
                    lastWidths.Add(roadSegment.extraMeshes[j].startWidth);
                    lastYOffsets.Add(roadSegment.extraMeshes[j].yOffset);
                }
            }

            for (int i = intersection.connections.Count - 1; i >= 0; i--)
            {
                roadSegment = intersection.connections[i].road.transform.parent.parent.GetComponent<RoadSegment>();
                currentWidths.Clear();
                currentYOffsets.Clear();
                int addedExtraMeshes = 0;

                for (int j = 0; j < roadSegment.extraMeshes.Count; j++)
                {
                    if ((roadSegment.extraMeshes[j].left == true && intersection.connections[i].road.name == "End Point") || (roadSegment.extraMeshes[j].left == false && intersection.connections[i].road.name == "Start Point"))
                    {
                        if (intersection.connections[i].road.name == "Start Point")
                        {
                            if (addedExtraMeshes < lastWidths.Count)
                            {
                                if (roadSegment.extraMeshes[j].startWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0 || lastWidths[addedExtraMeshes] != 0 || lastYOffsets[addedExtraMeshes] != 0)
                                {
                                    intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 1, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].startWidth, lastWidths[addedExtraMeshes], roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                    intersection.CreateExtraMesh();
                                    addedExtraMeshes += 1;
                                }
                            }
                            else if (roadSegment.extraMeshes[j].startWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                            {
                                intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 1, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].startWidth, 0, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                intersection.CreateExtraMesh();
                                addedExtraMeshes += 1;
                            }

                            if (roadSegment.extraMeshes[j].startWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                            {
                                intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 2, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].startWidth, roadSegment.extraMeshes[j].startWidth, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                intersection.CreateExtraMesh();
                            }
                        }
                        else
                        {
                            if (addedExtraMeshes < lastWidths.Count)
                            {
                                if (roadSegment.extraMeshes[j].endWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0 || lastWidths[addedExtraMeshes] != 0 || lastYOffsets[addedExtraMeshes] != 0)
                                {
                                    intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 1, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].endWidth, lastWidths[addedExtraMeshes], roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                    intersection.CreateExtraMesh();
                                    addedExtraMeshes += 1;
                                }
                            }
                            else if (roadSegment.extraMeshes[j].endWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                            {
                                intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 1, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].endWidth, 0, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                intersection.CreateExtraMesh();
                                addedExtraMeshes += 1;
                            }

                            if (roadSegment.extraMeshes[j].endWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                            {
                                intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 2, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].endWidth, roadSegment.extraMeshes[j].endWidth, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                intersection.CreateExtraMesh();
                            }
                        }
                    }
                    else
                    {
                        if (intersection.connections[i].road.name == "Start Point")
                        {
                            if (roadSegment.extraMeshes[j].startWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                            {
                                intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 3, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].startWidth, roadSegment.extraMeshes[j].startWidth, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                                intersection.CreateExtraMesh();
                                currentWidths.Add(roadSegment.extraMeshes[j].startWidth);
                                currentYOffsets.Add(roadSegment.extraMeshes[j].yOffset);
                            }
                        }
                        else if (roadSegment.extraMeshes[j].endWidth != 0 || roadSegment.extraMeshes[j].yOffset != 0)
                        {
                            intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 3, roadSegment.extraMeshes[j].baseMaterial, roadSegment.extraMeshes[j].overlayMaterial, roadSegment.extraMeshes[j].physicMaterial, roadSegment.extraMeshes[j].endWidth, roadSegment.extraMeshes[j].endWidth, roadSegment.extraMeshes[j].flipped, roadSegment.extraMeshes[j].yOffset));
                            intersection.CreateExtraMesh();
                            currentWidths.Add(roadSegment.extraMeshes[j].endWidth);
                            currentYOffsets.Add(roadSegment.extraMeshes[j].yOffset);
                        }
                    }
                }

                for (int j = addedExtraMeshes; j < lastWidths.Count; j++)
                {
                    intersection.extraMeshes.Add(new ExtraMesh(true, i * 3 + 1, lastRoadSegment.extraMeshes[j].baseMaterial, lastRoadSegment.extraMeshes[j].overlayMaterial, lastRoadSegment.extraMeshes[j].physicMaterial, 0, lastWidths[j], roadSegment.extraMeshes[j].flipped, lastRoadSegment.extraMeshes[j].yOffset));
                    intersection.CreateExtraMesh();
                }

                lastRoadSegment = roadSegment;
                lastWidths = new List<float>(currentWidths);
                lastYOffsets = new List<float>(currentYOffsets);
            }
        }
    }

    public static void UpdateMaxRadius(Intersection intersection)
    {
        float nearest = float.MaxValue;

        for (int i = 0; i < intersection.connections.Count; i++)
        {
            float distance = Vector3.Distance(intersection.transform.position, intersection.connections[i].lastPoint);

            if (distance < nearest)
            {
                nearest = distance;
            }
        }

        intersection.maxRoundaboutRadius = nearest * 0.9f;
    }

}
#endif