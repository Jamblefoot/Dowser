using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NEEDED FOR SAVING
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveControl : MonoBehaviour
{
    public static SaveControl instance;

    [SerializeField] GameObject wellPrefab;
    [SerializeField] GameObject podPrefab;
    [SerializeField] GameObject personPrefab;

    [System.Serializable]
    public class SaveObject
    {
        public int type;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 custom;
        public bool active;
        public Transform tran;
    }
    public List<SaveObject> saveObjects = new List<SaveObject>();

    float loadDistance = 1000f;
    float unloadDistance = 1200f;

    float autosaveInterval = 60; //300 = 5 minutes

    void Awake()
    {
        if(SaveControl.instance != null)
            Destroy(this);
        else SaveControl.instance = this;

        //ResetData();

        Load();

        
    }
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Init", 1f);
        
    }

    void Init()
    {
        //if (saveObjects.Count > 0)
        //{
        //    LoadFromPosition(GameControl.instance.GetPlanetMapPos(GameControl.instance.player.tran.position));
        //}

        StartCoroutine(LoadAndUnload());
        StartCoroutine(Autosave());
    }

    IEnumerator LoadAndUnload()
    {
        while(true)
        {
            Debug.Log("Loading and unloading");

            LoadFromPosition(GameControl.instance.player.tran.position);
            UnloadFromPosition(GameControl.instance.player.tran.position);

            yield return new WaitForSeconds(10);
        }
    }

    IEnumerator Autosave()
    {
        while(true)
        {
            yield return new WaitForSeconds(autosaveInterval);

            Save();
            Debug.Log("Autosave Complete!");
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/MySaveData.dat");
        SaveData data = new SaveData();
        data.count = saveObjects.Count;
        data.type = new int[saveObjects.Count];
        //data.position = new Vector3[saveObjects.Count];
        data.posX = new float[saveObjects.Count];
        data.posY = new float[saveObjects.Count];
        data.posZ = new float[saveObjects.Count];
        //data.rotation = new Quaternion[saveObjects.Count];
        data.rotX = new float[saveObjects.Count];
        data.rotY = new float[saveObjects.Count];
        data.rotZ = new float[saveObjects.Count];
        data.rotW = new float[saveObjects.Count];
        //data.custom = new Vector3[saveObjects.Count];
        data.customX = new float[saveObjects.Count];
        data.customY = new float[saveObjects.Count];
        data.customZ = new float[saveObjects.Count];

        for(int i = 0; i < saveObjects.Count; i++)
        {
            if(saveObjects[i].active && saveObjects[i].tran != null)
            {
                saveObjects[i].position = GameControl.instance.GetPlanetMapPos(saveObjects[i].tran.position);
                saveObjects[i].rotation = saveObjects[i].tran.rotation;
                saveObjects[i].custom = GetCustomData(saveObjects[i]);
            }

            data.type[i] = saveObjects[i].type;
            //data.position[i] = saveObjects[i].position;
            data.posX[i] = saveObjects[i].position.x;
            data.posY[i] = saveObjects[i].position.y;
            data.posZ[i] = saveObjects[i].position.z;
            //data.rotation[i] = saveObjects[i].rotation;
            data.rotX[i] = saveObjects[i].rotation.x;
            data.rotY[i] = saveObjects[i].rotation.y;
            data.rotZ[i] = saveObjects[i].rotation.z;
            data.rotW[i] = saveObjects[i].rotation.w;
            //data.custom[i] = saveObjects[i].custom;
            data.customX[i] = saveObjects[i].custom.x;
            data.customY[i] = saveObjects[i].custom.y;
            data.customZ[i] = saveObjects[i].custom.z;
        }
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game data saved! There are " + data.count.ToString() + " saved objects");
    }

    public void Load()
    {
        if(File.Exists(Application.persistentDataPath + "/MySaveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/MySaveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            for(int i = 0; i < data.count; i++)
            {
                SaveObject so = new SaveObject();
                so.type = data.type[i];
                so.position = new Vector3(data.posX[i], data.posY[i], data.posZ[i]);//data.position[i];
                so.rotation = new Quaternion(data.rotX[i], data.rotY[i], data.rotZ[i], data.rotW[i]);//data.rotation[i];
                so.custom = new Vector3(data.customX[i], data.customY[i], data.customZ[i]);//data.custom[i];        
                saveObjects.Add(so);       
            }

            Debug.Log("Game data loaded! There were " + saveObjects.Count.ToString() + " objects loaded");
        }
        else Debug.LogError("There is no save data!");
    }

    public void ResetData()
    {
        if(File.Exists(Application.persistentDataPath + "/MySaveData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/MySaveData.dat");
            saveObjects.Clear();
            Debug.Log("Data reset complete!");
        }
        else Debug.LogError("No save data to delete");
    }

    public void LoadFromPosition(Vector3 worldPos)
    {
        if(saveObjects.Count <= 0) return;

        //Vector3 pos = GameControl.instance.GetPlanetMapPos(worldPos);
        float sqrdist = loadDistance * loadDistance;
        for(int i = 0; i < saveObjects.Count; i++)
        {
            if(saveObjects[i].active) continue;
            
            Vector3 soWorldPos = GameControl.instance.GetMovedWorldPos(saveObjects[i].position);
            if((soWorldPos - worldPos).sqrMagnitude < sqrdist)
            {
                GameObject go = null;
                switch (saveObjects[i].type)
                {
                    case 0: //player - custom = (-, -, -)
                        //don't instantiate, set position, leave rotation, 
                        GameControl.instance.player.tran.position = soWorldPos;
                        go = GameControl.instance.player.gameObject;
                        //custom - health?
                        break;
                    case 1: //ship - custom = (-, -, -)
                        go = FindObjectOfType<Ship>().gameObject;
                        go.transform.position = soWorldPos;
                        go.transform.rotation = saveObjects[i].rotation;
                        break;
                    case 2: //well - custom = (type, -, -)
                        go = Instantiate(wellPrefab, soWorldPos, saveObjects[i].rotation);
                        Well w = go.GetComponent<Well>();
                        w.Plant();
                        w.ContinuePumping((int)saveObjects[i].custom.x);
                        break;
                    case 3: //pod - custom = (amount, oilPercent, bioPercent)
                        go = Instantiate(podPrefab, soWorldPos, saveObjects[i].rotation);
                        Pod pod = go.GetComponent<Pod>();
                        pod.SetLiquid(saveObjects[i].custom.x, saveObjects[i].custom.y, saveObjects[i].custom.z);
                        //go.GetComponent<Rigidbody>().isKinematic = true;
                        break;
                    case 4: //person - custom = (-, -, -)
                        go = Instantiate(personPrefab, soWorldPos, saveObjects[i].rotation);
                        break;
                }

                SaveMe sm = go.GetComponent<SaveMe>();
                sm.id = i;
                saveObjects[i].active = true;
                saveObjects[i].tran = go.transform;

                
            }
        }
    }

    public void UnloadFromPosition(Vector3 pos) //Pos here is assumed to be the moved world pos
    {
        float sqrdist = unloadDistance * unloadDistance;
        for (int i = 0; i < saveObjects.Count; i++)
        {
            if (!saveObjects[i].active || saveObjects[i].tran == null) continue;

            if ((saveObjects[i].tran.position - pos).sqrMagnitude > sqrdist)
            {
                saveObjects[i].position = GameControl.instance.GetPlanetMapPos(saveObjects[i].tran.position);
                saveObjects[i].rotation = saveObjects[i].tran.rotation;
                saveObjects[i].custom = GetCustomData(saveObjects[i]);

                saveObjects[i].active = false;
                Destroy(saveObjects[i].tran.gameObject);
                saveObjects[i].tran = null;

                Debug.Log("Unloaded object");
            }
        }
    }

    public void AddSaveObject(SaveMe sm)
    {
        if(sm.single)
        {
            for(int i = 0; i < saveObjects.Count; i++)
            {
                if(saveObjects[i].type == sm.type)
                {
                    saveObjects[i].active = true;
                    saveObjects[i].tran = sm.transform;
                    sm.transform.position = saveObjects[i].position;
                    sm.transform.rotation = saveObjects[i].rotation;
                    return;
                }
            }
        }
        
        SaveObject so = new SaveObject();
        sm.id = saveObjects.Count;
        so.type = sm.type;
        so.active = true;
        so.tran = sm.transform;
        so.position = GameControl.instance.GetPlanetMapPos(so.tran.position);
        so.rotation = so.tran.rotation;
        so.custom = Vector3.zero;
        saveObjects.Add(so);
    }

    Vector3 GetCustomData(SaveObject so)
    {
        if(so.tran == null) return so.custom;

        switch(so.type)
        {
            case 0: //player
                return Vector3.zero;
            case 1: //ship
                return Vector3.zero;
            case 2: //well
                return new Vector3(so.tran.GetComponent<Well>().output, 0, 0);
            case 3: //pod
                return so.tran.GetComponent<Pod>().GetLiquid();
            case 4: //person
                return Vector3.zero;
        }

        return Vector3.zero;
    }

    void OnApplicationQuit()
    {
        Save();
    }
}

[Serializable]
class SaveData
{
    public int count;
    public int[] type;
    //public Vector3[] position;
    public float[] posX;
    public float[] posY;
    public float[] posZ;
    public float[] rotX;
    public float[] rotY;
    public float[] rotZ;
    public float[] rotW;
    public float[] customX;
    public float[] customY;
    public float[] customZ;
    //public Quaternion[] rotation;
    //public Vector3[] custom;

}
