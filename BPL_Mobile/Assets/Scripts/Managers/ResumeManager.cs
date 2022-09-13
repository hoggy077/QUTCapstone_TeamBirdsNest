using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

public static class ResumeManager
{
    
    public static bool hasPriorGame { get; private set; } = false;
    private static bool hasEvaluated = false;
    public static SavedSession? availableSession = null;
    public static readonly string extension = ".sav";
    //file extension is .sav or .sav

    public static void Reset()
    {
        hasEvaluated = false;
        availableSession = null;
    }

    public static void EvaluateSession()
    {
        if (SaveSystem.verifyFile<SavedSession>($"lastSession{extension}"))
        {
            try
            {


#if UNITY_EDITOR
                availableSession = SaveSystem.loadGeneric<SavedSession>(false, $"lastSession{extension}");
#else
                availableSession = SaveSystem.loadGeneric<SavedSession>(true, $"lastSession{extension}");
#endif
                hasEvaluated = true;
                hasPriorGame = true;

#if UNITY_EDITOR
                refList = AssetDatabase.LoadAssetAtPath<ReferenceStorage>("Assets/RefTable.asset");
#endif

            }
            catch (Exception e)
            {

            }
        }
    }



    public static void LoadGame(string sceneName = "")
    {
        if(!hasEvaluated)
            EvaluateSession();

        if(availableSession != null)
        {
            if(sceneName != "")
            {
                AsyncOperation bruv = SceneManager.LoadSceneAsync("", LoadSceneMode.Single);
                bruv.completed += (AsyncOp) =>
                {
                    foreach (TrackedObject tObj in ((SavedSession)availableSession).trackedGameObjects)
                    {
                        if (refList.references.Any((k) => { return k.name == tObj.referenceTerm; }))
                        {
                            GameObject basePrefab = (GameObject)refList.references.First((k) => { return k.name == tObj.referenceTerm; }).value;
                            basePrefab = GameObject.Instantiate(basePrefab, tObj.objMatrix.po(), tObj.objMatrix.ro());
                            basePrefab.transform.localScale = tObj.objMatrix.sc();
                            if (basePrefab.GetComponent<BowlID>() != null)
                                basePrefab.GetComponent<BowlID>().SetTeam(tObj.TeamRef);
                            //basePrefab.transform.SetPositionAndRotation(tObj.objMatrix, tObj.objMatrix.toMatrix().rotation);
                        }
                    }
                    return;
                };
            }

            foreach(TrackedObject tObj in ((SavedSession)availableSession).trackedGameObjects)
            {
                if (refList.references.Any((k) => { return k.name == tObj.referenceTerm; }))
                {
                    GameObject basePrefab = (GameObject)refList.references.First((k) => { return k.name == tObj.referenceTerm; }).value;
                    basePrefab = GameObject.Instantiate(basePrefab, tObj.objMatrix.po(), tObj.objMatrix.ro());
                    basePrefab.transform.localScale = tObj.objMatrix.sc();
                    if (basePrefab.GetComponent<BowlID>() != null)
                        basePrefab.GetComponent<BowlID>().SetTeam(tObj.TeamRef);
                    //basePrefab.transform.SetPositionAndRotation(tObj.objMatrix.toMatrix().GetPosition(), tObj.objMatrix.toMatrix().rotation);
                }
            }
        }
    }

    public static void SaveGame()
    {
        TrackThisThing[] things2track = GameObject.FindObjectsOfType<TrackThisThing>();
        TrackedObject[] tracking = new TrackedObject[things2track.Length];
        for (int i = 0; i < things2track.Length; i++)
            tracking[i] = things2track[i].getTracked();

        SavedSession ss = new SavedSession()
        {
            trackedGameObjects = tracking,
            saveTime = DateTime.UtcNow

            //CurrentGamemode = GSM.fuckthis is long,
            //    ss.Team1_state = GameStateManager.Instance.Team_1;
            //    ss.Team2_state = GameStateManager.Instance.Team_2;

            //Add in a gamemode set here as well
        };

        SaveSystem.saveGeneric(ss, "lastSession.sav");
    }


    private static ReferenceStorage refList = null;
    public static void SetReferenceTable(ReferenceStorage refTable) =>
        refList = refTable;
}


#region Sessions, Tracking, and Interface
public struct SavedSession
{
    public DateTime saveTime;
    public int CurrentGamemode;//cast this
    public TrackedObject[] trackedGameObjects;
    public Team_struct Team1_state;
    public Team_struct Team2_state;
    //public TurnBasedManager.Turn LastTurn;
    //public TurnBasedManager.Turn CurrentTurn;
}

[Serializable]
public struct TrackedObject
{
    public TrackedObject(TrackedData obj, string term)
    {
        GameObject gobj = obj.getSelf();
        objMatrix = new TransformData(gobj.transform.localToWorldMatrix);
        BowlID bid = gobj.GetComponent<BowlID>();
        TeamRef = bid != null ? bid.GetTeam() : 0;
        referenceTerm = term;
    }
    [SerializeField]
    public TransformData objMatrix;
    public int TeamRef;
    public string referenceTerm;
}

[Serializable]
public class TransformData
{
    public TransformData() { }
    public TransformData(Matrix4x4 d) 
    {
        Vector3 p = d.GetPosition();
        Quaternion r = d.rotation;
        Vector3 s = d.lossyScale;
        pos = new System.Numerics.Vector3(p.x,p.y,p.z);
        rot = new System.Numerics.Vector4(r.x,r.y,r.z,r.w);
        scl = new System.Numerics.Vector3(s.x,s.y,s.z);
    }

    public Vector3 sc() => new Vector3(scl.X, scl.Y, scl.Z);
    public Vector3 po() => new Vector3(pos.X, pos.Y, pos.Z);
    public Quaternion ro() => new Quaternion(rot.X, rot.Y, rot.Z, rot.W);

    public System.Numerics.Vector3 pos;
    public System.Numerics.Vector4 rot;
    public System.Numerics.Vector3 scl;
}

public interface TrackedData
{
    public string getReferenceTerm();
    public TrackedObject getTracked();
    public GameObject getSelf();
}
#endregion




#if UNITY_EDITOR
#region custom tool
public class SessionWindow : EditorWindow
{
    [MenuItem("Window/Sessions")]
    static void CreateWindow()
    {
        SessionWindow cb = EditorWindow.GetWindow<SessionWindow>();//EditorWindow.GetWindow<SessionWindow>();
        cb.Show();
    }

    private Object currentval;
    private string name;

    public static ReferenceStorage ObjTable
    {
        get
        {
            if (ObjTable_ == null)
            {
                ObjTable_ = AssetDatabase.LoadAssetAtPath<ReferenceStorage>("Assets/RefTable.asset");
                if (ObjTable_ == null)
                {
                    ObjTable_ = new ReferenceStorage();
                    AssetDatabase.CreateAsset(ObjTable, "Assets/RefTable.asset");
                    AssetDatabase.Refresh();
                }
            }
            return ObjTable_;
        }
        set { ObjTable_ = value; }
    }

    private static ReferenceStorage ObjTable_;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Scene Saving options");
        EditorGUILayout.BeginHorizontal();
        
        if(GUILayout.Button("Save Scene"))
            ResumeManager.SaveGame();

        if(GUILayout.Button("Load Scene"))
            ResumeManager.LoadGame();

        if(GUILayout.Button("Wipe save file."))
        {
            if(File.Exists($"{SaveSystem.PersistentPath}\\lastSession{ResumeManager.extension}"))
                SaveSystem.performDelete($"lastSession{ResumeManager.extension}");
        }

        EditorGUILayout.EndHorizontal();

        Divider(new Color32(211, 211, 211, 255), padding: 10);

        EditorGUILayout.LabelField("Current references");

        EditorGUILayout.BeginVertical();
        for(int ind = 0; ind < ObjTable.Count; ind++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ObjTable.references[ind].name);
            EditorGUILayout.ObjectField(ObjTable.references[ind].value, ObjTable.references[ind].value.GetType(), false);
            if (GUILayout.Button("Remove"))
            {
                ObjTable.references.RemoveAt(ind);
                EditorUtility.SetDirty(ObjTable);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        Divider(new Color32(211, 211, 211, 255), padding: 10);

        EditorGUILayout.BeginHorizontal();
        name = EditorGUILayout.TextField(name);
        currentval = EditorGUILayout.ObjectField(currentval, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();

        //
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Named References"))
        {
            if(name != string.Empty && !ObjTable.references.Any((kvp) => { return kvp.name == name || kvp.value == currentval; }))
            {
                
                ObjTable.references.Add(new pair() { name = name, value = currentval });
                EditorUtility.SetDirty(ObjTable);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                name = string.Empty;
                currentval = null;
            }
        }
        EditorGUILayout.EndHorizontal();
    }


    void Divider(Color32 color, int thickness = 2, int padding = 5)
    {
        Rect rec = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
        rec.height = thickness; rec.y += padding / 2; rec.x += 3; rec.width -= 6;
        EditorGUI.DrawRect(rec, color);
    }
}
#endregion
#endif
