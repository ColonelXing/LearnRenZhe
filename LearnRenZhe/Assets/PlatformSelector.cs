using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_IOS
public class AndroidJavaClass: System.IDisposable {
	public AndroidJavaClass (string name){
		
	}
	
	public T GetStatic<T>(string name) where T : new()
	{
		return new T();
	}	
	
	public T CallStatic<T>(string name) where T : new()
	{
		return new T();
	}	
	
	public void Dispose(){
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	protected virtual void Dispose(bool disposing){
		
	}
}
public class AndroidJavaObject : System.IDisposable{
	public T Call<T>(string name) { 
		return default(T);
	}
	
	public void Call(string x, params object[] args){}
	public T GetStatic<T>(string name) {
		return default(T);
	}	
	
	public IntPtr GetRawObject(){
		return new IntPtr();
	}
	
	public void Dispose(){
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	protected virtual void Dispose(bool disposing){
		
	}
}

public class AndroidJavaRunnable {
	public AndroidJavaRunnable(Action action){}
}

#endif
public class PlatformSelector : MonoBehaviour
{
    public enum platformTypes
    {
        None,
        LOCAL,
        WANDOUJIA,
        _91,
        TENGXUN,
        AIBEI,
        UC,
        YiJieBaidu,
        IOS_91,
        IOS_PP,
        IOS_HAIMA,
        IOS_TBTUI,
        IOS_AIBEI,
        IOS_ITOOLS,
        IOS_KUAIYONG,
        IOS_AIBEIXY,
        QIHOO360,
        CHANGBA,
        WANDOUJIA_NEW,

        ANDROID_CHUKONG,
        ANDROID_2345,
        IOS_CHANGBA,
        ANDROID_ZHONGHUAWANNIANLI,
        ANDROID_ZHUODASHI,
		IOS_TAILANDAPPSTORE,
		IOS_APPFAME,
		iOS_YunDing_RenZheLianMeng,//忍者联盟
		iOS_YunDing_Portuguese,//云顶葡萄牙   云顶SDK登录，苹果支付
        ANDROID_TAILAND,
        ANDROID_VIETAM,
        ANDROID_TAILAND_GOOGLE_PLAY,
        ANDROID_TAILAND_GOOGLE_PLAY_FIXED_BUG,
		IOS_VIETAMAPPSTORE,
		IOS_VIETAM_PRISON_BREAK,
		IOS_APPFAME_SGP,
		IOS_APPFAME_YY,
		IOS_APPFAME_COMPLEX_FONT,
		IOS_APPFAME_COMPLEX_FONT2,
        //-----------------------------------------------------------------------------------------新接的SDK请放到下面                                                       20170613-------------------------------------------->
        /// <summary>
        /// 易接游侠, 易接智轩,易接炫游
        /// </summary>
        YIJIE,
        /// <summary>
        /// 易接智轩
        /// </summary>
   //     YIJIE_ZHIXUAN,
		/// <summary>
		///易接 SDK IOS
		/// </summary>
		IOS_YiJieSDKiOS,
        /// <summary>
        /// 鸿游
        /// </summary>
        Android_HongYou,
        /// <summary>
        /// //忍者大乱斗,忍者传说  ----------------->创星
        /// </summary>
        IOS_ChuangXing,
        /// <summary>
        /// 木叶小忍者,雾隐小忍者,查克拉世界,卡卡西の番外篇----------->汉风
        /// </summary>
        IOS_HanFeng,
        /// <summary>
        /// 浮空岛----- 忍者大战,   登陆用了游侠的SDK ，
        /// </summary>
        IOS_FuKongDao,

        NEW_A_YXF,
		IOS_YOUXIA
    }

    [HideInInspector]
	public platformTypes type=platformTypes.LOCAL;

	public enum PlatformChildTypes { 
//#if ChuangXingiOS
		RenZheChuanShuo,
		RenZheDaLuanDou,
//#elif YiJieSDKiOS
		YouXia,
		ZhiXuan,
		XuanYou,

        IOS_HanFeng_MuYe,// 木叶小忍者
        IOS_HanFeng_WuYin,//雾隐小忍者
        IOS_HanFeng_ChaKeLa,//,查克拉世界
        IOS_HanFeng_KaKaXi,//卡卡西の番外篇

//#else
		None
//#endif
	}
    
    public static PlatformSelector Instance {
        get {
            if (_instance == null) { 
				GameObject gm  = null;
#if TailandIOSAppStore
				gm =new GameObject("TailandIOSAppStore");
				_instance =  gm.AddComponent("TailandIOSAppStoreControl");
                _instance.type = platformTypes.IOS_TAILANDAPPSTORE;
#elif YunDingiOSPortugueseController
				gm = new GameObject("YunDingiOSPortugueseController");
				_instance =  gm.AddComponent("YunDingiOSPortugueseController");
                _instance.type = platformTypes.iOS_YunDing_Portuguese;
#elif ChuangXingiOS
				gm = new GameObject("ChuangXingiOS");
				_instance = gm.AddComponent<ChuangXingiOS>();
                _instance.type = platformTypes.IOS_ChuangXing;
#elif FuKongDaoiOS
				gm = new GameObject("FuKongDaoiOS");
				_instance = gm.AddComponent<FuKongDaoiOS>();
                _instance.type = platformTypes.IOS_FuKongDao;
#elif YiJieSDK
                 gm = new GameObject("YiJieSDK");
                _instance = gm.AddComponent<YiJieSDK>();
                _instance.type = platformTypes.YIJIE;
#elif YiJieSDKiOS
				 gm = new GameObject("YiJieSDKiOS");
				_instance = gm.AddComponent<YiJieSDKiOS>();
				_instance.type = platformTypes.IOS_YiJieSDKiOS;
#elif YIJIE_ZHIXUAN
                   gm = new GameObject("YiJieSDK");
                _instance = gm.AddComponent<YiJieSDK>();
                _instance.type = platformTypes.YIJIE_ZHIXUAN;
#elif AndroidSDKHongYou
                 gm = new GameObject("AndroidSDKHongYou");
                 _instance = gm.AddComponent<AndroidSDKHongYou>();
                _instance.type = platformTypes.Android_HongYou;
#elif AndroidSDKYXF
                  gm = new GameObject("AndroidSDKYXF");
                _instance = gm.AddComponent<AndroidSDKYXF>();
                _instance.type = platformTypes.NEW_A_YXF;
#else
                   gm = new GameObject("PlatformSelector");
                _instance = gm.AddComponent<PlatformSelector>();
                 _instance.type = platformTypes.LOCAL;
#endif
				DontDestroyOnLoad(gm);
            }
            return _instance;
        }
    }
    private static PlatformSelector _instance = null;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init() { Debug.Log("PlatformSelector->type=" + type); }

    [HideInInspector] 
    public int PlatformID = -1;
    [HideInInspector]
    public bool CurrentPlatformSupportTheBackKey = false;

    public virtual bool IsLogined {
        get;
        set;
    }
    /// <summary>
    /// 渠道ID
    /// </summary>
    public virtual string GetChannelID() { return ""; }

    /// <summary>
    /// 获取包名
    /// </summary>
    /// <returns></returns>
    public virtual string GetBundleID() { return ""; }
	public Action OnSDKInited = null;

	public virtual void InitSDK(params object[] args) { }
    public virtual void Login(params object[] args) { }
    public virtual void Logout(params object[] args) { }
    public virtual void Purchase(params object[] args) { }
    //创建角色时
    public virtual void OnCreateRole(params object[] args) { }
    //当角色更新时
    public virtual void UpdateRole(params object[] args) { }
    public virtual void ExitSDK(params object[] args) { }

    public virtual void SetData(params object[] args) { }
    public virtual void LoginAgain(params object[] args) { }


	/// <summary>
	/// 显示第三方充值界面
	/// </summary>
	public delegate void VipCZDelegate(object  data);
	public static event VipCZDelegate   VipCZEvent;
	public static void VipCZSDK(object  data)
	{
		if(VipCZEvent !=null)
			VipCZEvent(data);
	}
	/// <summary>
	/// 显示第三方登录界面
	/// </summary>
	public delegate void LoginSDKDelegate(params object[] args);
	public static event LoginSDKDelegate LoginSDKEvent;
	public static void LoginSDK()
	{
		if(LoginSDKEvent !=null)
			LoginSDKEvent();
	}

	public delegate void LogOutSDKDelegate();
	public static event LogOutSDKDelegate LogOutSDKEvent;

	public static void LogOutSDK(){

		if(LogOutSDKEvent!= null)
		{
			LogOutSDKEvent();
		}
	}

	public delegate void PlayerCenterSDKDelegate();
	public static event PlayerCenterSDKDelegate PlayerCenterSDKEvent;
	public static void PlayerCenterSDK()
	{
		if(PlayerCenterSDKEvent != null)
		{
			PlayerCenterSDKEvent();
		}
	}
	public static bool HasPlayerCenter()
	{
		return (PlayerCenterSDKEvent != null);
	}
}
