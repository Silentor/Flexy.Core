namespace Flexy.Core;

public static class Constants
{	
	#if UNITY_EDITOR || DEVELOPMENT_BUILD || DEBUG
	public const Boolean	IsDevBuildOrEditor =	true;
	#else
	public const Boolean	IsDevBuildOrEditor =	false;
	#endif
	
	
	
	public const Boolean	IsClient	= !Constants.IsServer;
	public const Boolean	IsServer	= IsUnityServer || NetCore;
	
	#if RELEASE_BUILD || RELEASE
	public const Boolean	IsReleaseBuild =	true;
	#else
	public const Boolean	IsReleaseBuild =	false;
	#endif
	
	#if BETA_BUILD
	public const Boolean	IsBetaBuild =	true;
	#else
	public const Boolean	IsBetaBuild =	false;
	#endif
	
	#if FORCE_LOCAL_ONLINE                              
	public const Boolean	ForceLocalOnline =	true;
	#else
	public const Boolean	ForceLocalOnline =	false;
	#endif
	
	#if DISABLE_LOGS
	public const Boolean	DisableLogs =	true;
	#else
	public const Boolean	DisableLogs =	false;
	#endif
	
	#if ENABLE_CHEATS
	public const Boolean	EnableCheats =	true;
	#else
	public const Boolean	EnableCheats =	false;
	#endif
	
	
	//
	// Platforms
	//
	
	#if NETCOREAPP
	public const Boolean	NetCore = true;
	#else
	public const Boolean	NetCore = false;
	#endif
	
	#if UNITY_EDITOR
	public const Boolean	IsEditor = true;
	#else
	public const Boolean	IsEditor = false;
	#endif
	
	#if UNITY_SERVER 
	public const Boolean	IsUnityServer = true;
	#else
	public const Boolean	IsUnityServer = false;
	#endif
	
	#if UNITY_ANDROID
	public const Boolean	IsAndroid =	true;
	#else
	public const Boolean	IsAndroid =	false;
	#endif
	
	#if UNITY_IOS
	public const Boolean	IsIos =	true;
	#else
	public const Boolean	IsIos =	false;
	#endif
	
	#if UNITY_STANDALONE
	public const Boolean	IsStandalone = true;
	#else
	public const Boolean	IsStandalone = false;
	#endif

	#if UNITY_STANDALONE_WIN
	public const Boolean	IsStandaloneWin = true;
	#else
    public const Boolean	IsStandaloneWin = false;
	#endif

	#if UNITY_STANDALONE_OSX
	public const Boolean	IsStandaloneOsx = true;
	#else
	public const Boolean	IsStandaloneOsx = false;
	#endif
    
	#if UNITY_WSA
	public const Boolean	IsUWP =	true;
	#else
	public const Boolean	IsUWP =	false;
	#endif

	public const Boolean	IsStandaloneOrUWP	= IsStandalone || IsUWP;
	public const Boolean	IsMobile			= IsAndroid || IsIos;
}