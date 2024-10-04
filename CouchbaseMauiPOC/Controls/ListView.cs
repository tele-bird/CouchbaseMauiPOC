using System.Windows.Input;

namespace CouchbaseMauiPOC.Controls;

public class ListView : Microsoft.Maui.Controls.ListView
{
    public static readonly BindableProperty ItemTappedCommandProperty =
        BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(ListView));

    public ICommand ItemTappedCommand
    {
        get => (ICommand)GetValue(ItemTappedCommandProperty);
        set => SetValue(ItemTappedCommandProperty, value);
    }

    public ListView()
    {
        ItemTapped += OnItemTapped;
    }

    public ListView(ListViewCachingStrategy strategy)
        : base(DeviceInfo.Platform == DevicePlatform.iOS ? ListViewCachingStrategy.RetainElement : strategy)
    {
        ItemTapped += OnItemTapped;
    }

    private void OnItemTapped(object? sender, ItemTappedEventArgs e)
    {
        if(e.Item != null && ItemTappedCommand != null && ItemTappedCommand.CanExecute(e.Item))
        {
            ItemTappedCommand.Execute(e.Item);
            SelectedItem = null;
        }
    }
}
