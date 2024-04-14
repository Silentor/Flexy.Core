using System.Linq;

namespace Flexy.Core.Actions
{
	[Serializable]
	public class Sequence : FlexyActionAsync
	{
		[SerializeField]		EType			Type;
		[SerializeReference]	FlexyAction[]	Actions;
		
		public override async UniTask DoAsync	( FCtx ctx )
		{
			switch (Type)
			{
				case EType.Sequential:
					foreach ( var act in Actions )
					{
						try						{ await act.DoAsync( ctx ); }
						catch ( Exception ex )	{ Debug.LogException( ex ); }
					}
					break;

				case EType.Parallel:
				{
					try						{ await UniTask.WhenAll( Enumerable.Select(Actions, a => a.DoAsync( ctx ) ).ToArray( ) ); }
					catch ( Exception ex )	{ Debug.LogException( ex ); }
				}
					break;

				default: throw new ArgumentOutOfRangeException();
			}
		}

		private enum EType 
		{
			Sequential,
			Parallel,
		}
	}
}