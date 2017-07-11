using UnityEngine;
using System.Collections;

public class ResourceObject {

    public string OriginSourceName = "";
    public string Guid = "";
    public string MD5 = "";
    public long length = 0;
   public  EnumResourcePostion Postion=EnumResourcePostion.NONE;
    public ResourceObject(string Oname,string Guid, string MD5,long length,EnumResourcePostion position) {

        this.OriginSourceName = Oname;
        this.Guid = Guid;
        this.MD5 = MD5;
        this.length = length;
        this.Postion = position;
    }
}

public enum EnumResourcePostion
{
    NONE,
    OUTER,
    INTERNAL,
}