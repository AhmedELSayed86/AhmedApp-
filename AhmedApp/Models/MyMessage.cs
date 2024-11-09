using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace AhmedApp.Models;

public class MyMessage : ObservableObject
{
    private string content;
    private string title;
    //private int priority;
    //private bool isFlashing;
    private Color myColor;
    //private int duration;

    public string Content
    {
        get => content;
        set => SetProperty(ref content , value);
    }

    public string Title
    {
        get => title;
        set => SetProperty(ref title , value);
    }

    //public int Priority
    //{
    //    get => priority;
    //    set => SetProperty(ref priority , value);
    //}

    //public bool IsFlashing
    //{
    //    get => isFlashing;
    //    set => SetProperty(ref isFlashing , value);
    //}

    public Color MyColor
    {
        get => myColor;
        set => SetProperty(ref myColor , value);
    }
 
    //public int Duration
    //{
    //    get => duration;
    //    set => SetProperty(ref duration , value);
    //}
}