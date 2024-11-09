using AhmedApp.Models;
using AhmedApp.Services;
using System.Collections.ObjectModel;

namespace AhmedApp.Views;

public partial class PopupMessagesPage : ContentPage
{
    public ObservableCollection<MyMessage> Messages;
    public PopupMessagesPage()
    {
        InitializeComponent();

       Messages = MessageService._messageList;

        CollectionMessages.ItemsSource = Messages;
        BindingContext = this;
    }

    private void Button_Clicked(object sender , EventArgs e)
    { 
        Application.Current.MainPage.Navigation.PopModalAsync( );
    }
}
