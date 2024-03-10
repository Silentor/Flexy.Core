using System.Linq;
using System.Reflection;
using Flexy.Utils;
using UnityEngine.SceneManagement;

namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+2)]
	public class GameContext : MonoBehaviour
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void StaticClear( ) 
		{
			_global = null; 
			_sceneToCtxRegistry.Clear( ); 
			SceneManager.sceneUnloaded -= ClearSceneRegistration; 
			SceneManager.sceneUnloaded += ClearSceneRegistration; 
			
			SceneManager.activeSceneChanged -= RegisterCreatedScene; 
			SceneManager.activeSceneChanged += RegisterCreatedScene;
			
			SceneManager.sceneLoaded -= RegisterSideLoadedScene; 
			SceneManager.sceneLoaded += RegisterSideLoadedScene;
		}
		
		[SerializeField]	String			_name;
		[SerializeField]	GameObject		_services;
		public				Boolean			AllowAutoSceneRegistration;
		protected			Boolean			_allowRegisterAllScenesOnFirstGameFrame;
		
		protected static	GameContext		_global;
		protected			GameContext		_parent;
		
		protected static readonly	Dictionary<Scene, GameContext>	_sceneToCtxRegistry = new ( );
		private readonly			Dictionary<Type, Object>		_registeredServices	= new ( );
		
		public static	GameContext		Global					=> _global.OrNull( ) ?? (_global = CreateGlobalContext());
			
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject.scene );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			RegisterGameScene		( Scene scene )
		{
			Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register scene: {scene.name}" );
			_sceneToCtxRegistry[scene] = this;
		}
		
		public String					Name					=> _name;
		public RoDict<Type, Object>		RegisteredServices		=> _registeredServices;
		
		protected		void			Awake					( )		
		{
			if( _global == null )
			{
				_global = this;
				AllowAutoSceneRegistration = true;
				_allowRegisterAllScenesOnFirstGameFrame = true;
				DontDestroyOnLoad( gameObject );
			}
			else if ( _parent == null )
			{
				_parent = GetCtx(this);
			}
			
			if( String.IsNullOrWhiteSpace( _name ) )
				_name = gameObject.name;
			
			Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Awake" );
			
			if( AllowAutoSceneRegistration )
			{
				if( _allowRegisterAllScenesOnFirstGameFrame & Time.frameCount == 0 )
				{
					Debug.Log( $"{Time.frameCount} [GameCtx] {_name} - Register All Scenes" );
					RegisterGameScene( _global.gameObject.scene );
					var count = SceneManager.sceneCount;
					for ( var i = 0; i < count; i++ )
						RegisterGameScene( SceneManager.GetSceneAt( i ) );
				}
				else
				{
					RegisterGameScene( gameObject.scene );
				}
			}
			
			if( _services )
			{
				AddServicesFromObject( _services );
			
				InitializeServices		( this );
				InitializeAsyncServices	( this ).Forget( Debug.LogException );
				
				static			void			InitializeServices				( GameContext ctx )		
				{
					foreach ( var service in ctx._registeredServices.Values.ToArray().OfType<IService>( ) )
					{
						try						{ service.OrderedInit( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
				}
				static async	UniTask			InitializeAsyncServices			( GameContext ctx )		
				{
					foreach ( var service in ctx._registeredServices.Values.ToArray().OfType<IServiceAsync>( ) )
					{
						try						{ await service.OrderedInitAsync( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
				}
			}
		}

		

		protected		void			OnDestroy				( )		
		{
			if ( !_parent ) 
				return;
			
			foreach ( var pair in _sceneToCtxRegistry.ToArray( ) )
			{
				if( pair.Value == this )
					_sceneToCtxRegistry[pair.Key] = _parent;
			}
		}
		
		public			void			SetName					( String newName )									
		{
			_name = newName;
		}
		public			void			SetParent				( GameContext ctx )									
		{
			_parent = ctx;
		}
		public 			void			AddServicesFromObject	( GameObject go, Boolean allHierarchy = true )		
		{
			var services = allHierarchy ? go.GetComponentsInChildren<MonoBehaviour>( ) : go.GetComponents<MonoBehaviour>( );  
		
			foreach ( var service in services )
			{
				if ( !service )	//Probably script class was defined out on this platform
					continue;

				if ( service is ServiceProvider sp )
					sp.ProvideServices( this );
				else
					SetService( service );
			}
		}
		public			T				GetService<T>			( )						where T : class 			
		{
			if( _registeredServices.TryGetValue( typeof(T), out var svc ) )
				return svc as T;

			if( _parent )
				return _parent.GetService<T>( );
			
			return default;
		}
		public			void			SetService<TBindFrom>	( TBindFrom service )	where TBindFrom : class		
		{
			if( service == null )
				return;
			
			var typeActual	= service.GetType();
			
			if( typeActual.GetCustomAttribute<ServiceInterfaceAttribute>( ) is {} si )
			{
				foreach( var serviceType in si.InterfaceType )
				{
					if ( !serviceType.IsAssignableFrom( typeActual ) ) 
						continue;
					
					Debug.Log	( $"[GameCtx] {Name} - SetService: {si.InterfaceType} => {typeActual.Name}" );
					_registeredServices.Add( serviceType, service );
				}
			}
			else
			{
				Debug.Log	( $"[GameCtx] {Name} - SetService: {typeActual.Name}" );
				_registeredServices.Add( typeActual, service );
			}
		}
		
		private static	GameContext		CreateGlobalContext		( )													
		{
			var go = new GameObject( "Flexy Global Game Context", typeof(GameContext) );
			DontDestroyOnLoad( go );
			
			var ctx = go.GetComponent<GameContext>( );
			ctx._name = "Flexy Global Game Context";
			
			return ctx;
		}
		private static 	void			ClearSceneRegistration	( Scene scene )										
		{
			Debug.Log( $"{Time.frameCount} [GameCtx] ClearSceneRegistration {scene.name}" );
			_sceneToCtxRegistry.Remove( scene );
		}
		private static 	void			RegisterCreatedScene	( Scene oldScene, Scene newScene )					
		{
			if( _sceneToCtxRegistry.ContainsKey( oldScene ) && !_sceneToCtxRegistry.ContainsKey( newScene ) )
            {
				var ctx = _sceneToCtxRegistry[oldScene];
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.Name} - Register created scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		private static 	void			RegisterSideLoadedScene	( Scene newScene, LoadSceneMode loadSceneMode )		
		{
			if( !_sceneToCtxRegistry.ContainsKey( newScene ) )
			{
				var scene	= SceneManager.GetActiveScene( );
				var ctx		= _sceneToCtxRegistry[scene]; 
				Debug.Log( $"{Time.frameCount} [GameCtx] {ctx.Name} - Register Side loaded scene: {newScene.name}" );
				ctx.RegisterGameScene( newScene );
			}
		}
		
		public static class Internal
		{
			public static 	T		GetCtxInParent<T>			( Scene scene ) where T: GameContext				
			{
				if ( !_sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ) 
					return default;
				
				while ( ctx )
				{
					if ( ctx is T tctx )
						return tctx;
					
					ctx = ctx._parent;
				}
				
				return default;
			}
		}
		
		#if UNITY_EDITOR
		[RuntimeInspectorUI( Repaint = true )]
		public void RuntimeGUI	( )		
		{
			if( !Application.isPlaying )
				return;
			
			GUILayout.Space( 10 );
			GUILayout.Label( "Registered Services:" );
			
			if( _parent )
			{
				GUILayout.Space( 5 );
				DrawCtxAndParentLine( this );
			}
			else
			{
				var ctxs = FindObjectsByType<GameContext>( FindObjectsInactive.Include, FindObjectsSortMode.None );

				Array.Sort( ctxs, ( l, r ) => IsInParent( l, r ) ? -1 : 1 );
				
				static Boolean IsInParent( GameContext l, GameContext r )
				{
					for ( var ctx = l._parent; ctx != null; ctx = ctx._parent )
						if( ctx == r )
							return true;
					
					return false;
				}
				
				foreach ( var context in ctxs )
				{
					GUILayout.Space( 5 );
					DrawCtx( context );
				}
				
				
				GUILayout.Space( 10 );
				GUILayout.Label("Scene To Ctx");
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginVertical( );

					foreach ( var pair in _sceneToCtxRegistry )
					{
						GUILayout.Label( $"{pair.Key.name} => {pair.Value.Name}" );
					}
					GUILayout.EndVertical( );
				}
				GUILayout.EndHorizontal( );
			}
			
			static void DrawCtxAndParentLine( GameContext ctx )
			{
				DrawCtx( ctx );
				
				if( ctx._parent != null )
				{
					GUILayout.Space( 5 );
					DrawCtxAndParentLine( ctx._parent );
				}
			}
			static void DrawCtx( GameContext ctx )
			{
				GUILayout.Label(ctx.Name);
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginHorizontal( );
					GUILayout.BeginVertical();
					{
						foreach ( var pair in ctx._registeredServices )
						{
							var key			= pair.Key.Name;
							var name		= pair.Value.GetType().Name;
							
							GUILayout.Label( key != name ? $"{key} => {name}" : $"{name}" );
						}
					}
					GUILayout.EndVertical();
					// GUILayout.Space( 100 );
					// GUILayout.BeginVertical();
					// {
					// 	foreach ( var pair in ctx._registeredServices )
					// 	{
					// 		var name		= pair.Value.GetType().Name;
					// 		var fullName	= pair.Value.GetType().FullName;
					//
					// 		GUILayout.Label( $"ns: {fullName.Replace(name, "")}" );
					// 	}
					// }
					// GUILayout.EndVertical();
					// GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				GUILayout.EndHorizontal();
			}
		}
		#endif
	}

	public static class GameContextExt
	{
		public static T GetService<T>( this GameObject context )	where T:class => context.scene.GetService<T>( );
		public static T GetService<T>( this MonoBehaviour context )	where T:class => context.gameObject.scene.GetService<T>( );
		public static T GetService<T>( this Scene context )			where T:class => GameContext.GetCtx( context ).GetService<T>();
	}
}