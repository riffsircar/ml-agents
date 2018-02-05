﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.Linq;
/*
 This code is meant to modify the behavior of the inspector on Brain Components.
 Depending on the type of brain that is used, the available fields will be modified in the inspector accordingly.
*/
[CustomEditor(typeof(Agent), true)]
[CanEditMultipleObjects]
public class AgentEditor : Editor
{

    public override void OnInspectorGUI()
    {
        SerializedObject serializedAgent = serializedObject;
        serializedAgent.Update();

        SerializedProperty brain = serializedAgent.FindProperty("brain");

        SerializedProperty actionsPerDecision = serializedAgent.FindProperty("agentParameters.numberOfActionsBetweenDecisions");
        SerializedProperty stepsPerAction = serializedAgent.FindProperty("agentParameters.numberOfStepsBetweenActions");
        SerializedProperty maxSteps = serializedAgent.FindProperty("agentParameters.maxStep");
        SerializedProperty isResetOnDone = serializedAgent.FindProperty("agentParameters.resetOnDone");
        SerializedProperty isEBS = serializedAgent.FindProperty("agentParameters.eventBased");
        SerializedProperty cameras = serializedAgent.FindProperty("agentParameters.agentCameras");

        EditorGUILayout.PropertyField(brain);

        EditorGUILayout.LabelField("Agent Cameras");
		for (int i = 0; i < cameras.arraySize; i++)
		{
			EditorGUILayout.PropertyField(cameras.GetArrayElementAtIndex(i), new GUIContent("Camera " + (i + 1).ToString() + ": "));
		}
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Camera", EditorStyles.miniButton))
        {
            cameras.arraySize++;
        }
		if (GUILayout.Button("Remove Camera", EditorStyles.miniButton))
		{
			cameras.arraySize--;
		}
        EditorGUILayout.EndHorizontal();
		
        EditorGUILayout.PropertyField(maxSteps, new GUIContent("Max Step",
                                		"The per-agent maximum number of steps."));
        EditorGUILayout.PropertyField(isResetOnDone, new GUIContent("Reset On Done",
                         "If checked, the agent will reset on done. Else, AgentOnDone() will be called."));
        EditorGUILayout.PropertyField(isEBS, new GUIContent("Event Based Only."));
        if (!isEBS.boolValue)
        {
            EditorGUILayout.PropertyField(stepsPerAction, new GUIContent("Steps per Action"));
            EditorGUILayout.PropertyField(actionsPerDecision, new GUIContent("Actions per Decision"));
        }

		serializedAgent.ApplyModifiedProperties();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        base.OnInspectorGUI();




    }
}