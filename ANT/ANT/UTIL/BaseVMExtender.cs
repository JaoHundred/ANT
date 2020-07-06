using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using MvvmHelpers;
using Xamarin.Forms;

namespace ANT.UTIL
{
    /// <summary>
    /// herda de MvvmHelpers BaseViewModel, classe usada para troca de modo de seleção em CollectionView, e propriedade para pesquisa na navigationBar
    /// </summary>
    public abstract class BaseVMExtender : BaseViewModel
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

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get { return _searchQuery; }
            set { SetProperty(ref _searchQuery, value); }
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
