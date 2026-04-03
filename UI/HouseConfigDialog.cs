using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitHouseGenerator.UI
{
    public class HouseConfigDialog : Window
    {
        public int SelectedModulesX { get; private set; }
        public int SelectedModulesY { get; private set; }

        private readonly TextBox _modulesX;
        private readonly TextBox _modulesY;
        private readonly TextBlock _sizeLabel;

        public HouseConfigDialog()
        {
            Title = "GenHouse";
            Width = 340;
            Height = 160;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var root = new Grid { Margin = new Thickness(15) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // --- Module grid input ---
            _modulesX = new TextBox { Text = "8", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            _modulesY = new TextBox { Text = "6", TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            _sizeLabel = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Gray,
                FontSize = 11,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var modulesGrid = new Grid { Margin = new Thickness(10, 8, 10, 8) };
            modulesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            modulesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            modulesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            modulesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
            modulesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lx  = new TextBlock { Text = "Modules X:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            var lxy = new TextBlock { Text = "  ×  Y:",    VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 8, 0) };

            Grid.SetColumn(lx,         0); Grid.SetColumn(_modulesX,  1);
            Grid.SetColumn(lxy,        2); Grid.SetColumn(_modulesY,  3);
            Grid.SetColumn(_sizeLabel, 4);

            foreach (var el in new UIElement[] { lx, _modulesX, lxy, _modulesY, _sizeLabel })
                modulesGrid.Children.Add(el);

            var group = new GroupBox
            {
                Header  = "Building footprint  (120 × 120 cm modules)",
                Content = modulesGrid
            };
            Grid.SetRow(group, 0);

            // --- Buttons ---
            var okBtn     = new Button { Content = "OK",     Width = 80, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
            var cancelBtn = new Button { Content = "Cancel", Width = 80, IsCancel = true };
            okBtn.Click     += OkButton_Click;
            cancelBtn.Click += (s, e) => DialogResult = false;

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttons.Children.Add(okBtn);
            buttons.Children.Add(cancelBtn);
            Grid.SetRow(buttons, 2);

            root.Children.Add(group);
            root.Children.Add(buttons);
            Content = root;

            _modulesX.TextChanged += (s, e) => UpdateSizeLabel();
            _modulesY.TextChanged += (s, e) => UpdateSizeLabel();
            Loaded += (s, e) => UpdateSizeLabel();
        }

        private void UpdateSizeLabel()
        {
            if (int.TryParse(_modulesX?.Text, out int mx) && mx > 0 &&
                int.TryParse(_modulesY?.Text, out int my) && my > 0)
                _sizeLabel.Text = $"= {mx * 1.2:F1} × {my * 1.2:F1} m";
            else
                _sizeLabel.Text = "invalid";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(_modulesX.Text, out int mx) || mx < 1 ||
                !int.TryParse(_modulesY.Text, out int my) || my < 1)
            {
                MessageBox.Show("Enter valid module counts (minimum 1).", "Validation Error");
                return;
            }
            SelectedModulesX = mx;
            SelectedModulesY = my;
            DialogResult = true;
        }
    }
}
