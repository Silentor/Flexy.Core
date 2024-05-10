using System.Globalization;
using Flexy.Utils;

namespace Flexy.Core
{
	[DefaultExecutionOrder(Int16.MinValue+1)]
	public sealed class GlobalContext : GameContext
	{
		private new		void			Awake				( )		
		{
			EarlyUpdateLoop( ).Forget( );
			gameObject.AddComponent<GlobalContextLastUpdates>( );
			CreateLocalEventBus( );
			
			base.Awake( );
		}
			
		private async	UniTaskVoid		EarlyUpdateLoop		( )		
		{
			while( this )
			{
				try
				{
					await UniTask.DelayFrame(1, PlayerLoopTiming.LastEarlyUpdate);
					
					if( !this )
						return;
						
					while( !gameObject.activeInHierarchy )
						await UniTask.DelayFrame(1, PlayerLoopTiming.LastEarlyUpdate);
					
					if( !this )
						return;
						
					EarlyUpdate( );
				}
				catch (Exception ex) { Debug.LogException( ex ); }
			}
		}
		private			void			FixedUpdate			( )		=> base.FixedUpdateFirst( );
		internal		void			FixedUpdateLast		( )		=> base.FixedUpdateLast( );
		private			void			Update				( )		=> base.UpdateFirst( );
		internal		void			UpdateLast			( )		=> base.UpdateLast( );
		private			void			LateUpdate			( )		=> base.LateUpdateFirst( );
		internal		void			LateUpdateLast		( )		=> base.LateUpdateLast( );
		
		#if UNITY_EDITOR
		[RuntimeInspectorUI( Repaint = true )]
		public void AdditionalInspectorGUI( )
		{
			if( !Application.isPlaying )
				return;
			
			GUILayout.Space( 10 );
			GUILayout.Label( "System Groups:" );
			GUILayout.Space( 5 );

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(20);
				GUILayout.BeginVertical();
				{
					DrawGroup( _group_EarlyUpdate		);
					DrawGroup( _group_FixedUpdateFirst		);
					DrawGroup( _group_FixedUpdateLast	);
					DrawGroup( _group_UpdateFirst			);
					DrawGroup( _group_UpdateLast		);
					DrawGroup( _group_LateUpdateFirst		);
					DrawGroup( _group_LateUpdateLast	);
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			
			// GUILayout.Space( 10 );
			// GUILayout.Label( "Event Bus:" );
			//
			// GUILayout.BeginHorizontal();
			// {
			// 	GUILayout.Space(20);
			// 	GUILayout.BeginVertical();
			// 	{
			// 		_localEventBus.DrawBusesUI( );
			// 	}
			// 	GUILayout.EndVertical();
			// }
			// GUILayout.EndHorizontal();
			
			static void DrawGroup( FlexySystemGroup group )
			{
				if( group == null )
					return;
				
				GUILayout.BeginHorizontal( );
				GUILayout.Label( group.Name );
				GUILayout.FlexibleSpace( );
				
				GUI.color = group.LastRunUpdateFrame == Time.frameCount ? Color.white : Color.gray;
				GUILayout.Label( group.LastRunUpdateTime.ToString( "F2", CultureInfo.InvariantCulture) );
				GUI.color = Color.white;			
				
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal( );
				{
					GUILayout.Space( 20 );
					GUILayout.BeginVertical( );
					{
						foreach ( var worldSystem in group.Systems )
						{
							if( worldSystem is FlexySystemGroup g )
							{
								DrawGroup( g );
							}
							else
							{
								GUILayout.BeginHorizontal( );
								GUILayout.Label( worldSystem.Name );
								GUILayout.FlexibleSpace( );
								GUI.color = worldSystem.LastRunUpdateFrame == Time.frameCount ? Color.white : Color.gray;
								GUILayout.Label( worldSystem.LastRunUpdateTime.ToString( "F2", CultureInfo.InvariantCulture) );
								GUI.color = Color.white;
								
								GUILayout.EndHorizontal();
							}
						}
					}
					GUILayout.EndVertical( );
				}
				GUILayout.EndHorizontal( );
				GUILayout.Space( 5 );
			}
		}
		#endif
	}
}