using UnityEngine;

/// <summary>
/// 关闭对话框的事件节点
/// </summary>
[CreateAssetMenu(fileName = "Node_", menuName = "Event/Message/Close Dialogue Box")]
public class EN_CloseDialogueBox : EventNodeBase
{
    public override void Execute()
    {
        base.Execute();
        UIManager.CloseDialogueBox();
        state = NodeState.Finished;
        OnFinished?.Invoke(true);
    }
}