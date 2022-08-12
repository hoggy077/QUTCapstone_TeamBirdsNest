using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CareerBuilder : EditorWindow
{
    [MenuItem("Window/Career Builder")]
    static void CreateWindow() 
    {
        CareerBuilder cb = EditorWindow.GetWindow<CareerBuilder>();
        cb.maxSize = new Vector2(265, 125);
        cb.minSize = cb.maxSize;
        cb.Show();
    }

    uint bowlsRolled = 0;
    uint roundsWon = 0;
    uint gamesWon = 0;
    string Name = "";

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        Name = EditorGUILayout.TextField("User Name", Name);
        gamesWon = (uint)Mathf.Clamp(EditorGUILayout.IntField("Games Won", (int)gamesWon), 0, uint.MaxValue);
        roundsWon = (uint)Mathf.Clamp(EditorGUILayout.IntField("Rounds Won", (int)roundsWon), 0, uint.MaxValue);
        bowlsRolled = (uint)Mathf.Clamp(EditorGUILayout.IntField("Bowls Rolled", (int)bowlsRolled),0,uint.MaxValue);
        EditorGUILayout.EndVertical();

        Divider(new Color32(211, 211, 211, 255), padding: 10);

        EditorGUILayout.BeginHorizontal();
        bool Save = GUILayout.Button("Save");
        bool Clear = GUILayout.Button("Clear");
        bool Delete = GUILayout.Button("Delete");
        EditorGUILayout.EndHorizontal();

        if (Save)
            SaveSystem.saveGeneric<PlayerCareer>(new PlayerCareer() { BowlsRolled = bowlsRolled, GamesWon = gamesWon, RoundsWon = roundsWon, Name = Name }, "careerInfo.career");

        if (Clear)
        {
            gamesWon = 0;
            Name = "";
            roundsWon = 0;
            bowlsRolled = 0;
        }

        if (Delete)
            SaveSystem.performDelete("careerInfo.career");
    }

    void Divider(Color32 color, int thickness = 2, int padding = 5)
    {
        Rect rec = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
        rec.height = thickness; rec.y += padding / 2; rec.x += 3; rec.width -= 6;
        EditorGUI.DrawRect(rec, color);
    }
}
