using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public GameObject[] Stacks;

    public GameObject PrefabWoodBlock;
    public GameObject PrefabConcreteBlock;
    public GameObject PrefabGlassBlock;

    public CinemachineFreeLook cinemachine;

    public TMP_Text blockTXT;
    public TMP_Text buttonTXT;

    public bool tested = false; 
    
    [System.Serializable]
    public struct Topic {
        public int id;
        public string subject;
        public string grade;
        public int mastery;
        public string domainid;
        public string domain;
        public string cluster;
        public string standardid;
        public string standarddescription;
    }

    [System.Serializable]
    public struct Topics {
        public Topic[] topics;
    }

    private Topics apiData;

    // Start is called before the first frame update
    void Start()
    {
        CinemachineCore.GetInputAxis = GetAxisCustom;
        StartCoroutine(FetchData());        
    }

    GameObject InstanceBlock(int type, Transform stack){
        switch(type){
            case 0:
                return GameObject.Instantiate(PrefabGlassBlock, stack);
            case 1:
                return GameObject.Instantiate(PrefabWoodBlock, stack);
            case 2:
                return GameObject.Instantiate(PrefabConcreteBlock, stack);
            default:
                return new GameObject();
        }
    }

    void AdjustBlockPosition(Transform block, int childCount){
        float yAdjust = 0.55f;
        float blockSpacing = 1.5f;
        if(childCount % 6 < 3){
            block.transform.localPosition = new Vector3(childCount % 3 * blockSpacing, childCount / 3 * yAdjust, 0);
        }else {
            block.transform.Rotate(0,90, 0);
            block.transform.localPosition = new Vector3(blockSpacing, childCount / 3 * yAdjust, childCount % 3 * blockSpacing - blockSpacing);
        }
    }

    void PlaceBlocks(){
        foreach(Topic topic in apiData.topics){
            GameObject newBlock;
            switch(topic.grade){
                case "6th Grade":
                    newBlock = InstanceBlock(topic.mastery, Stacks[0].transform);
                    newBlock.name = ""+topic.id;
                    Debug.Log(Stacks[0].transform.childCount);
                    AdjustBlockPosition(newBlock.transform,  Stacks[0].transform.childCount-1);
                    
                break;
                case "7th Grade":
                    newBlock = InstanceBlock(topic.mastery, Stacks[1].transform);
                    newBlock.name = ""+topic.id;
                    AdjustBlockPosition(newBlock.transform,  Stacks[1].transform.childCount-1);
                    
                break;
                case "8th Grade":
                    newBlock = InstanceBlock(topic.mastery, Stacks[2].transform);
                    newBlock.name = ""+topic.id;
                    AdjustBlockPosition(newBlock.transform,  Stacks[2].transform.childCount-1);
                break;
            }
        }
    }

    IEnumerator FetchData() {
        UnityWebRequest www = UnityWebRequest.Get("https://ga1vqcu3o1.execute-api.us-east-1.amazonaws.com/Assessment/stack");
        yield return www.SendWebRequest();
 
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            string jsonText = "{\"topics\":"+www.downloadHandler.text+"}";
            apiData = JsonUtility.FromJson<Topics>(jsonText);
            System.Array.Sort(apiData.topics, (t1, t2)=>t1.domain.CompareTo(t2.domain));
            System.Array.Sort(apiData.topics, (t1, t2)=>t1.cluster.CompareTo(t2.cluster));
            System.Array.Sort(apiData.topics, (t1, t2)=>t1.standardid.CompareTo(t2.standardid));
            PlaceBlocks();
        }
    }

    public float GetAxisCustom(string axisName){
        if(axisName == "Mouse X"){
            if (Input.GetMouseButton(0)){
                return UnityEngine.Input.GetAxis("Mouse X");
            } else{
                return 0;
            }
        }
        else if (axisName == "Mouse Y"){
            if (Input.GetMouseButton(0)){
                return UnityEngine.Input.GetAxis("Mouse Y");
            } else{
                return 0;
            }
        }
        return UnityEngine.Input.GetAxis(axisName);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                Transform CameraTarget = hit.collider.transform.parent.parent.Find("CameraTarget").transform;
                cinemachine.Follow = CameraTarget;
                cinemachine.LookAt = CameraTarget;
            }
        }
        if (Input.GetMouseButtonDown(1)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)){
                GameObject block = hit.collider.gameObject;
                Topic data = System.Array.Find(apiData.topics, (t)=>""+t.id == block.name);
                Debug.Log(data.grade);
                if(data.id != 0){
                    blockTXT.transform.parent.gameObject.SetActive(true);
                    blockTXT.text = data.grade + ":" + data.domain + "\n" + data.cluster + "\n" + data.standardid + ":" + data.standarddescription;
                }else{
                    blockTXT.transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }

    public void TestMyStack(){
        if(!tested){
            GameObject[] glasses = GameObject.FindGameObjectsWithTag("Glass");
            foreach(GameObject go in glasses){
                Destroy(go);
            }
            tested = true;
            buttonTXT.text = "Reset";
        }else{
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
