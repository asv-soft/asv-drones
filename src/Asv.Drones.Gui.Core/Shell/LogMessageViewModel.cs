using System.Reactive.Linq;
using Asv.Common;
using Avalonia.Media;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class LogMessageViewModel:DisposableViewModelBase
    {
        private readonly LogMessage _message;

        public LogMessageViewModel(LogMessage message)
        {
            IsOpen = true;
            _message = message;
            
        }

        public LogMessageViewModel(ISourceList<LogMessage> source, LogMessage message):this(message)
        {
            Observable
                .Timer(TimeSpan.FromSeconds(10))
                .Subscribe(_ => source.Remove(_message))
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.IsOpen)
                .Where(_ => _ == false)
                .Subscribe(_ => source.Remove(_message))
                .DisposeItWith(Disposable);
        }

        public string Title => _message.Source;
        [Reactive]
        public bool IsOpen { get; set; }
        public InfoBarSeverity Severity
        {
            get
            {
                return _message.Type switch
                {
                    LogMessageType.Info => InfoBarSeverity.Success,
                    LogMessageType.Error => InfoBarSeverity.Error,
                    LogMessageType.Warning => InfoBarSeverity.Warning,
                    LogMessageType.Trace => InfoBarSeverity.Informational,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public string Message => _message.Message;
    }
}