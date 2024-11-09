using AhmedApp.Models;
using AhmedApp.Views;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AhmedApp.Services;

public static class MessageService
{
    public static ObservableCollection<MyMessage> _messageList = [];
    private static bool _isShowingMessage = false;
    private static BasePage basePage;

    public static void GetbasePage()
    {
        basePage = Application.Current.MainPage as BasePage;
    }

    public static async Task ShowMessage(string title , string content , Color color , bool isFlashing = false)
    {
        GetbasePage();
        var parameters = new MyMessage
        {
            Title = title ,
            Content = content ,
            MyColor = color
        };

        _messageList.Add(parameters);

        Device.BeginInvokeOnMainThread(() =>
        {
            basePage.MessagesLabel.Text = content;
            basePage.MessagesLabel.TextColor = color;
        });

        // بدء معالجة الرسائل فقط إذا لم تكن هناك معالجة حالية
        if(!_isShowingMessage)
        {
            await DisplayMessage(color);
        }
    }

    //private static async Task ProcessMessages()
    //{
    //    _isShowingMessage = true;

    //    while(_messageList.Count > 0)
    //    {
    //        // استخراج الرسائل وترتيبها بناءً على الأولوية فقط إذا كان هناك أكثر من رسالة
    //        var messages = _messageList.ToList();
    //        var messageToShow = messages.OrderBy(m => m.Priority).FirstOrDefault();

    //        // إزالة الرسالة من القائمة
    //        _messageList = new ConcurrentQueue<SpaerPart>(messages.Skip(1));

    //        if(messageToShow != null)
    //        {
    //            await DisplayMessage(messageToShow.Title , messageToShow.Content , messageToShow.MyColor , messageToShow.IsFlashing , messageToShow.Duration);

    //            // عدم انتظار مدة الرسالة بالكامل، بدلاً من ذلك التحقق من المدة المتبقية أثناء العرض
    //            int elapsed = 0;
    //            while(elapsed < messageToShow.Duration)
    //            {
    //                await Task.Delay(200); // التحقق كل 200 ملي ثانية
    //                elapsed += 200;
    //            }
    //        }
    //    }
    //    _isShowingMessage = false;
    //}

    private static async Task DisplayMessage(Color color , bool isFlashing = false)
    {
        await Device.InvokeOnMainThreadAsync(async () =>
        {
            if(isFlashing)
            {
                int elapsed = 0;
                bool isOriginalColor = true;
                Brush originalBackground = basePage.borderLblMessage.Stroke;

                while(elapsed < 5000)
                {
                    // تغيير الخلفية بالتناوب بين الأصلية و لون الفلاش
                    basePage.borderLblMessage.Stroke = isOriginalColor ? color : originalBackground;
                    isOriginalColor = !isOriginalColor;

                    // الانتظار لفترة قصيرة لضبط الفلاش
                    await Task.Delay(500);
                    elapsed += 500;
                }

                // إعادة اللون الأصلي بعد انتهاء الفلاش
                basePage.borderLblMessage.Stroke = originalBackground;
            }
        });
        await Task.Delay(5000);
    }
}