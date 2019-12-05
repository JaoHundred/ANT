using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using MvvmHelpers;
using Xamarin.Forms;

namespace ANT.UTIL
{
    /// <summary>
    /// herda de MvvmHelpers BaseViewModel, classe usada para troca de modo de seleção em CollectionView
    /// </summary>
    public abstract class BaseVMSelectionModeExtender : BaseViewModel
    {
        private bool _isMultiSelect;
        public bool IsMultiSelect
        {
            get { return _isMultiSelect; }
            set { SetProperty(ref _isMultiSelect, value); }
        }

        private Xamarin.Forms.SelectionMode _selectionMode = SelectionMode.Single;
        public Xamarin.Forms.SelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set { SetProperty(ref _selectionMode, value); }
        }

        /// <summary>
        /// Usado para habilitar seleção única
        /// </summary>
        public void SingleSelectionMode()
        {
            SelectionMode = SelectionMode.Single;
            IsMultiSelect = false;
        }

        /// <summary>
        /// Usado para habilitar multi seleção
        /// </summary>
        public void MultiSelectionMode()
        {
            SelectionMode = SelectionMode.Multiple;
            IsMultiSelect = true;
        }
    }
}
