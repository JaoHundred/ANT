using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;
using magno = MvvmHelpers.Commands;
using ANT.Core;
using System.Windows.Input;
using System.Threading.Tasks;

namespace ANT.Modules
{
    public class ChoiceModalViewModel : BaseViewModel
    {
        public ChoiceModalViewModel(string title, string message, Action action)
        {
            TitleModal = title;
            Message = message;
            _action = action;

            ConfirmCommand = new magno.AsyncCommand(OnConfirm);
            CancelCommand = new magno.AsyncCommand(OnCancel);
        }

        private string _titleModal;
        private Action _action;

        public string TitleModal
        {
            get { return _titleModal; }
            set { SetProperty(ref _titleModal, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ICommand ConfirmCommand { get; private set; }
        private async Task OnConfirm()
        {
            _action.Invoke();
            await NavigationManager.PopPopUpPageAsync();
        }

        public ICommand CancelCommand { get; private set; }
        private async Task OnCancel()
        {
            await NavigationManager.PopPopUpPageAsync();
        }

    }
}
