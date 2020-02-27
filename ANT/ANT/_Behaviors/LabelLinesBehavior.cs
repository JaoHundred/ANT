using Xamarin.Forms;

namespace ANT._Behaviors
{
    /// <summary>
    /// formata a exibição de linhas de texto para aproveitar o espaço(texto comprido recebe Trailtruncate, enquanto texto com espaços ou vírgulas recebem
    /// o tratamento de duas linhas seguido de trailtruncate)
    /// </summary>
    public class LabelLinesBehavior : Behavior<Label>
    {

        public int MaxLines { get; set; }

        protected override void OnAttachedTo(Label bindable)
        {
            bindable.PropertyChanged += Bindable_PropertyChanged;
            base.OnAttachedTo(bindable);
        }

        private void Bindable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Text")
            {
                var bindable = (Label)sender;
                string tx = bindable.Text;

                if (tx == null)
                    return;

                bindable.MaxLines = tx.Length >= 11 && !tx.Contains(",") && !tx.Contains(" ") ? 1 : MaxLines;
            }
        }

        protected override void OnDetachingFrom(Label bindable)
        {
            bindable.PropertyChanged -= Bindable_PropertyChanged;
            base.OnDetachingFrom(bindable);
        }
    }
}
