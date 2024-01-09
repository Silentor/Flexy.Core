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
		private static void StaticClear( ) { _global = null; _sceneToCtxRegistry.Clear( ); SceneManager.sceneUnloaded -= ClearSceneFromRegistry; SceneManager.sceneUnloaded += ClearSceneFromRegistry; }
		
		[SerializeField]	String			_name;
		[SerializeField]	GameObject		_services;
		
		protected static	GameContext		_global;
		protected			GameContext		_parent;
		
		protected static readonly	Dictionary<Scene, GameContext>	_sceneToCtxRegistry = new ( );
		private readonly			Dictionary<Type, Object>		_registeredServices	= new ( );
		
		public static	GameContext		Global					=> _global ? _global : (_global = CreateGlobalContext());
			
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject.scene );
		public static 	GameContext		GetCtx					( Scene scene )		=> _sceneToCtxRegistry.TryGetValue( scene, out var ctx ) ? ctx : Global;
		public			void			RegisterGameScene		( Scene scene )		=> _sceneToCtxRegistry[scene] = this;
		
		public String					Name					=> _name;
		public RoDict<Type, Object>		RegisteredServices		=> _registeredServices;
		
		protected		void			Awake					( )		
		{
			if( _global == null )
			{
				_global = this;
				RegisterGameScene( gameObject.scene ); //Register before move to another scene
				DontDestroyOnLoad( gameObject );
			}
			else
			{
				_parent = GetCtx(this);
			}
			
			RegisterGameScene( gameObject.scene );
			RegisterGameScene( _global.gameObject.scene );
			
			if( String.IsNullOrWhiteSpace( _name ) )
				_name = gameObject.name;
			
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
			var typeBase	= typeof(TBindFrom);
			
			//Do not work with custom base types like MonoBehEx
			//Consider use ServiceInterfaceAttribute 
			//if( typeBase == typeActual || typeBase == typeof(MonoBehaviour) || typeBase == typeof(Component) || typeBase == typeof(UnityEngine.Object) || typeBase == typeof(ScriptableObject) || typeBase == typeof(object) )
			//typeBase = typeActual.BaseType;
				
			if( typeBase == typeof(MonoBehaviour) || typeBase == typeof(Component) || typeBase == typeof(UnityEngine.Object) || typeBase == typeof(ScriptableObject) || typeBase == typeof(object) )
				typeBase = typeActual;
			
			Debug.Log	( $"[GameWorldBase] - SetService: {typeActual.Name} → {typeActual.Name}" );
			
			_registeredServices.Add( typeActual, service );
			
			if( typeBase != typeActual )     
			{
				Debug.Log	( $"[GameWorldBase] - SetService: {typeBase.Name} → {typeActual.Name}" );
				
				try						{ _registeredServices.Add( typeBase, service ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
			}
				
			if( typeActual.GetCustomAttribute<ServiceInterfaceAttribute>( ) is {} si )
			{
				foreach( var serviceType in si.InterfaceType )
				{
					if ( !serviceType.IsAssignableFrom( typeActual ) ) 
						continue;
					
					Debug.Log	( $"[GameWorldBase] - SetService: {si.InterfaceType} → {typeActual.Name}" );
					_registeredServices.Add( serviceType, service );
				}
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
		private static 	void			ClearSceneFromRegistry	( Scene scene )										
		{
			_sceneToCtxRegistry.Remove( scene );
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
				DrawCtx( this );
				GUILayout.Space( 5 );
				DrawCtx( _parent );
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