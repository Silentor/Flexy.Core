using Flexy.Utils;

namespace Flexy.Core
{
	public class Analytics
	{
		[InitializeOnLoadMethod]
		private static void Init ( ) { _backends = new( ); ComputeInstallId( ); }
		
		private static	String					_deviceVendorId;
		private static	List<IBackend>			_backends;
		
		public static	String	AppVersion		=> Application.version;
		public static	String	DeviceVendorId	=> _deviceVendorId;

		public static	void	AddBackend( IBackend  backend) => _backends.Add( backend );
		
		public static	void	Send			( String category, String name, Int32 type, Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null )
		{
			foreach ( var backend in _backends )
			{
				try						{ backend.Send( category, name, type, value1, value2, str1, str2, additionalData ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }	
			}
		}
		
		// Helpers
		public static	void	SendBoot		( String name, EProgress ep,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Boot,		name, (Int32)ep, 		value1, value2, str1, str2, additionalData );
		public static	void	SendTutorial	( String name, EProgress ep,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Tutorial,	name, (Int32)ep,		value1, value2, str1, str2, additionalData );
		public static	void	SendProgress	( String name, EProgress ep,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Progress,	name, (Int32)ep,		value1, value2, str1, str2, additionalData );
		
		public static	void	SendResource	( String name, Boolean spend,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Resource,	name, spend ? 1 : 0,	value1, value2, str1, str2, additionalData );
		public static	void	SendAds			( String name, EProgress ep,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Ads,		name, (Int32)ep, 		value1, value2, str1, str2, additionalData );
		public static	void	SendIap			( String name, EProgress ep,	Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Iap,		name, (Int32)ep,		value1, value2, str1, str2, additionalData );
		
		public static	void	SendUI			( String name, 					Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.UI,			name, 0, 				value1, value2, str1, str2, additionalData );
		
		public static	void	SendMetagame	( String name, 					Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Metagame,	name, 0,				value1, value2, str1, str2, additionalData );
		public static	void	SendCoregame	( String name, 					Single value1 = 0, Single value2 = 0, String str1 = null, String str2 = null, IDictionary<String, Object> additionalData = null ) => Send( Category.Coregame,	name, 0,				value1, value2, str1, str2, additionalData );

		
		private static	void	ComputeInstallId( )	
		{
			if (Constants.IsServer)
			{
				_deviceVendorId = Guid.Empty.ToString(  );
				return;
			}

			Debug.Log		( $"[Analytics] - Init" );
			
			_deviceVendorId	= SystemInfo.deviceUniqueIdentifier;

			var id			= _deviceVendorId;
			Debug.Log		( $"[Analytics] - Raw Device Vendor Id: {id}" );

			id				= SystemInfo.deviceUniqueIdentifier.Replace("-", "")[..32];
			Debug.Log		( $"[Analytics] - Cropped Device Vendor Id: {id}" );
			
			_deviceVendorId	= Guid.Parse( id ).ToString(); 
			Debug.Log		( $"[Analytics] - Device Vendor Guid: {id}" );
		}
		
		//id			= $"{id[0..8]}-{id[8..12]}-{id[12..16]}-{id[16..20]}-{id[20..32]}";
		
		public interface IBackend
		{
			public void Send( String category, String name, Int32 type, Single value1, Single value2, String str1, String str2, IDictionary<String, Object> additionalData );
		}
		
		public static class Category
		{
			public const String Boot		= "Boot";
			public const String Tutorial	= "Tutorial";
			public const String Progress	= "Progress";
			
			public const String Resource	= "Resource";
			public const String Ads			= "Ads";
			public const String Iap			= "Iap";
			
			public const String UI			= "UI";
			
			public const String Metagame	= "Metagame";
			public const String Coregame	= "Coregame";
		}
	}
	
	public enum EProgress: Byte
	{
		Start		= 1,
		Complete	= 2,
		Fail		= 3
	}
}
