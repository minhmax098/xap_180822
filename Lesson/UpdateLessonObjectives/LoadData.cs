using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using UnityEngine.Networking;
using System.IO;
using System; 
using System.Threading.Tasks; 
using EasyUI.Toast;

namespace UpdateLessonObjectives
{
    public class LoadData : MonoBehaviour
    {
        private string jsonResponse;
        private static LoadData instance; 
        public static LoadData Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = FindObjectOfType<LoadData>();
                }
                return instance; 
            }
        }
        public ListOrgans getListOrgans()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIUrlConfig.GetListOrgans); 
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            jsonResponse = reader.ReadToEnd();
            Debug.Log("Json response: ");
            Debug.Log(jsonResponse);
            return JsonUtility.FromJson<ListOrgans>(jsonResponse);
        }
        // public async Task<ListOrgans> getListOrgans()
        // {
        //     try
        //     {
        //         APIResponse<ListOrgans[]> getListOrgansResponse = await UnityHttpClient.CallAPI<ListOrgans[]>(String.Format(APIUrlConfig.GET_ORGAN_LIST), UnityWebRequest.kHttpVerbGET);
        //         if (getListOrgansResponse.code == APIUrlConfig.SUCCESS_RESPONSE_CODE)
        //         {

        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.Log("failed");
        //     }
        // }

        public AllLessonDetails GetLessonByID(string lessonId)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(String.Format(APIUrlConfig.GetLessonsByID, lessonId));
            request.Method = "GET";
            request.Headers["Authorization"] = PlayerPrefs.GetString("user_token"); 
            HttpWebResponse response = (HttpWebResponse)request.GetResponse(); 
            StreamReader reader = new StreamReader(response.GetResponseStream()); 
            jsonResponse = reader.ReadToEnd();
            return JsonUtility.FromJson<AllLessonDetails>(jsonResponse); 
        }
    }
}
