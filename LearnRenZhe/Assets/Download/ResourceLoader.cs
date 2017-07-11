using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
//using System.Diagnostics;
//using ICSharpCode.SharpZipLib.Core;
using System.Xml;
using System.Security.Cryptography;
using System.Collections;
using Debug = UnityEngine.Debug;

public class ResourceLoader : MonoBehaviour
{ 

    [HideInInspector]
    public static string LocalPath = "";
    public static ResourceLoader Instance = null;
    private bool theResourceIsOK = false;
    private bool theResoureIsDowloading = false;
    public int targetFPS = 45;

    [HideInInspector] 
    public   string ResourceURL{
		get{
            return "http://123.57.152.1:8080/CardgameConfig/yijieconfig/";
		}
    } 

    private const string ResourceListName = "sourceList.txt";
    public const string LocalSourcePath = "configInternal/sourceListInternal";
    public const string LocalSourceDirectory = "configInternal/";

    private WWW ResourceList = null;
    private float time;
    private float unitDelayTime=0.12f;
    private int DowloadbBegain = 0;// 0 还没有开始下载, 1正在下载 2 下载超时 3 下载完成
    private long CurrentBytesLength = 0;
    private long TotalBytesLength = 0;
    private int totalNumberNeedDowload = 0;
    private int currentNumberDowdlod = 0;

    private Dictionary<string, ResourceObject> allResources = new Dictionary<string, ResourceObject>();
    private  Dictionary<string, ResourceObject> allResourcesLocal=new Dictionary<string, ResourceObject>();

    private SecurityQueue<SaveDataInfo> SaveQueue = new SecurityQueue<SaveDataInfo>();
    private SecurityQueue<WWWreqestInfo> DownloadQueue = new SecurityQueue<WWWreqestInfo>();
    private SecurityDictionary<WWW, WWWreqestInfo> DownloadDictionary = new SecurityDictionary<WWW, WWWreqestInfo>();
    public class SaveDataInfo
    {
        public string rawName;
        public string guid;
        public byte[] datas;
        public SaveDataInfo(string _rawName, string _guid, byte[] _datas)
        {
            rawName = _rawName;
            guid = _guid;
            datas = _datas;
        }
    }

    public static  string GetTheSuffixOfURL()
    {
        string url = "?";
        string arg = "random=";
        System.Random random =new  System.Random(unchecked((int)(System.DateTime.Now.Ticks)));
        return url + arg + random.Next(1, 1000).ToString();
    }

    public class WWWreqestInfo
    {
        public string url;
        public string rawName;
        public WWWreqestInfo(string _url, string _rawName)
        {
            url = _url;
            rawName = _rawName;
        }

    }

   public static  string loadingStr = "----null";
//    #undefine Debug1
    void Awake()
    {
        Application.targetFrameRate = targetFPS;
        ZipConstants.DefaultCodePage = 65001;
        Instance = this;
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, 200, 30), loadingStr);
    }

    private Action callback = null;
    public void CheckTheVersion(Action callback)
    {
        loadingStr = "0.5" + "正在核对本地资源";
        this.callback = callback;
        StartCoroutine(startDowloadTheResourceList());
    }

    IEnumerator startDowloadTheResourceList()
    {
        time = Time.realtimeSinceStartup;
        DowloadbBegain = 1;
        ResourceList = new WWW(ResourceURL + ResourceListName+GetTheSuffixOfURL());
        Debug.LogError("0:"+ResourceURL + ResourceListName+GetTheSuffixOfURL());
        yield return ResourceList;
    }
    void Update()
    {
        if (DowloadbBegain == 1)
        {
         //   Debug.Log("111111111111");
            if (((int)Time.realtimeSinceStartup - (int)time) > 30)
            {
                DowloadbBegain = 2;
            }

            if (ResourceList.isDone && ResourceList.error == null)
            {
                Debug.Log("1:ResourceList.isDone");
                DowloadbBegain = 3;
                string resoures = Encoding.UTF8.GetString(ResourceList.bytes);

                Debug.LogError("下载下来的字符串是>>>"+resoures);
                string[] resouresLines = resoures.Split('\n');
             //   Debug_our.LogError("Ò»¹²ÓÐ×ÊÔŽµÄÊýÁ¿"+resouresLines.Length);
                for (int i = 0; i < resouresLines.Length; i++)
                {
                    string[] resouresOne = resouresLines[i].Split('\t');
  //                  Debug.LogError(resouresOne[1]);
                    ResourceObject ro = new ResourceObject(resouresOne[1], resouresOne[2], resouresOne[3], Convert.ToInt64(resouresOne[4]),EnumResourcePostion.NONE);
                    
                    if (!allResources.ContainsKey(ro.OriginSourceName))
                        allResources.Add(ro.OriginSourceName, ro);
                }

                int needdowload = checkTheLocalResoure();
                if (needdowload == 0)
                {
                    loadingStr = "1" + "本地资源都是好的";
                    theResourceIsOK = true;
                    if (this.callback != null)
                    {
                        callback();
                    }
                }
                else
                {
                    string tips = "";
                    if (TotalBytesLength<1000)
                    {
                        tips = "<1K";
                    }
                    else
                    {
                        tips = (int) (TotalBytesLength/1000) + "K";
                    }
                    loadingStr = "0" + "总共" + needdowload + "文件，需要下载，总共大小:" + tips;
                }
            }
        }


        if (DownloadQueue.Count > 0)
        {
            Debug.Log("DownloadQueue.Count > 0");
            Debug.LogError("总共需要"+DownloadQueue.Count+"下载");
            theResourceIsOK = false;
//            Debug_our.LogError(" ÏÂÔØ¶ÓÁÐÖÐÓÐÇëÇó");
           
            float delay = DownloadQueue.Count*unitDelayTime;
            WWWreqestInfo wwwRequestInfo = null;
            DownloadQueue.Dequeue(ref wwwRequestInfo);
            Debug.LogError("需要" + wwwRequestInfo.rawName + "下载");
            StartCoroutine(DoDowload(wwwRequestInfo,delay));
        }

        if (SaveQueue.Count > 0)
        {
            Debug.Log("SaveQueue.Count > 0");
            SaveDataInfo si = null;
            SaveQueue.Dequeue(ref si);
            StartCoroutine(SavedataToFile(si.guid, si.datas, si.rawName));
        }

    }


    private IEnumerator DoDowload(WWWreqestInfo wwwRequestInfo,float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.LogError("添加到wwwPool里面"+wwwRequestInfo.rawName);

        WwwWorkpool.instance.addwork(wwwRequestInfo.url+GetTheSuffixOfURL(), (data) =>
            {
                SaveDataInfo saveDataInfo = new SaveDataInfo(wwwRequestInfo.rawName, allResources[wwwRequestInfo.rawName].Guid, data);

                SaveQueue.Enqueue(saveDataInfo);
            });

    }
    public string GetTheLocalPathName()
    {
        string PathURL = "";
        if (Application.platform == RuntimePlatform.Android)
        {
            PathURL = Application.persistentDataPath + "/MeData/";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            PathURL = Application.persistentDataPath + "/MeData/";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            PathURL = Application.dataPath + "/MeData/";
        }
        else if
        (Application.platform == RuntimePlatform.WindowsEditor)
        {
            PathURL = Application.dataPath + "/MeData/";
        }
        else if
         (Application.platform == RuntimePlatform.OSXEditor)
        {
            PathURL = Application.dataPath + "/MeData/";
        }
        string temp = PathURL.Remove(PathURL.Length - 1);
        if (!Directory.Exists(temp))
        {
            Directory.CreateDirectory(temp);
        }
        return PathURL;
    }
    private int checkTheLocalResoure()
    {
        LocalPath = GetTheLocalPathName();
        Debug.LogError("+++++++++++++++++开始接测本地资源++++++++++++++++++++++++");
        TextAsset textAsset = (TextAsset)Resources.Load(LocalSourcePath);
        string[] localResourcesList =textAsset.text.Split("\n".ToCharArray());
        allResourcesLocal.Clear();
        for (int i = 0; i < localResourcesList.Length; i++)
        {
            string[] oneRource = localResourcesList[i].Split('\t');
            ResourceObject resourceObject =new ResourceObject(oneRource[0],"-1",oneRource[1],-1,EnumResourcePostion.NONE);
            allResourcesLocal.Add(resourceObject.OriginSourceName,resourceObject);
        }

        string PathURL = GetTheLocalPathName();
        foreach (string key in allResources.Keys)
        {
            if (allResourcesLocal.ContainsKey(key))
            {
               // Debug_our.LogError("在本地找到资源》》》》》"+key);
                if(allResourcesLocal[key].MD5==allResources[key].MD5)
                {
                    allResources[key].Postion=EnumResourcePostion.INTERNAL;
                    continue;
                }
                else
                {
                 //   Debug.LogError("本地资源和标准资源不一致，我要更新》》》》"+key);
                    if (!File.Exists(PathURL + allResources[key].Guid))
                    {
                        allResources[key].Postion = EnumResourcePostion.OUTER;
                        TotalBytesLength += allResources[key].length;
                        totalNumberNeedDowload++;
                        DownloadQueue.Enqueue(new WWWreqestInfo(ResourceURL + allResources[key].Guid, key));
                    }
                    else
                    {
                        string tempPathURL = PathURL.Remove(PathURL.Length - 1);
                        byte[] noPure = FileToBytes(PathURL + allResources[key].Guid);
                        byte[] pure = removeRubish(noPure);
                        byte[] rawBytes = UnZipFile(pure);
                        string localMD5 = bytesMD5Value(rawBytes);

                        if (localMD5 != allResources[key].MD5)
                        {
                            //Debug_our.LogError("·¢ÏÖ±ŸµØ×ÊÔŽËð»µ¡·¡·¡· ÉŸ³ý" + key);
                            File.Delete(PathURL + allResources[key].Guid);
                            allResources[key].Postion=EnumResourcePostion.OUTER;
                            TotalBytesLength += allResources[key].length;
                            totalNumberNeedDowload++;
                            DownloadQueue.Enqueue(new WWWreqestInfo(ResourceURL + allResources[key].Guid, key));
                        }
                        else
                        {
                            allResources[key].Postion = EnumResourcePostion.OUTER;
                            //Debug_our.LogError("·¢ÏÖ±ŸµØ×ÊÔŽ MD5Öµ Ã»ÓÐ·¢Éú±ä»¯¡·¡·¡·"+key);
                        }
                    }
                }
            }
            else
            {
                if (!File.Exists(PathURL + allResources[key].Guid))
                {
                    allResources[key].Postion = EnumResourcePostion.OUTER;
                    TotalBytesLength += allResources[key].length;
                    totalNumberNeedDowload++;
                    DownloadQueue.Enqueue(new WWWreqestInfo(ResourceURL + allResources[key].Guid, key));
                }
                else
                {
                    string tempPathURL = PathURL.Remove(PathURL.Length - 1);
                    byte[] noPure = FileToBytes(PathURL + allResources[key].Guid);
                    byte[] pure = removeRubish(noPure);
                    byte[] rawBytes = UnZipFile(pure);
                    string localMD5 = bytesMD5Value(rawBytes);

                    if (localMD5 != allResources[key].MD5)
                    {
                        allResources[key].Postion = EnumResourcePostion.OUTER;
                        File.Delete(PathURL + allResources[key].Guid);
                        TotalBytesLength += allResources[key].length;
                        totalNumberNeedDowload++;
                        DownloadQueue.Enqueue(new WWWreqestInfo(ResourceURL + allResources[key].Guid, key));
                    }
                    else
                    {
                        allResources[key].Postion = EnumResourcePostion.OUTER;
                    }
                }

            }
        }
        return totalNumberNeedDowload;
    }


    public byte[] getRawResourceBytes(string key)
    {

        if (allResources.ContainsKey(key))
        {
            if (allResources[key].Postion==EnumResourcePostion.INTERNAL)
            {
               // Debug_our.LogError("LocalSourceDirectory+key" + LocalSourceDirectory + key);
                TextAsset textAsset = (TextAsset)Resources.Load(LocalSourceDirectory+key);
                return Encoding.UTF8.GetBytes(textAsset.text);
            }
            else if (allResources[key].Postion == EnumResourcePostion.OUTER)
            {
                string PathURL = GetTheLocalPathName();
                byte[] noPure = FileToBytes(PathURL + allResources[key].Guid);
                byte[] pure = removeRubish(noPure);
                byte[] rawBytes = UnZipFile(pure);
                return rawBytes;
                
            }
            else
            {
                Debug.LogError("本地资源找到啦， 但是存储位置有问题<<<");
                return null;
            }
        }
        else
        {
            Debug.LogError("本地资源没有找到 你所需要的资源");
            return null;
        }
    }
    private byte[] FileToBytes(string filePath)
    {
        byte[] rawDatas = null;
        if (!File.Exists(filePath))
        {
            //Debug_our.LogError("ÎÄŒþ²»ŽæÔÚ");
            return null;
        }
        else
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {

                rawDatas = new byte[fs.Length];
                fs.Read(rawDatas, 0, rawDatas.Length);

            }
            return rawDatas;
        }

    }
    private byte[] removeRubish(byte[] datas)
    {

        if (datas == null) return null;
        //List<byte> 
        LinkedList<byte> temp = new LinkedList<byte>(datas);
        for (int i = 0; i < 3; i++)
        { temp.RemoveFirst(); }
        byte[] result = new byte[temp.Count];
        temp.CopyTo(result, 0);
        return result;
    }
    private IEnumerator SavedataToFile(string fileName, byte[] datas, string _rawName)
    {
        string pathURL = GetTheLocalPathName();
        //Debug_our.LogError("Òª±£ŽæµÄÄ¿ÂŒÊÇ+"+pathURL + fileName);
        if (File.Exists(pathURL + fileName))
        {
            //  Debug_our.LogError("ŽæÔÚ×ÊÔŽÎÄŒþ" + pathURL + fileName+"ÉŸ³ý£¡£¡");
            File.Delete(pathURL + fileName);
        }

        using (FileStream st = File.Open(pathURL + fileName, FileMode.OpenOrCreate))
        {
            // Debug_our.LogError(_rawName+"±£ŽæÍê±Ï");
            st.Write(datas, 0, datas.Length);
        }
        //Debug_our.LogError("×Ü¹²ÒªÏÂÔØµÄbyte£º"+TotalBytesLength);
        CurrentBytesLength += allResources[_rawName].length;
        currentNumberDowdlod++;
       // Debug_our.LogError(_rawName+"保存完毕");
        loadingStr = ((float)CurrentBytesLength / (float)TotalBytesLength).ToString() + currentNumberDowdlod + "/" + totalNumberNeedDowload + "个 " + (int)(CurrentBytesLength) + "/" + (TotalBytesLength) + "B";
        checkALLresoure();
        yield break;
    }
    private bool checkALLresoure()
    {
        if (DownloadQueue.Count == 0 && SaveQueue.Count == 0 && CurrentBytesLength == TotalBytesLength)
        {

            theResourceIsOK = true;
            loadingStr = "1" + "下载完成";
            if (this.callback != null)
            {
                // Debug_our.LogError("È«²¿ÎÄŒþ¶ŒÒÑŸ­ÏÂÔØÍê³É ¡£²¢ÇÒ±£ŽæÍê±Ï£¬³É¹Š»Øµ÷");
                this.callback();
            }
            return theResourceIsOK;
        }
        else
        {
            return false;
        }

    }


    private static void CreateZipFile(string filesPath, string zipFilePath)
    {

        if (!Directory.Exists(filesPath))
        {
            Console.WriteLine("Cannot find directory '{0}'", filesPath);
            return;
        }

        if (File.Exists(filesPath))
        {
            File.Delete(filesPath);
        }

        try
        {
            string[] filenames = Directory.GetFiles(filesPath);
            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFilePath)))
            {

                s.SetLevel(9); // Ñ¹ËõŒ¶±ð 0-9
                //s.Password = "123"; //ZipÑ¹ËõÎÄŒþÃÜÂë
                byte[] buffer = new byte[4096]; //»º³åÇøŽóÐ¡
                foreach (string file in filenames)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);
                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine(" Ñ¹ËõœáÊø");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during processing {0}", ex);
        }
    }

    private static byte[] UnZipFile(byte[] zipBytes)
    {
        byte[] datas = null;
        loadingStr = "0unzipTheResource +++00000";

        if (zipBytes == null)
        {
            loadingStr = "0unzipTheResource +++11";
            return null;
        }

        using (MemoryStream ms = new MemoryStream(zipBytes))
        {
           // LoadingControler.getSelf().showContent(0f, "unzipTheResource +++2222");
            using (ZipInputStream s = new ZipInputStream(ms))
            {
                ZipEntry theEntry;
             //   LoadingControler.getSelf().showContent(0f, "unzipTheResource +++333333333333333");
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (fileName != String.Empty)
                    {
                        datas = new byte[s.Length];
                        s.Read(datas, 0, datas.Length);
                    }
                }
            }
        }
        return datas;
    }

    // <summary>
    /// ŒÆËãÎÄŒþµÄMD5Öµ
    /// </summary>
    /// <param name="filepath">ÐèÒªŒÆËãµÄÎÄŒþÃû</param>
    /// <returns></returns>
    public static string FileMD5Value(String filepath)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] md5ch;
        using (FileStream fs = File.OpenRead(filepath))
        {
            md5ch = md5.ComputeHash(fs);
        }
        md5.Clear();
        string strMd5 = "";
        for (int i = 0; i < md5ch.Length; i++)
        {
            strMd5 += md5ch[i].ToString("x").PadLeft(2, '0');
        }
        return strMd5;
    }

    /// <summary>
    /// ŒÆËã×Ö·ûŽ®µÄMD5Öµ
    /// </summary>
    /// <param name="sDataIn">ÐèÒªŒÆËãµÄ×Ö·ûŽ®</param>
    /// <returns></returns>
    public static string StringMD5Value(string str)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] bytValue, bytHash;
        bytValue = System.Text.Encoding.UTF8.GetBytes(str);
        bytHash = md5.ComputeHash(bytValue);
        md5.Clear();
        string sTemp = "";
        for (int i = 0; i < bytHash.Length; i++)
        {
            sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
        }
        return sTemp.ToLower();
    }
    public static string bytesMD5Value(byte[] datas)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] bytHash;
        bytHash = md5.ComputeHash(datas);
        md5.Clear();
        string sTemp = "";
        for (int i = 0; i < bytHash.Length; i++)
        {
            sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
        }
        return sTemp.ToLower();
    }

    private const string PATH_SPLIT_CHAR = "\\";

    /// <summary>
    /// žŽÖÆÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþ,²»°üº¬×ÓÄ¿ÂŒŒ°×ÓÄ¿ÂŒÖÐµÄÎÄŒþ
    /// </summary>
    /// <param name="sourceDir">Ô­ÊŒÄ¿ÂŒ</param>
    /// <param name="targetDir">Ä¿±êÄ¿ÂŒ</param>
    /// <param name="overWrite">Èç¹ûÎªtrue,±íÊŸž²žÇÍ¬ÃûÎÄŒþ,·ñÔò²»ž²žÇ</param>
    public static void CopyFiles(string sourceDir, string targetDir, bool overWrite)
    {
        CopyFiles(sourceDir, targetDir, overWrite, false);
    }

    /// <summary>
    /// žŽÖÆÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþ
    /// </summary>
    /// <param name="sourceDir">Ô­ÊŒÄ¿ÂŒ</param>
    /// <param name="targetDir">Ä¿±êÄ¿ÂŒ</param>
    /// <param name="overWrite">Èç¹ûÎªtrue,ž²žÇÍ¬ÃûÎÄŒþ,·ñÔò²»ž²žÇ</param>
    /// <param name="copySubDir">Èç¹ûÎªtrue,°üº¬Ä¿ÂŒ,·ñÔò²»°üº¬</param>
    public static void CopyFiles(string sourceDir, string targetDir, bool overWrite, bool copySubDir)
    {
        //žŽÖÆµ±Ç°Ä¿ÂŒÎÄŒþ
        foreach (string sourceFileName in Directory.GetFiles(sourceDir))
        {
            string targetFileName = Path.Combine(targetDir, sourceFileName.Substring(sourceFileName.LastIndexOf(PATH_SPLIT_CHAR) + 1));

            if (File.Exists(targetFileName))
            {
                if (overWrite == true)
                {
                    File.SetAttributes(targetFileName, FileAttributes.Normal);
                    File.Copy(sourceFileName, targetFileName, overWrite);
                }
            }
            else
            {
                File.Copy(sourceFileName, targetFileName, overWrite);
            }
        }
        //žŽÖÆ×ÓÄ¿ÂŒ
        if (copySubDir)
        {
            foreach (string sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, sourceSubDir.Substring(sourceSubDir.LastIndexOf(PATH_SPLIT_CHAR) + 1));
                if (!Directory.Exists(targetSubDir))
                    Directory.CreateDirectory(targetSubDir);
                CopyFiles(sourceSubDir, targetSubDir, overWrite, true);
            }
        }
    }

    /// <summary>
    /// ŒôÇÐÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþ,²»°üº¬×ÓÄ¿ÂŒ
    /// </summary>
    /// <param name="sourceDir">Ô­ÊŒÄ¿ÂŒ</param>
    /// <param name="targetDir">Ä¿±êÄ¿ÂŒ</param>
    /// <param name="overWrite">Èç¹ûÎªtrue,ž²žÇÍ¬ÃûÎÄŒþ,·ñÔò²»ž²žÇ</param>
    public static void MoveFiles(string sourceDir, string targetDir, bool overWrite)
    {
        MoveFiles(sourceDir, targetDir, overWrite, false);
    }

    /// <summary>
    /// ŒôÇÐÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþ
    /// </summary>
    /// <param name="sourceDir">Ô­ÊŒÄ¿ÂŒ</param>
    /// <param name="targetDir">Ä¿±êÄ¿ÂŒ</param>
    /// <param name="overWrite">Èç¹ûÎªtrue,ž²žÇÍ¬ÃûÎÄŒþ,·ñÔò²»ž²žÇ</param>
    /// <param name="moveSubDir">Èç¹ûÎªtrue,°üº¬Ä¿ÂŒ,·ñÔò²»°üº¬</param>
    public static void MoveFiles(string sourceDir, string targetDir, bool overWrite, bool moveSubDir)
    {
        //ÒÆ¶¯µ±Ç°Ä¿ÂŒÎÄŒþ
        foreach (string sourceFileName in Directory.GetFiles(sourceDir))
        {
            string targetFileName = Path.Combine(targetDir, sourceFileName.Substring(sourceFileName.LastIndexOf(PATH_SPLIT_CHAR) + 1));
            if (File.Exists(targetFileName))
            {
                if (overWrite == true)
                {
                    File.SetAttributes(targetFileName, FileAttributes.Normal);
                    File.Delete(targetFileName);
                    File.Move(sourceFileName, targetFileName);
                }
            }
            else
            {
                File.Move(sourceFileName, targetFileName);
            }
        }
        if (moveSubDir)
        {
            foreach (string sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, sourceSubDir.Substring(sourceSubDir.LastIndexOf(PATH_SPLIT_CHAR) + 1));
                if (!Directory.Exists(targetSubDir))
                    Directory.CreateDirectory(targetSubDir);
                MoveFiles(sourceSubDir, targetSubDir, overWrite, true);
                Directory.Delete(sourceSubDir);
            }
        }
    }

    /// <summary>
    /// ÉŸ³ýÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþ£¬²»°üº¬×ÓÄ¿ÂŒ
    /// </summary>
    /// <param name="targetDir">²Ù×÷Ä¿ÂŒ</param>
    public static void DeleteFiles(string targetDir)
    {
        DeleteFiles(targetDir, false);
    }

    /// <summary>
    /// ÉŸ³ýÖž¶šÄ¿ÂŒµÄËùÓÐÎÄŒþºÍ×ÓÄ¿ÂŒ
    /// </summary>
    /// <param name="targetDir">²Ù×÷Ä¿ÂŒ</param>
    /// <param name="delSubDir">Èç¹ûÎªtrue,°üº¬¶Ô×ÓÄ¿ÂŒµÄ²Ù×÷</param>
    public static void DeleteFiles(string targetDir, bool delSubDir)
    {
        foreach (string fileName in Directory.GetFiles(targetDir))
        {
            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }
        if (delSubDir)
        {
            DirectoryInfo dir = new DirectoryInfo(targetDir);
            foreach (DirectoryInfo subDi in dir.GetDirectories())
            {
                DeleteFiles(subDi.FullName, true);
                subDi.Delete();
            }
        }
    }

    /// <summary>
    /// ŽŽœšÖž¶šÄ¿ÂŒ
    /// </summary>
    /// <param name="targetDir"></param>
    public static void CreateDirectory(string targetDir)
    {
        DirectoryInfo dir = new DirectoryInfo(targetDir);
        if (!dir.Exists)
            dir.Create();
    }

    /// <summary>
    /// œšÁ¢×ÓÄ¿ÂŒ
    /// </summary>
    /// <param name="targetDir">Ä¿ÂŒÂ·Ÿ¶</param>
    /// <param name="subDirName">×ÓÄ¿ÂŒÃû³Æ</param>
    public static void CreateDirectory(string parentDir, string subDirName)
    {
        CreateDirectory(parentDir + PATH_SPLIT_CHAR + subDirName);
    }

    /// <summary>
    /// ÉŸ³ýÖž¶šÄ¿ÂŒ
    /// </summary>
    /// <param name="targetDir">Ä¿ÂŒÂ·Ÿ¶</param>
    public static void DeleteDirectory(string targetDir)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(targetDir);
        if (dirInfo.Exists)
        {
            DeleteFiles(targetDir, true);
            dirInfo.Delete(true);
        }
    }

    /// <summary>
    /// ÉŸ³ýÖž¶šÄ¿ÂŒµÄËùÓÐ×ÓÄ¿ÂŒ,²»°üÀš¶Ôµ±Ç°Ä¿ÂŒÎÄŒþµÄÉŸ³ý
    /// </summary>
    /// <param name="targetDir">Ä¿ÂŒÂ·Ÿ¶</param>
    public static void DeleteSubDirectory(string targetDir)
    {
        foreach (string subDir in Directory.GetDirectories(targetDir))
        {
            DeleteDirectory(subDir);
        }
    }

    /// <summary>
    /// œ«Öž¶šÄ¿ÂŒÏÂµÄ×ÓÄ¿ÂŒºÍÎÄŒþÉú³ÉxmlÎÄµµ
    /// </summary>
    /// <param name="targetDir">žùÄ¿ÂŒ</param>
    /// <returns>·µ»ØXmlDocument¶ÔÏó</returns>
    public static XmlDocument CreateXml(string targetDir)
    {
        XmlDocument myDocument = new XmlDocument();
        XmlDeclaration declaration = myDocument.CreateXmlDeclaration("1.0", "utf-8", null);
        myDocument.AppendChild(declaration);
        XmlElement rootElement = myDocument.CreateElement(targetDir.Substring(targetDir.LastIndexOf(PATH_SPLIT_CHAR) + 1));
        myDocument.AppendChild(rootElement);
        foreach (string fileName in Directory.GetFiles(targetDir))
        {
            XmlElement childElement = myDocument.CreateElement("File");
            childElement.InnerText = fileName.Substring(fileName.LastIndexOf(PATH_SPLIT_CHAR) + 1);
            rootElement.AppendChild(childElement);
        }
        foreach (string directory in Directory.GetDirectories(targetDir))
        {
            XmlElement childElement = myDocument.CreateElement("Directory");
            childElement.SetAttribute("Name", directory.Substring(directory.LastIndexOf(PATH_SPLIT_CHAR) + 1));
            rootElement.AppendChild(childElement);
            CreateBranch(directory, childElement, myDocument);
        }
        return myDocument;
    }

    /// <summary>
    /// Éú³ÉXml·ÖÖ§
    /// </summary>
    /// <param name="targetDir">×ÓÄ¿ÂŒ</param>
    /// <param name="xmlNode">žžÄ¿ÂŒXmlDocument</param>
    /// <param name="myDocument">XmlDocument¶ÔÏó</param>
    private static void CreateBranch(string targetDir, XmlElement xmlNode, XmlDocument myDocument)
    {
        foreach (string fileName in Directory.GetFiles(targetDir))
        {
            XmlElement childElement = myDocument.CreateElement("File");
            childElement.InnerText = fileName.Substring(fileName.LastIndexOf(PATH_SPLIT_CHAR) + 1);
            xmlNode.AppendChild(childElement);
        }
        foreach (string directory in Directory.GetDirectories(targetDir))
        {
            XmlElement childElement = myDocument.CreateElement("Directory");
            childElement.SetAttribute("Name", directory.Substring(directory.LastIndexOf(PATH_SPLIT_CHAR) + 1));
            xmlNode.AppendChild(childElement);
            CreateBranch(directory, childElement, myDocument);
        }
    }
}

