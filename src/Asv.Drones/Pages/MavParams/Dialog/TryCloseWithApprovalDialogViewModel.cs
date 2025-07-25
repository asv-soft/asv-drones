using System.Collections.Generic;
using Asv.Avalonia;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Drones;

public class TryCloseWithApprovalDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "params.close-with-approval-dialog.text";

    public TryCloseWithApprovalDialogViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TryCloseWithApprovalDialogViewModel(ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        Message = RS.ParamPageViewModel_DataLossDialog_Content;
    }

    public string Message { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
