using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class WwwWorkpool : MonoBehaviour
{
    public static WwwWorkpool instance = null;
    public  int  maxCapcity = 2;

    private void Init()
    {
        for (int i = 0; i < maxCapcity; i++)
        {
            workItems.Add(new WwwWorkItem(""));
        }
    }

    private SecurityLinkList<workTarget> targets= new SecurityLinkList<workTarget>();

    private void Awake()
    {
        instance = this;
        Init();
    }

    private void startDowload(Func<IEnumerator> aFunc )
    {
        this.StartCoroutine(aFunc());
    }

    public class WwwWorkItem
    {
        public enum EnumWwwStatus
        {
            ready,
            working,
        }

        public WwwWorkItem(string url)
        {
            this.url = url;
        }

        public string url = "";
        public WWW www = null;
        public EnumWwwStatus wwwStatus = EnumWwwStatus.ready;
        private byte[] privateResource = null;
        public byte[] Resource
        {
           private set { this.privateResource = value; }
           get { return this.privateResource; }
        }

        public void MystartDownload(string url)
        {
            this.url = url;
           WwwWorkpool.instance.startDowload(Download);   
        }

        private IEnumerator Download()
        {
            if (this.wwwStatus == EnumWwwStatus.working)
            {
                yield break;
            }
            if (www != null)
            {
                www.Dispose();
               // www.assetBundle.Unload(true);
            }
            this.wwwStatus=EnumWwwStatus.working;

            Debug.LogError("开始下载》》》"+this.url);
            www=new WWW(this.url);
            yield return www;

            if (www.isDone && www.error == null)
            {
                Debug.LogError("现在完》》》》》" + this.url);
                this.Resource = www.bytes;
                WwwWorkpool.instance.notify(this,EnumMessageType.DownSuccess);
                Dispose();
                this.wwwStatus=EnumWwwStatus.ready;
            }
            else if (www.isDone && www.error != null)
            {
               Debug.LogError("现在过程中出现错误》》》》》" + this.url);
               WwwWorkpool.instance.notify(this,EnumMessageType.HasError);
                Dispose();
                this.wwwStatus = EnumWwwStatus.ready;
            }
           WwwWorkpool.instance.checkTheQueueAndWork();
        }

        public void Dispose()
        {
            if (www != null)
            {
                www.Dispose();
                //www.assetBundle.Unload(true);
                www = null;
            }
        }
    }

    public enum EnumMessageType
    {
       DownSuccess,
       HasError,
    }
    private void notify(WwwWorkItem wwwWorkItem,EnumMessageType message)
    {
            switch (message)
            {
                case EnumMessageType.DownSuccess:
                    if (links.ContainsKey(wwwWorkItem))
                    {
                        if (links[wwwWorkItem] != null)
                        {
                            if (links[wwwWorkItem].successCallBack != null)
                            {
                                links[wwwWorkItem].successCallBack(wwwWorkItem.Resource);
                            }
                        }
                    }
                    break;
                case EnumMessageType.HasError:
                    if (links.ContainsKey(wwwWorkItem))
                    {
                        if (links[wwwWorkItem] != null)
                        {
                            string url = links[wwwWorkItem].url;
                            System.Action<byte[]> callback = links[wwwWorkItem].successCallBack;
                            targets.AddLast(new workTarget(url, callback));
                        }
                    }
                    break;
            }     
    }

    private  class  workTarget
    {
        public string url;

        public  System.Action<byte[]> successCallBack;

        public workTarget(string url,System.Action<byte[]> successCallBack)
        {
            this.url = url;
            this.successCallBack = successCallBack;
        }


    }
        
        
    private Dictionary<WwwWorkItem,workTarget> links=new Dictionary<WwwWorkItem, workTarget>();
    private  List<WwwWorkItem> workItems  =new List<WwwWorkItem>(3);
    public void addwork(string url,System.Action<byte[]> sccucessCallback)
    {
        targets.AddFirst(new workTarget(url,sccucessCallback));
        checkTheQueueAndWork();
    }

    private void  checkTheQueueAndWork()
    {
        if (targets.Count <= 0)
        {
            Debug.LogError("队列中 已经没有要下载的啦");
            return;
        }
        else
        {
            foreach (var wwwWorkItem in workItems)
            {
                if (wwwWorkItem.wwwStatus == WwwWorkItem.EnumWwwStatus.ready)
                {
                   Debug.LogError("我发现有》》》》空闲《《《《《www 单位，");
                    if (targets.Count > 0)
                    {
                        workTarget target = targets.Last();
                        targets.RemoveLast();
                        if (links.ContainsKey(wwwWorkItem))
                        {
                            links.Remove(wwwWorkItem);
                        }
                        links.Add(wwwWorkItem, target);
                        wwwWorkItem.MystartDownload(target.url);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
