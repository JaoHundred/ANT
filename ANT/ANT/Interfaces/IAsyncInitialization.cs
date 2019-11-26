using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ANT.Interfaces
{
    /// <summary>
    /// Interface para implementação assíncrona de cargas iniciais dentro de construtores
    /// </summary>
    public interface IAsyncInitialization
    {
        Task InitializeTask { get; }
        Task LoadAync();
    }
}
