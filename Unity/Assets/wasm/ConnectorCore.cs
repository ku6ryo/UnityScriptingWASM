using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum PrimitiveTypeEnum
{
    Cube = 0,
    Plane = 1,
    Sphere = 2,
}


public class ConnectorCore : MonoBehaviour
{
    #if UNITY_EDITOR
    ConnectorWasmerSharp connector = new ConnectorWasmerSharp();
    // ConnectorCsWasm connector = new ConnectorCsWasm();

    #elif UNITY_WEBGL
    ConnectorWebGL connector = new ConnectorWebGL();
    #elif UNITY_ANDROID
    ConnectorWasmSharp connector = new ConnectorWasmSharp();
    #elif UNITY_STANDALONE_WIN
    ConnectorWasmerSharp connector = new ConnectorWasmerSharp();
    #else
    ConnectorDummy connector = new ConnectorDummy();
    #endif

    IDictionary<int, GameObject> ObjectMap = new Dictionary<int, GameObject>();
    IDictionary<string, int> ObjectNameMap = new Dictionary<string, int>();

    IDictionary<int, Material> MaterialMap = new Dictionary<int, Material>();
    IDictionary<string, int> MaterialNameMap = new Dictionary<string, int>();

    IDictionary<int, UnityAction<BaseEventData>> EventListenerMap = new Dictionary<int, UnityAction<BaseEventData>>();
    // key: object id , value : listner id
    IDictionary<int, int> ObjectEventListenerMap = new Dictionary<int, int>();


    int ObjectCount = 0;
    int MaterialCount = 0;
    int EventListenerCount = 0;

    bool Connected = false;

    public int CreatePrimitiveObject(PrimitiveTypeEnum p)
    {
        Debug.Log("pri");
        var type = PrimitiveType.Cube;
        if (p == PrimitiveTypeEnum.Cube) {
            type = PrimitiveType.Cube;
        }
        var obj = GameObject.CreatePrimitive(type);
        int id = ObjectCount;
        ObjectMap.Add(ObjectCount, obj);
        ObjectCount += 1;
        return id;
    }

    public int GetObjectByName (string name) {
      var known = ObjectNameMap.ContainsKey(name);
      if (known) {
        return ObjectNameMap[name];
      }
      var obj = GameObject.Find(name);
      if (obj) {
        int id = ObjectCount;
        ObjectMap.Add(ObjectCount, obj);
        ObjectNameMap.Add(name, id);
        ObjectCount += 1;
        return id;
      } else {
        return -1;
      }
    }

    public int SetObjectPosition(int objectId, float x, float y, float z)
    {
        GameObject obj = ObjectMap[objectId];
        if (obj) {
            obj.transform.position = new Vector3(x, y, z);
            return 0;
        } else {
            return 1;
        }
    }

    public Vector3? GetObjectPosition(int objectId)
    {
        GameObject obj = ObjectMap[objectId];
        if (obj) {
            return obj.transform.position;
        }
        return null;
    }

    public int SetObjectEventListener(int objectId, int type, Action listener)
    {
        GameObject obj = ObjectMap[objectId];
        if (obj) {
            obj.AddComponent<EventTrigger>();
            EventTrigger trigger = obj.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            UnityAction<BaseEventData> callback = delegate(BaseEventData data)
            {
                listener();
            };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
            var id = EventListenerCount;
            EventListenerMap.Add(id, callback);
            ObjectEventListenerMap.Add(objectId, id);
            EventListenerCount += 1;
            return id;
        } else {
            return -1;
        }
    }

    public int GetMaterialByObjectId(int objectId)
    {
        GameObject obj = ObjectMap[objectId];
        if (obj) {
          var renderer = obj.GetComponent<Renderer>();
          var material = renderer.material;
          var known = MaterialNameMap.ContainsKey(material.name);
          if (known) {
            return MaterialNameMap[material.name];
          } else {
            var id = MaterialCount;
            MaterialMap.Add(id, material);
            MaterialNameMap.Add(material.name, id);
            MaterialCount += 1;
            return id;
          }
        } else {
          return -1;
        }
    }

    public int GetMaterialByName(string name) 
    {
        if (MaterialNameMap.ContainsKey(name)) {
            return MaterialNameMap[name];
        }
        Material material = (Material) Resources.Load(name, typeof(Material));
        if (material) {
          var id = MaterialCount;
          MaterialMap.Add(id, material);
          MaterialNameMap.Add(name, id);
          MaterialCount += 1;
          return id;
        } else {
          return -1;
        }
    }

    public int SetMaterialColor(int materialId, float r, float g, float b, float a)
    {
        var material = MaterialMap[materialId];
        if (material) {
            material.color = new Color(r, g, b, a);
            return 1;
        } else {
            return -1;
        }
    }

    public void Connect ()
    {
      Connected = true;
    }

    public void Disconnect ()
    {
      Connected = false;
    }

    ///////////////////////////////
    // Unity life cycle methods. //
    ///////////////////////////////

    void Start()
    {
        Debug.Log("start");
        connector.Init(this);
    }
    void Update()
    {
      if (Connected) {
        connector.Update();
      }
    }
}
