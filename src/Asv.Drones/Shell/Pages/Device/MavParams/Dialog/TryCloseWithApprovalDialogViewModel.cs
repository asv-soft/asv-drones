using System.Collections.Generic;
using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class TryCloseWithApprovalDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}CloseWithApproval";

    public TryCloseWithApprovalDialogViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TryCloseWithApprovalDialogViewModel(ILoggerFactory loggerFactory)
        : base(DialogId)
    {
        Message = RS.ParamPageViewModel_DataLossDialog_Content;
    }

    public string Message { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
