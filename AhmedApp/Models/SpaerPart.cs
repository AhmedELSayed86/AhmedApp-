using SQLite; 
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AhmedApp.Models;

[Table("SpaerPart")]
public class SpaerPart : ObservableObject
{ 
    [Key]  //[PrimaryKey, AutoIncrement]
    private int sapCode;
    private string partNo;
    private string matrialGroup;
    private string model;
    private string descriptionAR;
    private string descriptionEN;
    private string c1;
    private string c2;
    private bool isDamaged;
    public string aNots;

    public int SapCode
    {
        get => sapCode;
        set => SetProperty(ref sapCode , value);
    }

    public string PartNo
    {
        get => partNo;
        set => SetProperty(ref partNo , value);
    }

    public string MatrialGroup
    {
        get => matrialGroup;
        set => SetProperty(ref matrialGroup , value);
    }
    public string Model
    {
        get => model;
        set => SetProperty(ref model , value);
    }
    public string DescriptionAR
    {
        get => descriptionAR;
        set => SetProperty(ref descriptionAR , value);
    }
    public string DescriptionEN
    {
        get => descriptionEN;
        set => SetProperty(ref descriptionEN , value);
    }

    public string C1
    {
        get => c1;
        set => SetProperty(ref c1 , value);
    }

    public string C2
    {
        get => c2;
        set => SetProperty(ref c2 , value);
    }

    public bool IsDamaged
    {
        get => isDamaged;
        set => SetProperty(ref isDamaged , value);
    }

    public string ANots
    {
        get => aNots;
        set => SetProperty(ref aNots , value);
    }
}