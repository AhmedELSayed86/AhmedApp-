using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedApp.ViewModels;

public class SpaerPartViewModel : INotifyPropertyChanged
{
    private GridLength _columnWidth;
    public GridLength ColumnWidth
    {
        get { return _columnWidth; }
        set
        {
            _columnWidth = value;
            OnPropertyChanged(nameof(ColumnWidth));
        }
    }

    public SpaerPartViewModel()
    {
        // تحديد قيمة العرض الابتدائي للعمود
        ColumnWidth = new GridLength(1 , GridUnitType.Star); // أو أي قيمة أخرى مناسبة
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propertyName));
    }
}