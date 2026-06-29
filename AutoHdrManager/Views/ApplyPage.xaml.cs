using System.Windows.Controls;
using AutoHdrManager.ViewModels;

namespace AutoHdrManager.Views;

public partial class ApplyPage : Page
{
    public ApplyPage()
    {
        DataContext = new ApplyViewModel();
        InitializeComponent();
    }
}
