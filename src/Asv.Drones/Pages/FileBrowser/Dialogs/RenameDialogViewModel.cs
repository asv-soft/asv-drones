using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Asv.Avalonia;
using Asv.Common;
using R3;

namespace Asv.Drones;

public partial class RenameDialogViewModel : DialogViewModelBase
{
    private const string DialogId = $"{BaseId}.rename";
    private const int MaxNameLenght = 255;
    private static readonly Regex ValidNameRegex = MyRegex();

    public RenameDialogViewModel()
        : base(DialogId, DesignTime.LoggerFactory)
    {
        NewName = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        NewName
            .EnableValidationRoutable(NameValidator, this, isForceValidation: true)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> NewName { get; }

    private static ValidationResult NameValidator(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            return ValidationResult.FailAsNullOrWhiteSpace;
        }

        if (arg.Any(Path.GetInvalidFileNameChars().Contains) || !ValidNameRegex.IsMatch(arg))
        {
            return new ValidationResult
            {
                IsSuccess = false,
                ValidationException = new ValidationException("Name contains invalid characters"),
            };
        }

        if (arg.Length > MaxNameLenght)
        {
            return ValidationResult.FailAsOutOfRange("1", MaxNameLenght.ToString());
        }

        return ValidationResult.Success;
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        dialog.DefaultButton = ContentDialogButton.Primary;
        IsValid.Subscribe(b => dialog.IsPrimaryButtonEnabled = b).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    [GeneratedRegex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
