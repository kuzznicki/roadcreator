﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RoadCreatorProjectSettings
{
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        SettingsProvider settingsProvider = new SettingsProvider("Project/RoadCreator", SettingsScope.Project)
        {
            label = "Road Creator",

            guiHandler = (searchContext) =>
            {
                SerializedObject settings = RoadCreatorSettings.GetSerializedSettings();

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("pointSize").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Point Size", settings.FindProperty("pointSize").floatValue));
                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    Transform[] objects = GameObject.FindObjectsOfType<Transform>();

                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (objects[i].name.Contains("Connection Point") || objects[i].name == "Start Point" || objects[i].name == "Control Point" || objects[i].name == "End Point")
                        {
                            objects[i].GetComponent<BoxCollider>().size = new Vector3(settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue);
                        }
                    }

                    UpdateSettings();
                }

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("resolution").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Resolution", settings.FindProperty("resolution").floatValue), 0.01f, 2f);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();

                    RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();
                    for (int i = 0; i < roads.Length; i++)
                    {
                        roads[i].CreateMesh();
                    }

                    Intersection[] intersections = GameObject.FindObjectsOfType<Intersection>();
                    for (int i = 0; i < intersections.Length; i++)
                    {
                        intersections[i].CreateMesh();
                    }
                }

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("hideNonEditableChildren").boolValue = EditorGUILayout.Toggle("Hide Non-editable Children", settings.FindProperty("hideNonEditableChildren").boolValue);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                }

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("roundaboutConnectionIndexOffset").intValue = Mathf.Clamp(EditorGUILayout.IntField("Roundabout Connection Index Offset", settings.FindProperty("roundaboutConnectionIndexOffset").intValue), 0, 10);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();

                    Intersection[] intersections = GameObject.FindObjectsOfType<Intersection>();
                    for (int i = 0; i < intersections.Length; i++)
                    {
                        if (intersections[i].roundaboutMode == true)
                        {
                            intersections[i].CreateMesh();
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(settings.FindProperty("pointShape"));

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                }

                GUIStyle guiStyle = new GUIStyle();
                guiStyle.fontStyle = FontStyle.Bold;

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUILayout.Label("Road Guidelines", guiStyle);
                settings.FindProperty("roadGuidelinesLength").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Length (each side)", settings.FindProperty("roadGuidelinesLength").floatValue), 0, 15);
                settings.FindProperty("roadGuidelinesDistance").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Display Distance", settings.FindProperty("roadGuidelinesDistance").floatValue), 1, 50);
                settings.FindProperty("roadGuidelinesSnapDistance").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Snap Distance", settings.FindProperty("roadGuidelinesSnapDistance").floatValue), 0.1f, 5);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                    RoadCreatorSettings.UpdateRoadGuidelines();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUILayout.Label("Defaults", guiStyle);
                settings.FindProperty("defaultLanes").intValue = Mathf.Clamp(EditorGUILayout.IntField("Default Lanes", settings.FindProperty("defaultLanes").intValue), 0, 10);
                settings.FindProperty("defaultBaseMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Base Material", settings.FindProperty("defaultBaseMaterial").objectReferenceValue, typeof(Material), false);
                settings.FindProperty("defaultRoadOverlayMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Road Overlay Material", settings.FindProperty("defaultRoadOverlayMaterial").objectReferenceValue, typeof(Material), false);
                settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Extra Mesh Overlay Material", settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue, typeof(Material), false);
                settings.FindProperty("defaultIntersectionOverlayMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Intersection Overlay Material", settings.FindProperty("defaultIntersectionOverlayMaterial").objectReferenceValue, typeof(Material), false);
                settings.FindProperty("defaultIntersectionMainRoadMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Intersection Main Road Material", settings.FindProperty("defaultIntersectionMainRoadMaterial").objectReferenceValue, typeof(Material), false);
                settings.FindProperty("defaultRoundaboutConnectionSectionsMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Default Roundabout Connection Sections Material", settings.FindProperty("defaultRoundaboutConnectionSectionsMaterial").objectReferenceValue, typeof(Material), false);
                EditorGUILayout.PropertyField(settings.FindProperty("defaultSimpleBridgeMaterials"), true);
                settings.FindProperty("defaultPillarPrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Pillar Prefab", settings.FindProperty("defaultPillarPrefab").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("defaultBridgePillarPrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Bridge Pillar Prefab", settings.FindProperty("defaultBridgePillarPrefab").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("defaultCustomBridgePrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Custom Bridge Prefab", settings.FindProperty("defaultCustomBridgePrefab").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("defaultPrefabLinePrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Prefab Line Prefab", settings.FindProperty("defaultPrefabLinePrefab").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("defaultPrefabLineStartPrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Prefab Line Start Prefab", settings.FindProperty("defaultPrefabLineStartPrefab").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("defaultPrefabLineEndPrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Default Prefab Line End Prefab", settings.FindProperty("defaultPrefabLineEndPrefab").objectReferenceValue, typeof(GameObject), false);

                if (GUILayout.Button("Reset Default Values"))
                {
                    settings.FindProperty("defaultLanes").intValue = 2;
                    settings.FindProperty("defaultBaseMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultRoadOverlayMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultIntersectionOverlayMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultIntersectionMainRoadMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultRoundaboutConnectionSectionsMaterial").objectReferenceValue = null;
                    settings.FindProperty("defaultSimpleBridgeMaterials").ClearArray();
                    settings.FindProperty("defaultPillarPrefab").objectReferenceValue = null;
                    settings.FindProperty("defaultBridgePillarPrefab").objectReferenceValue = null;
                    settings.FindProperty("defaultCustomBridgePrefab").objectReferenceValue = null;
                    settings.FindProperty("defaultPrefabLinePrefab").objectReferenceValue = null;
                    settings.FindProperty("defaultPrefabLineStartPrefab").objectReferenceValue = null;
                    settings.FindProperty("defaultPrefabLineEndPrefab").objectReferenceValue = null;
                }

                GUILayout.Space(20);
                GUILayout.Label("Turn Markings", guiStyle);
                settings.FindProperty("leftTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Left Turn Marking", settings.FindProperty("leftTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("forwardTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Forward Turn Marking", settings.FindProperty("forwardTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("rightTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Right Turn Marking", settings.FindProperty("rightTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("leftForwardTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Left And Forward Turn Marking", settings.FindProperty("leftForwardTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("rightForwardTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Right And Forward Turn Marking", settings.FindProperty("rightForwardTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("leftRightTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Left And Right Turn Marking", settings.FindProperty("leftRightTurnMarking").objectReferenceValue, typeof(GameObject), false);
                settings.FindProperty("leftRightForwardTurnMarking").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Left, Right And Forward Turn Marking", settings.FindProperty("leftRightForwardTurnMarking").objectReferenceValue, typeof(GameObject), false);

                if (GUILayout.Button("Reset Turn Markings"))
                {
                    settings.FindProperty("leftTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("forwardTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("rightTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("leftForwardTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("rightForwardTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("leftRightTurnMarking").objectReferenceValue = null;
                    settings.FindProperty("leftRightForwardTurnMarking").objectReferenceValue = null;
                }

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    RoadCreatorSettings.GetOrCreateSettings().CheckDefaults();
                    UpdateSettings();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUILayout.Label("Colours", guiStyle);
                settings.FindProperty("pointColour").colorValue = EditorGUILayout.ColorField("Point Colour", settings.FindProperty("pointColour").colorValue);
                settings.FindProperty("controlPointColour").colorValue = EditorGUILayout.ColorField("Control Point Colour", settings.FindProperty("controlPointColour").colorValue);
                settings.FindProperty("intersectionColour").colorValue = EditorGUILayout.ColorField("Intersection Point Colour", settings.FindProperty("intersectionColour").colorValue);
                settings.FindProperty("cursorColour").colorValue = EditorGUILayout.ColorField("Cursor Colour", settings.FindProperty("cursorColour").colorValue);
                settings.FindProperty("roadGuidelinesColour").colorValue = EditorGUILayout.ColorField("Road Guidelines Colour", settings.FindProperty("roadGuidelinesColour").colorValue);
                settings.FindProperty("roadControlGuidelinesColour").colorValue = EditorGUILayout.ColorField("Road Control Guidelines Colour", settings.FindProperty("roadControlGuidelinesColour").colorValue);

                if (GUILayout.Button("Reset Colours"))
                {
                    Color color = Color.red;
                    color.a = 0.75f;
                    settings.FindProperty("pointColour").colorValue = color;

                    color = Color.yellow;
                    color.a = 0.75f;
                    settings.FindProperty("controlPointColour").colorValue = color;

                    color = Color.green;
                    color.a = 0.75f;
                    settings.FindProperty("intersectionColour").colorValue = color;

                    color = Color.blue;
                    color.a = 0.75f;
                    settings.FindProperty("cursorColour").colorValue = color;

                    settings.FindProperty("roadGuidelinesColour").colorValue = Misc.lightGreen;
                    settings.FindProperty("roadControlGuidelinesColour").colorValue = Misc.darkGreen;
                }

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                }
            }
        };

        return settingsProvider;
    }

    public static void UpdateSettings()
    {
        RoadSystem[] roadSystems = GameObject.FindObjectsOfType<RoadSystem>();
        for (int i = 0; i < roadSystems.Length; i++)
        {
            for (int j = 0; j < roadSystems[i].transform.childCount; j++)
            {
                Transform transform = roadSystems[i].transform.GetChild(j);

                if (transform.GetComponent<RoadCreator>() != null)
                {
                    transform.GetComponent<RoadCreator>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<RoadSegment>() != null)
                {
                    transform.GetComponent<RoadSegment>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<PrefabLineCreator>() != null)
                {
                    transform.GetComponent<PrefabLineCreator>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<Intersection>() != null)
                {
                    transform.GetComponent<Intersection>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<RoadSystem>() != null)
                {
                    transform.GetComponent<RoadSystem>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
            }
        }
    }

}
#endif