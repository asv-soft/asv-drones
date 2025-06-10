using System.Collections.Generic;
using Asv.Avalonia;



namespace Asv.Drones;

public class TryCloseWithApprovalDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "params.close-with-approval-dialog.text";

    public TryCloseWithApprovalDialogViewModel()
        : base(DialogId)
    {
        Message = RS.ParamPageViewModel_DataLossDialog_Content;
    }

    public string Message { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
