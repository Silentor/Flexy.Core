using System.Linq;
using System.Reflection;
using Flexy.Utils;
using Flexy.Utils.ReadOnlyWrappers;
using UnityEngine.SceneManagement;

namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+2)]
	public class GameContext : MonoBehaviour
	{
		private static	GameContext		_global;
		private			GameContext		_parent;

		[SerializeField]	String		_name;
		[SerializeField]	GameObject	_services;
		
		public		GameContext			GlobalGameContext	=> _subContexts[0];
		public		GameContext			TopGameContext		=> _subContexts[^1];
		
		[InitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void StaticClear( ) =>  _sceneToCtxRegistry	.Clear( );
		private static readonly  Dictionary<Int32, GameContext>	_sceneToCtxRegistry = new ( );
		
		public static 	GameContext		GetCtx					( GameObject go )	=> GetCtx( go.scene );
		public static 	GameContext		GetCtx					( Component c )		=> GetCtx( c.gameObject.scene );
		public static 	GameContext		GetCtx					( Scene scene )			
		{
			return _sceneToCtxRegistry.TryGetValue(scene.handle, out var ctx) ? ctx._subContexts[^1] : null;
		}
		public			void			RegisterGameScene		( Scene scene )			
		{
			if( !_sceneToCtxRegistry.TryGetValue( scene.handle, out var ctx) )
			{
				_sceneToCtxRegistry.Add( scene.handle, this );
			}
			else
			{
				ctx._subContexts.Add( this );
			}
		}
		
		private readonly List<Object>				_registeredServices		= new ( );
		private readonly Dictionary<Type, Object>	_registeredServicesDict	= new ( );
		private readonly List<GameContext>			_subContexts			= new ( );

		public static GameContext	Global					=> _global; 
		public String				Name					=> _name;
		public RoList<Object>		RegisteredServices		=> _registeredServices;
		public RoDict<Type, Object> RegisteredServicesDict	=> _registeredServicesDict;
		
		public	void	SetName		( String name ) => _name = name;

		protected void	Awake		( )		
		{
			if( _global == null )
			{
				_global = this;
				DontDestroyOnLoad( gameObject );
				if( Time.timeScale == 0 )
					Time.timeScale = 1;
			}
			
			RegisterGameScene( gameObject.scene );
			
			if( String.IsNullOrWhiteSpace( _name ) )
				_name = gameObject.name;
			
			_subContexts.Add( this );
			
			InitializeServices( );
			InitializeAsyncServices( ).Forget( Debug.LogException );
			
			// _world = this.GetGameWorld( );
			// _world.AddContext( this );
			
			void			InitializeServices				( )		
			{
				// Add and init All Services in order
				AddServicesFromObject( gameObject, false );
				AddServicesFromObject( _services );
			
				var ctx = this;
				
				foreach ( var service in ctx.RegisteredServices.ToArray().OfType<IService>( ) )
				{
					try						{ service.OrderedInit( this ); }
					catch ( Exception ex )	{ Debug.LogException( ex ); }
				}
			}
			async	UniTask			InitializeAsyncServices			( )		
			{
				// Add and init All Services in order
				var ctx = this;
				{
					foreach ( var service in ctx.RegisteredServices.ToArray().OfType<IServiceAsync>( ) )
					{
						try						{ await service.OrderedInitAsync( this ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
				}
			}
		}                             
		private void	OnDestroy	( )		
		{
			_subContexts.Clear( );
			if( _sceneToCtxRegistry[gameObject.scene.handle] == this )
				_sceneToCtxRegistry.Remove( gameObject.scene.handle );
			//_world.RemoveContext( this );
		}
		
		public 			void			AddServicesFromObject	( GameObject go, Boolean allHierarchy = true )		
		{
			if( TopGameContext != this )
			{
				TopGameContext.AddServicesFromObject( go, allHierarchy );
				return;
			}
			
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
			for ( var i = _subContexts.Count - 1; i >= 0; i-- )
			{
				var gameContext	= _subContexts[i];
				if( gameContext._registeredServicesDict.TryGetValue( typeof(T), out var svc ) )
					return svc as T;
			}
			
			return default;
		}
		public			void			SetService<TBindFrom>	( TBindFrom service )	where TBindFrom : class		
		{
			if( service == null )
				return;
			
			if( TopGameContext != this )
			{
				TopGameContext.SetService<TBindFrom>( service );
				return;
			}
			
			var typeActual	= service.GetType();
			var typeBase	= typeof(TBindFrom);
			
			//Do not work with custom base types like MonoBehEx
			//Consider use ServiceInterfaceAttribute 
			//if( typeBase == typeActual || typeBase == typeof(MonoBehaviour) || typeBase == typeof(Component) || typeBase == typeof(UnityEngine.Object) || typeBase == typeof(ScriptableObject) || typeBase == typeof(object) )
			//typeBase = typeActual.BaseType;
				
			if( typeBase == typeof(MonoBehaviour) || typeBase == typeof(Component) || typeBase == typeof(UnityEngine.Object) || typeBase == typeof(ScriptableObject) || typeBase == typeof(object) )
				typeBase = typeActual;
			
			Debug.Log	( $"[GameWorldBase] - SetService: {typeActual.Name} → {typeActual.Name}" );
			
			_registeredServices		.Add( service );
			_registeredServicesDict	.Add( typeActual, service );
			
			if( typeBase != typeActual )     
			{
				Debug.Log	( $"[GameWorldBase] - SetService: {typeBase.Name} → {typeActual.Name}" );
				
				try						{ _registeredServicesDict.Add( typeBase, service ); }
				catch ( Exception ex )	{ Debug.LogException( ex ); }
			}
				
			if( typeActual.GetCustomAttribute<ServiceInterfaceAttribute>( ) is {} si )
			{
				if( si.InterfaceType.IsAssignableFrom( typeActual ) )
				{
					Debug.Log	( $"[GameWorldBase] - SetService: {si.InterfaceType} → {typeActual.Name}" );
					_registeredServicesDict.Add( si.InterfaceType, service );
				}
			}	
		}
		
		internal		void			PushContext				( GameContext gameContext )					
		{
			_subContexts.Add( gameContext );
		}
		internal		void			PollContext				( GameContext gameContext )					
		{
			_subContexts.Remove( gameContext );
		}
        
		#if UNITY_EDITOR
		[RuntimeInspectorUI( Repaint = true )]
		public void RuntimeGUI	( )		
		{
			if( !Application.isPlaying )
				return;
			
			GUILayout.Space( 10 );
			GUILayout.Label( "Registered Services:" );
			GUILayout.Space( 5 );
			
			DrawServices( );
		}
		public void DrawServices( )		
		{
			for (var i = _subContexts.Count - 1; i >= 0; i--)
			{
				GUILayout.Space( 5 );
				DrawCtx( _subContexts[i] );
			}
			
			static void DrawCtx( GameContext ctx )
			{
				GUILayout.Label(ctx.Name);
				GUILayout.BeginHorizontal();
				{
					GUILayout.Space(20);
					GUILayout.BeginVertical();
					{
						foreach ( var service in ctx._registeredServices )
						{
							var instance	= "";
							var className	= service.GetType().FullName;
				
							try						{ instance  = service.ToString(); }
							catch (Exception ex)	{ Debug.LogException(ex); }
				
							GUILayout.Label( instance.Contains(className, StringComparison.Ordinal) ? $"{instance}" : $"{instance}({className})" );
						}
					}
					GUILayout.EndVertical();
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
		public static T GetService<T>( this Scene context )			where T:class	
		{
			var ctx = GameContext.GetCtx( context ); 
			return ctx != null ? ctx.GetService<T>( ) : null;
		}
	}
}