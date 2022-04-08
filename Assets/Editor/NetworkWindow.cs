using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net;
using System.Linq;

public class NetworkWindow : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;
    private bool lineBreak = true;
    private float maxWidth;

    [MenuItem("Window/Network Window")]
    public static void ShowWindow()
    {
        GetWindow<NetworkWindow>("Network Window");
    }

    private void OnGUI()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorGUILayout.Space();
            GUI.contentColor = Color.red;
            GUILayout.Label("Enter play mode first to start Riptide");
            GUI.contentColor = Color.white;
        }
        else
        {
            if (NetworkManager.Singleton.Server.IsRunning)
            {
                GUI.contentColor = Color.green;
                GUILayout.Label("Server is running");
                GUI.contentColor = Color.white;

                EditorGUILayout.Space();
                GuiLine();
                GUILayout.Label("Connected Clients: " + NetworkManager.Singleton.Server.ClientCount);
                GuiLine();
                EditorGUILayout.Space();

                GUI.contentColor = Color.cyan;
                GUILayout.Label("HoloLens Logs:", EditorStyles.boldLabel);
                GUI.contentColor = Color.white;

                lineBreak = GUILayout.Toggle(lineBreak, "Line Break");
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(GUILayout.Height(504));

                Rect rectPos = EditorGUILayout.GetControlRect();
                Rect rectBox = new Rect(rectPos.x, rectPos.y, rectPos.width, 500f);
                maxWidth = rectPos.width;

                int viewCount = 30;
                int firstIndex = (int)scrollPosition.y / 18;

                if (!lineBreak) 
                {
                    for (int i = firstIndex; i < Mathf.Min(Log.logs.Count, firstIndex + viewCount); i++)
                    {
                        if (Log.logs[i].Text.Length * 5.5f > maxWidth)
                        {
                            maxWidth = Log.logs[i].Text.Length * 5.5f;
                        }
                    }
                }
       
                Rect viewRect = new Rect(rectBox.x, rectBox.y, maxWidth, Log.logs.Count * 18f);

                scrollPosition = GUI.BeginScrollView(rectBox, scrollPosition, viewRect, !lineBreak, true, lineBreak ? GUIStyle.none : GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);

                Rect contentPosition = new Rect(rectBox.x, 105f + firstIndex * 18f, viewRect.width, 18f);

                GUIStyle textStyle = EditorStyles.label;
                textStyle.wordWrap = lineBreak;

                for (int i = firstIndex; i < Mathf.Min(Log.logs.Count, firstIndex + viewCount); i++)
                {
                    float heightDelta = !lineBreak ? 18f : 18f * Mathf.Max(1, (int)((Log.logs[i].Text.Length * 6f) / (contentPosition.width)));

                    if (Log.logs[i].Type == Log.LogType.Warning)
                    {
                        EditorGUI.DrawRect(new Rect(contentPosition.x, contentPosition.y, Log.logs[i].Text.Length * 5.5f, heightDelta), new Color(255, 255, 0, 0.2f));
                    }
                    else if (Log.logs[i].Type == Log.LogType.Error)
                    {
                        EditorGUI.DrawRect(new Rect(contentPosition.x, contentPosition.y, Log.logs[i].Text.Length * 5.5f, heightDelta), new Color(255, 0, 0, 0.2f));
                    }

                    contentPosition.height = heightDelta;
                    GUI.Label(contentPosition, Log.logs[i].Text, textStyle);
                    contentPosition.y += heightDelta;
                }

                GUI.EndScrollView();

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void GuiLine(int height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, height);
        rect.height = height;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }
}
