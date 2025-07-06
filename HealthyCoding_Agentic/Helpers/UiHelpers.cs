using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace HealthyCoding_Agentic.Helpers;

public static class DataGridSelectionBehavior {
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(DataGridSelectionBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsChanged));

    public static IList GetSelectedItems(DependencyObject obj) => (IList)obj.GetValue(SelectedItemsProperty);
    public static void SetSelectedItems(DependencyObject obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is DataGrid dataGrid) {
            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;

            if (e.NewValue is IList selectedItems) {
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
                SyncSelectedItems(dataGrid, selectedItems);
            }
        }
    }

    private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (sender is DataGrid dataGrid &&
            GetSelectedItems(dataGrid) is IList selectedItems) {
            foreach (var item in e.RemovedItems)
                selectedItems.Remove(item);

            foreach (var item in e.AddedItems)
                if (!selectedItems.Contains(item))
                    selectedItems.Add(item);
        }
    }

    private static void SyncSelectedItems(DataGrid dataGrid, IList selectedItems) {
        dataGrid.SelectedItems.Clear();

        foreach (var item in selectedItems)
            dataGrid.SelectedItems.Add(item);
    }
}
public class ReverseBooleanToVisibilityConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        (bool)value ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}