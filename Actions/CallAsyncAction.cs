namespace Flexy.Core.Actions;

[Serializable]
public class CallAsyncAction: FlexyActionAsync
{
	public override UniTask DoAsync(ActionCtx ctx)
	{
		return UniTask.CompletedTask;
	}
}