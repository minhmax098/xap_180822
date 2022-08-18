using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TriLibCore;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System;
using EasyUI.Toast;

public class UploadModel : MonoBehaviour
{
    public Image imgLoadingFill;
    public GameObject uiBFill;
    public Text txtPercent;
    public Button btnUploadModel3D;
    public Button btnBack;
    public GameObject uiCoat;
    public Text warningFileFormat;
    public Text warningFileSize;
    public static int idModel;
    private bool isCallAPI = false;
    private string[] arrFormatFile = {"FBX", "OBJ"};

    private static UploadModel instance;

    public static UploadModel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UploadModel>();
            }
            return instance; 
        }
    }

    private void Start()
    {
        SetEventUI();
        Screen.orientation = ScreenOrientation.Portrait; 
        StatusBarManager.statusBarState = StatusBarManager.States.TranslucentOverContent;
        StatusBarManager.navigationBarState = StatusBarManager.States.Hidden;
    }

    private void Update()
    {
        if(isCallAPI == true && UnityHttpClient.processAPI*2f*100f < 100)
        {
            imgLoadingFill.fillAmount = UnityHttpClient.processAPI * 2f;    
            txtPercent.text=$"{(imgLoadingFill.fillAmount*100f):N0} %";
        }
    }

    public void SetEventUI()
    {
        btnUploadModel3D.onClick.AddListener(HandlerUploadModel);
        btnBack.onClick.AddListener(() => BackOrLeaveApp.Instance.BackToPreviousScene(SceneManager.GetActiveScene().name));
    }

    public string GetFormatFile(string path)
    {
        string fullModelName = path.Substring(path.LastIndexOf("/") + 1);
        string formatFile = fullModelName.Substring(fullModelName.LastIndexOf(".") + 1);
        string[] modelName = fullModelName.Split('.');
        return formatFile;
    }

    public void HandlerUploadModel()
    {
        DestroyModel();
        imgLoadingFill.fillAmount = 0f;
        AssetLoaderFilePicker.Create()
            .LoadModelFromFilePickerAsync("load model", 
                x =>
                {
                    string path = $"{x.Filename}";
                    var fileInfo = new System.IO.FileInfo(path);
                    var lengthFile = fileInfo.Length/1000000;

                    Debug.Log("LengFile: " + lengthFile);

                    string formatFile = GetFormatFile(path).ToUpper();
                    if (Array.IndexOf(arrFormatFile, formatFile) < 0) 
                    {
                            ReStore();
                            warningFileSize.enabled = false;     
                            warningFileFormat.enabled = true;     
                    }
                    else 
                    {
                        if(lengthFile > 100)    
                        {
                            ReStore();
                            warningFileSize.enabled = true; 
                            warningFileFormat.enabled = false;    
                        }
                        else
                        {
                            var cam = Camera.main;

                            if (cam != null)
                            {
                                x.RootGameObject.transform.SetParent(cam.transform);
                            }

                            var render = x.RootGameObject.GetComponentsInChildren<MeshRenderer>();

                            foreach (var y in x.MaterialRenderers.Values)
                            {
                                foreach (var mrc in y)
                                {
                                    foreach (var r in render)
                                    {
                                        if (r.name == mrc.Renderer.name)
                                        {
                                            r.materials = mrc.Renderer.materials;
                                            break;
                                        }
                                    }
                                }
                            }

                            x.RootGameObject.tag = "ModelClone";

                            if (x.RootGameObject.transform.parent != null)
                            {
                                x.RootGameObject.transform.SetParent(null);
                            }

                            warningFileFormat.enabled = false;
                            warningFileSize.enabled = false;   
                            UploadModelToServer(File.ReadAllBytes(path), path);
                            DontDestroyOnLoad(x.RootGameObject);   
                        }
                    }   
                },
                x => { },
                (x, y) => { },
                x => { 
                        if(x == true)
                        {
                            uiCoat.SetActive(true);
                            uiBFill.SetActive(true);
                            txtPercent.text="0%";
                        }
                        else 
                        {
                            ReStore();
                        }
                    },
                x => { 
                    ReStore();
                    warningFileFormat.enabled = true;
                    warningFileSize.enabled = false;  
                },
                null,
                ScriptableObject.CreateInstance<AssetLoaderOptions>());
    }
    

    async void UploadModelToServer(byte[] fileData, string fileName)
    {
        try 
        {
            Debug.Log("UploadModelToServer");
            isCallAPI = true;
            var form = new WWWForm();

            form.AddBinaryData("model", fileData, fileName);

            APIResponse<ResData[]> importModelResponse = await UnityHttpClient.UploadFileAPI<ResData[]>(APIUrlConfig.Upload3DModel, UnityWebRequest.kHttpVerbPOST, form);
            isCallAPI = false;
            if (importModelResponse.code == APIUrlConfig.SUCCESS_RESPONSE_CODE)
            {
                idModel = importModelResponse.data[0].file_id;
                ModelStoreManager.InitModelStore(idModel, importModelResponse.data[0].file_path);
                Debug.Log("iModel" + idModel);
                ReStore();
                BackOrLeaveApp.Instance.AddPreviousScene(SceneManager.GetActiveScene().name, SceneConfig.interactiveModel);
                SceneManager.LoadScene(SceneConfig.interactiveModel);
            }
            else
            {
                throw new Exception(importModelResponse.message);
            }
        }
        catch (Exception exception)
        {
            ReStore();
            Toast.ShowCommonToast(exception.Message, APIUrlConfig.BAD_REQUEST_RESPONSE_CODE); 
        }
    }

    private void DestroyModel()
    {
        GameObject modelClone = GameObject.FindWithTag("ModelClone");
        if (modelClone != null)
        {
            Destroy(modelClone);
        }
    }

    public void ReStore() 
    {
        uiCoat.SetActive(false);
        uiBFill.SetActive(false);
        imgLoadingFill.fillAmount = 0; 
        txtPercent.text = "";
    }
}

[System.Serializable]
class ResData 
{
    public int type;
    public string extention;
    public double size;
    public string file_name;
    public string file_path;
    public int created_by;
    public string created_date;
    public int file_id;
}

