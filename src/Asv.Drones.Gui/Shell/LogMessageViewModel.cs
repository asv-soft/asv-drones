using System;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Api;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui
{
    public class LogMessageViewModel : DisposableReactiveObject
    {
        private readonly ISourceList<LogMessage> _sourceList;
        private readonly LogMessage _message;

        public LogMessageViewModel(LogMessage message)
        {
            IsOpen = true;
            _message = message;
        }

        public LogMessageViewModel(ISourceList<LogMessage> sourceList, LogMessage message) : this(message)
        {
            _sourceList = sourceList;
            // we need to remove 
            Observable
                .Timer(TimeSpan.FromSeconds(10))
                .Subscribe(_ => Close())
                .DisposeItWith(Disposable);

            this.WhenAnyValue(_ => _.IsOpen)
                .Where(_ => _ == false)
                .Subscribe(_ => sourceList.Remove(_message))
                .DisposeItWith(Disposable);
        }

        public void Close()
        {
            _sourceList?.Remove(_message);
        }

        public string Title => _message.Source;
        [Reactive] public bool IsOpen { get; set; }

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
        public string Description => _message.Description;
    }
}