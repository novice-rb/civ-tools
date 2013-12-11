using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapEngine;

namespace MapEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int TileWidth = 5;
        public const int TileHeight = 5;

        private Game _Game;
        private string _Title;
        private StartingLocationPlacer _Placer = null;

        public MainWindow()
        {
            InitializeComponent();
            System.Reflection.Assembly asm = this.GetType().Assembly;
            System.Reflection.AssemblyName name = asm.GetName();
            this.Title = this.Title + " build " + name.Version.ToString();
            _Title = this.Title;
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            StopSearch();
        }

        private void ShowMap(bool visualizeClosestPlayers)
        {
            try
            {
                this.Title = _Title + string.Format(" - ({0}x{1} tiles)", _Game.Map.Width, _Game.Map.Height);
                uxMapGrid.Children.Clear();
                uxMapGrid.RowDefinitions.Clear();
                uxMapGrid.ColumnDefinitions.Clear();
                for (int x = 0; x < _Game.Map.Width; x++)
                {
                    uxMapGrid.ColumnDefinitions.Add(
                        new ColumnDefinition { Width = new GridLength(TileWidth) }
                    );
                }
                for (int y = 0; y < _Game.Map.Height; y++)
                {
                    uxMapGrid.RowDefinitions.Add(
                         new RowDefinition { Height = new GridLength(TileHeight) }
                     );
                }
                MapContainer.MaxHeight = _Game.Map.Height * TileHeight;
                txtReport.MaxHeight = uxMapScrollViewer.Height;
                for (int x = 0; x < _Game.Map.Width; x++)
                {
                    for (int y = 0; y < _Game.Map.Height; y++)
                    {
                        Tile t = _Game.Map.GetTile(x, y);
                        Rectangle terrain = CreateTileRectangle(x, y, t, 1);
                        terrain.Fill = GetTerrainBrush(t.Terrain, t.PlotType, t.FeatureType);
                        uxMapGrid.Children.Add(terrain);

                        if (t.BonusType != BonusTypes.BONUS_NONE)
                        {
                            Rectangle bonus = CreateTileRectangle(x, y, t, 1);
                            bonus.Margin = new Thickness(TileWidth / 5);
                            bonus.Fill = GetBonusBrush(t.BonusType);
                            uxMapGrid.Children.Add(bonus);
                        }

                        if (t.IsNOfRiver)
                        {
                            Rectangle river = CreateTileRectangle(x, y, t, 1);
                            river.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                            river.Height /= 7;
                            river.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                            uxMapGrid.Children.Add(river);
                        }
                        if (t.IsWOfRiver)
                        {
                            Rectangle river = CreateTileRectangle(x, y, t, 1);
                            river.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255));
                            river.Width /= 7;
                            river.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                            uxMapGrid.Children.Add(river);
                        }

                        Rectangle overlay = CreateTileRectangle(x, y, t, 0.8);
                        if (t.Units.Count > 0)
                            overlay.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        else if (t.Selected)
                            overlay.Fill = new SolidColorBrush(GetPrimaryColor(PlayerColors.PLAYERCOLOR_CYAN));
                        else if (visualizeClosestPlayers)
                        {
                            if (t.ClosestPlayer == -1)
                                overlay.Fill = new SolidColorBrush(GetPrimaryColor(PlayerColors.PLAYERCOLOR_BLACK));
                            else
                                overlay.Fill = new SolidColorBrush(GetPrimaryColor(_Game.Players[t.ClosestPlayer].Color));
                        }
                        else
                            continue;
                        uxMapGrid.Children.Add(overlay);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private Rectangle CreateTileRectangle(int x, int y, Tile t, double opacity)
        {
            Rectangle element = new Rectangle();
            element.Width = TileWidth;
            element.Height = TileHeight;
            element.SetValue(Grid.RowProperty, _Game.Map.Height - 1 - y);
            element.SetValue(Grid.ColumnProperty, x);
            element.Opacity = opacity;
            return element;
        }

        private static Point GetBonusSpriteLocation(BonusTypes bonusType)
        {
            switch(bonusType)
            {
                case BonusTypes.BONUS_ALUMINUM:
                    return new Point(0, 0);
                case BonusTypes.BONUS_COAL:
                    return new Point(1, 0);
                case BonusTypes.BONUS_COPPER:
                    return new Point(2, 0);
                case BonusTypes.BONUS_HORSE:
                    return new Point(3, 0);
                case BonusTypes.BONUS_IRON:
                    return new Point(4, 0);
                case BonusTypes.BONUS_MARBLE:
                    return new Point(5, 0);
                case BonusTypes.BONUS_OIL:
                    return new Point(6, 0);
                case BonusTypes.BONUS_STONE:
                    return new Point(7, 0);
                case BonusTypes.BONUS_URANIUM:
                    return new Point(8, 0);
                case BonusTypes.BONUS_BANANA:
                    return new Point(0, 1);
                case BonusTypes.BONUS_CLAM:
                    return new Point(1, 1);
                case BonusTypes.BONUS_CORN:
                    return new Point(2, 1);
                case BonusTypes.BONUS_COW:
                    return new Point(3, 1);
                case BonusTypes.BONUS_CRAB:
                    return new Point(4, 1);
                case BonusTypes.BONUS_DEER:
                    return new Point(5, 1);
                case BonusTypes.BONUS_FISH:
                    return new Point(6, 1);
                case BonusTypes.BONUS_PIG:
                    return new Point(7, 1);
                case BonusTypes.BONUS_RICE:
                    return new Point(8, 1);
                case BonusTypes.BONUS_SHEEP:
                    return new Point(0, 2);
                case BonusTypes.BONUS_WHEAT:
                    return new Point(1, 2);
                case BonusTypes.BONUS_DYE:
                    return new Point(2, 2);
                case BonusTypes.BONUS_FUR:
                    return new Point(3, 2);
                case BonusTypes.BONUS_GEMS:
                    return new Point(4, 2);
                case BonusTypes.BONUS_GOLD:
                    return new Point(5, 2);
                case BonusTypes.BONUS_INCENSE:
                    return new Point(6, 2);
                case BonusTypes.BONUS_IVORY:
                    return new Point(7, 2);
                case BonusTypes.BONUS_SILK:
                    return new Point(8, 2);
                case BonusTypes.BONUS_SILVER:
                    return new Point(0, 3);
                case BonusTypes.BONUS_SPICES:
                    return new Point(1, 3);
                case BonusTypes.BONUS_SUGAR:
                    return new Point(2, 3);
                case BonusTypes.BONUS_WINE:
                    return new Point(3, 3);
                case BonusTypes.BONUS_WHALE:
                    return new Point(4, 3);
                case BonusTypes.BONUS_DRAMA:
                    return new Point(5, 3);
                case BonusTypes.BONUS_MUSIC:
                    return new Point(6, 3);
                case BonusTypes.BONUS_MOVIES:
                    return new Point(7, 3);
                default:
                    return new Point(0, 0);
            }
        }

        private static Brush GetBonusBrush(BonusTypes bonusType)
        {
            Image img = App.Current.Resources["RESOURCES"] as Image;
            ImageBrush brush = new ImageBrush(img.Source);
            brush.Stretch = Stretch.Fill;
            Point loc = GetBonusSpriteLocation(bonusType);
            brush.Viewbox = new Rect(loc.X * 34, loc.Y * 34, 30, 30);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(0, 0, TileWidth * 3 / 5, TileHeight * 3 / 5);
            brush.ViewportUnits = BrushMappingMode.Absolute;
            return brush;
        }

        private static Brush GetTerrainBrush(TerrainTypes terrainType, PlotTypes plotType, FeatureTypes featureType)
        {
            string key = GetTerrainImageKey(terrainType, plotType, featureType);
            Image img = App.Current.Resources[key] as Image;
            if (img != null) return new ImageBrush(img.Source);
            return new SolidColorBrush(Color.FromRgb(0, 0, 0));
        }

        private static string GetTerrainImageKey(TerrainTypes terrainType, PlotTypes plotType, FeatureTypes featureType)
        {
            if (plotType == PlotTypes.PEAK)
                return "PEAK";
            else if (featureType == FeatureTypes.FEATURE_ICE)
                return "ICE";
            else if (featureType == FeatureTypes.FEATURE_OASIS)
                return "OASIS";
            string key = "";
            if (featureType == FeatureTypes.FEATURE_JUNGLE)
                key = "JUNGLE";
            else
                key = terrainType.ToString().Replace("TERRAIN_", "");
            if ((terrainType == TerrainTypes.TERRAIN_GRASS || terrainType == TerrainTypes.TERRAIN_PLAINS || terrainType == TerrainTypes.TERRAIN_TUNDRA) && featureType == FeatureTypes.FEATURE_FOREST)
                key += "_FOREST";
            if ((terrainType == TerrainTypes.TERRAIN_GRASS || terrainType == TerrainTypes.TERRAIN_PLAINS || terrainType == TerrainTypes.TERRAIN_TUNDRA || terrainType == TerrainTypes.TERRAIN_SNOW || terrainType == TerrainTypes.TERRAIN_DESERT) && plotType == PlotTypes.HILL)
                key += "_HILL";
            return key;
        }

        private void btnOpenMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StopSearch();
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".CivBeyondSwordWBSave";
                dlg.Filter = "Worldbuilder Saves (.CivBeyondSwordWBSave)|*.CivBeyondSwordWBSave|Civ3 maps (*.bic)|*.bic";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    _Game = WorldBuilderParser.FromWorldbuilderFile(dlg.FileName); //@"C:\Users\ottarv\Dropbox\Public\PBEM17\sandbox\pbem17_50x50_3.CivBeyondSwordWBSave");
                    ResizeAndShowMap();
                    ZoomSlider.Value = ZoomSlider.Minimum;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void StopSearch()
        {
            if (_Placer != null)
            {
                _Placer.Stop();
                _Placer = null;
            }
        }

        private void ResizeAndShowMap()
        {
            uxMapScrollViewer.Width = uxMapScrollViewer.Height * _Game.Map.Width / _Game.Map.Height;
            ShowMap(false);
            MapContainer.Width = _Game.Map.Width * TileWidth;
            MapContainer.Height = _Game.Map.Height * TileHeight;
            this.UpdateLayout();
            ZoomSlider.Minimum = Math.Log(uxMapScrollViewer.ViewportWidth / MapContainer.ActualWidth, 1.3);
            ZoomSlider.Maximum = Math.Max(10, ZoomSlider.Minimum + 5);
        }

        private void btnSaveMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".CivBeyondSwordWBSave";
                dlg.Filter = "Worldbuilder Saves (.CivBeyondSwordWBSave)|*.CivBeyondSwordWBSave";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    WorldBuilderParser.ToWorldbuilderFile(_Game, dlg.FileName); //@"C:\Users\ottarv\Dropbox\Public\PBEM17\sandbox\pbem17_50x50_3.CivBeyondSwordWBSave");
                    MessageBox.Show("Map saved successfully.", "Save complete");
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private double ParseDouble(string text, double defaultValue)
        {
            try
            {
                defaultValue = double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }
            return defaultValue;
        }

        private void btnBalanceCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Game == null || _Game.Map == null) return;
                BalanceCheckerSettings settings = GetBalanceSettings();
                BalanceReport report = BalanceChecker.CheckBalance(_Game.Map, settings, null);
                ShowMap(true);
                ShowBalanceReport(report);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowBalanceReport(BalanceReport report)
        {
            FlowDocument doc = new FlowDocument();
            txtReport.Document = doc;
            doc.Blocks.Add(new Paragraph(new Run("Total map unfairness (standard deviation in weighted land quality): " + (int)Math.Round(report.Unfairness))) { Foreground = new SolidColorBrush(Colors.Black) });
            foreach (var item in report.PlayerData)
            {
                string text = item.ToString(_Game);
                doc.Blocks.Add(new Paragraph(new Run(text)) { Foreground = new SolidColorBrush(GetPrimaryColor(_Game.Players[item.Player].Color)), Background=new SolidColorBrush(Colors.Black) });
            }
        }

        private BalanceCheckerSettings GetBalanceSettings()
        {
            BalanceCheckerSettings settings = new BalanceCheckerSettings();
            settings.IncludeWater = chkIncludeWater.IsChecked.Value;
            settings.IncludeIslands = chkIncludeIslands.IsChecked.Value;
            settings.TraverseMainlandFirst = chkMainlandFirst.IsChecked.Value;
            settings.UseTraversalCosts = chkUseTraversalCosts.IsChecked.Value;
            settings.BaseCost = ParseDouble(txtBaseCost.Text, settings.BaseCost);
            settings.WaterCost = ParseDouble(txtWaterCost.Text, settings.WaterCost);
            settings.FoodWeight = ParseDouble(txtFoodWeight.Text, settings.FoodWeight);
            settings.HammerWeight = ParseDouble(txtHammerWeight.Text, settings.HammerWeight);
            settings.JunglePenalizer = ParseDouble(txtJunglePenalizer.Text, settings.JunglePenalizer);
            return settings;
        }

        private static Color CreateColor(double alpha, double red, double green, double blue)
        {
            return Color.FromScRgb((float)alpha, (float)red, (float)green, (float)blue);
        }

        private static Color GetPrimaryColor(PlayerColors color)
        {
            switch (color)
            {
                case PlayerColors.PLAYERCOLOR_BLACK: return CreateColor(1, 0.13, 0.13, 0.13);
                case PlayerColors.PLAYERCOLOR_BLUE: return CreateColor(1, 0.21, 0.40, 1.00);
                case PlayerColors.PLAYERCOLOR_BROWN: return CreateColor(1, 0.39, 0.24, 0.00);
                case PlayerColors.PLAYERCOLOR_CYAN: return CreateColor(1, 0.07, 0.80, 0.96);
                case PlayerColors.PLAYERCOLOR_LIGHT_BLUE: return CreateColor(1, 0.5, 0.70, 1);
                case PlayerColors.PLAYERCOLOR_DARK_BLUE: return CreateColor(1, 0.16, 0.00, 0.64);
                case PlayerColors.PLAYERCOLOR_DARK_CYAN: return CreateColor(1, 0.00, 0.54, 0.55);
                case PlayerColors.PLAYERCOLOR_LIGHT_GREEN: return CreateColor(1, 0.5, 1, 0.50);
                case PlayerColors.PLAYERCOLOR_MIDDLE_GREEN: return CreateColor(1, 0.204, 0.576, 0.00);
                case PlayerColors.PLAYERCOLOR_DARK_GREEN: return CreateColor(1, 0.00, 0.39, 0.00);
                case PlayerColors.PLAYERCOLOR_DARK_DARK_GREEN: return CreateColor(1, 0.00, 0.27, 0.00);
                case PlayerColors.PLAYERCOLOR_DARK_PINK: return CreateColor(1, 0.69, 0.00, 0.38);
                case PlayerColors.PLAYERCOLOR_DARK_PURPLE: return CreateColor(1, 0.45, 0.00, 0.49);
                case PlayerColors.PLAYERCOLOR_DARK_RED: return CreateColor(1, 0.62, 0.00, 0.00);
                case PlayerColors.PLAYERCOLOR_DARK_YELLOW: return CreateColor(1, 0.97, 0.75, 0.0);
                case PlayerColors.PLAYERCOLOR_DARK_GRAY: return CreateColor(1, 0.369, 0.369, 0.369);
                case PlayerColors.PLAYERCOLOR_GRAY: return CreateColor(1, 0.7, 0.7, 0.7);
                case PlayerColors.PLAYERCOLOR_GREEN: return CreateColor(1, 0.49, 0.88, 0.00);
                case PlayerColors.PLAYERCOLOR_ORANGE: return CreateColor(1, 0.99, 0.35, 0.0);
                case PlayerColors.PLAYERCOLOR_PEACH: return CreateColor(1, 0.60 , 0.49, 0.0);
                case PlayerColors.PLAYERCOLOR_PINK: return CreateColor(1, 0.98 , 0.67, 0.49);
                case PlayerColors.PLAYERCOLOR_PURPLE: return CreateColor(1, 0.77, 0.34, 1.00);
                case PlayerColors.PLAYERCOLOR_RED: return CreateColor(1, 0.86, 0.02, 0.02);
                case PlayerColors.PLAYERCOLOR_WHITE: return CreateColor(1, 0.90, 0.90, 0.90);
                case PlayerColors.PLAYERCOLOR_YELLOW: return CreateColor(1, 1.00, 1.00, 0.17);
                case PlayerColors.PLAYERCOLOR_LIGHT_YELLOW: return CreateColor(1, 1, 1, 0.5);
                case PlayerColors.PLAYERCOLOR_LIGHT_PURPLE: return CreateColor(1, 0.7, 0.6, 1);
                case PlayerColors.PLAYERCOLOR_LIGHT_ORANGE: return CreateColor(1, 0.90, 0.65, 0.32);
                case PlayerColors.PLAYERCOLOR_MIDDLE_PURPLE: return CreateColor(1, 0.675, 0.118, 0.725);
                case PlayerColors.PLAYERCOLOR_GOLDENROD: return CreateColor(1, 0.871, 0.624, 0);
                case PlayerColors.PLAYERCOLOR_DARK_LEMON: return CreateColor(1, 0.847, 0.792, 0.039);
                case PlayerColors.PLAYERCOLOR_MIDDLE_BLUE: return CreateColor(1, 0, 0.220, 0.914);
                case PlayerColors.PLAYERCOLOR_MIDDLE_CYAN: return CreateColor(1, 0, 0.639, 0.710);
                case PlayerColors.PLAYERCOLOR_MAROON: return CreateColor(1, 0.514, 0.2, 0.157);
                case PlayerColors.PLAYERCOLOR_LIGHT_BROWN: return CreateColor(1, 0.518, 0.345, 0.075);
                case PlayerColors.PLAYERCOLOR_DARK_ORANGE: return CreateColor(1, 0.878, 0.235, 0);
                case PlayerColors.PLAYERCOLOR_PALE_RED: return CreateColor(1, 0.780, 0.282, 0.239);
                case PlayerColors.PLAYERCOLOR_DARK_INDIGO: return CreateColor(1, 0.306, 0.020, 0.835);
                case PlayerColors.PLAYERCOLOR_PALE_ORANGE: return CreateColor(1, 0.863, 0.471, 0.149);
                case PlayerColors.PLAYERCOLOR_LIGHT_BLACK: return CreateColor(1, 0.251, 0.251, 0.251);
                case PlayerColors.NONE:
                default:
                    return CreateColor(1, 0, 0, 0);
            }
        }

        private Point startPoint;
        private Rectangle rect;

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                startPoint = e.GetPosition(canvas);

                rect = new Rectangle
                {
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(rect, startPoint.X);
                Canvas.SetTop(rect, startPoint.X);
                canvas.Children.Add(rect);
                canvas.Cursor = Cursors.Cross;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Released || rect == null)
                    return;

                var pos = e.GetPosition(canvas);

                var x = Math.Min(pos.X, startPoint.X);
                var y = Math.Min(pos.Y, startPoint.Y);

                var w = Math.Max(pos.X, startPoint.X) - x;
                var h = Math.Max(pos.Y, startPoint.Y) - y;

                rect.Width = w;
                rect.Height = h;

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (rect == null) return;
                int width = (int)Math.Ceiling(rect.Width / TileWidth);
                int height = (int)Math.Ceiling(rect.Height / TileHeight);
                int left = (int)(Canvas.GetLeft(rect) / TileWidth);
                int top = _Game.Map.Height - height - (int)(Canvas.GetTop(rect) / TileHeight);
                _Game.Map.SelectTiles(left, top, width, height);
                ShowMap(false);
                canvas.Children.Remove(rect);
                rect = null;
                canvas.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void HandleError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }

        private void chkIncludeWater_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckboxStates();
        }

        private void SetCheckboxStates()
        {
            if (chkMainlandFirst == null || chkIncludeIslands == null || chkIncludeWater == null) return;
            if (chkIncludeWater.IsChecked.Value)
            {
                chkMainlandFirst.IsEnabled = true;
                if (chkMainlandFirst.IsChecked.Value)
                {
                    chkIncludeIslands.IsEnabled = true;
                }
                else
                {
                    chkIncludeIslands.IsEnabled = false;
                    chkIncludeIslands.IsChecked = true;
                }
            }
            else
            {
                chkMainlandFirst.IsEnabled = false;
                chkIncludeIslands.IsEnabled = false;
            }
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LayoutMap();
        }

        private void LayoutMap()
        {
            TransformGroup tg = MapContainer.LayoutTransform as TransformGroup;
            if (tg == null) return;
            ScaleTransform st = tg.Children[0] as ScaleTransform;
            TranslateTransform tt = tg.Children[1] as TranslateTransform;
            if (double.IsNaN(MapContainer.ActualWidth)) return;

            double oldScale = st.ScaleX;
            double scale = Math.Pow(1.3, ZoomSlider.Value);
            if (MapContainer.ActualWidth * scale < uxMapScrollViewer.ViewportWidth)
                scale = uxMapScrollViewer.ViewportWidth / MapContainer.ActualWidth;
            st.ScaleX = scale;
            st.ScaleY = scale;

            double oldCx = (uxMapScrollViewer.HorizontalOffset + MapContainer.ActualWidth/2) / oldScale;
            double oldCy = (uxMapScrollViewer.VerticalOffset + MapContainer.ActualHeight/2) / oldScale;
            // Keep center point the same with the new scale, so solve the above for HOffset and VOffset given the new scale
            double newHOffset = oldCx * scale - MapContainer.ActualWidth / 2;
            double newVOffset = oldCy * scale - MapContainer.ActualHeight / 2;

            uxMapScrollViewer.ScrollToHorizontalOffset(newHOffset); //((MapContainer.ActualWidth - ox) * st.ScaleX - uxMapScrollViewer.ActualWidth) / 2);
            uxMapScrollViewer.ScrollToVerticalOffset(newVOffset); //((MapContainer.ActualHeight - oy) * st.ScaleY - uxMapScrollViewer.ActualHeight) / 2);
        }

        private void uxMapScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void btnRotateCW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.RotateCW();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnRotateCCW_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.RotateCCW();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnCropToSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.CropToSelection();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnScrambleSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.ScrambleSelection(int.Parse(txtMaxScrambleDistance.Text), chkDontScrambleWater.IsChecked.Value);
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnResize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.ExpandToSize(int.Parse(txtResizeWidth.Text), int.Parse(txtResizeHeight.Text), TerrainTypes.TERRAIN_GRASS, PlotTypes.FLAT, (HorizontalNeighbour)int.Parse(((ComboBoxItem)cboHorizontalAlign.SelectedItem).Tag.ToString()), (VerticalNeighbour)int.Parse(((ComboBoxItem)cboVerticalAlign.SelectedItem).Tag.ToString()));
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnRotateTopLeft_Click(object sender, RoutedEventArgs e)
        {
            RotateAroundCorner(true, true);
        }

        private void btnRotateBottomLeft_Click(object sender, RoutedEventArgs e)
        {
            RotateAroundCorner(true, false);
        }

        private void btnRotateTopRight_Click(object sender, RoutedEventArgs e)
        {
            RotateAroundCorner(false, true);
        }

        private void btnRotateBottomRight_Click(object sender, RoutedEventArgs e)
        {
            RotateAroundCorner(false, false);
        }

        private void RotateAroundCorner(bool leftCorner, bool topCorner)
        {
            try
            {
                _Game.Map = _Game.Map.RotateAroundCorner(leftCorner, topCorner);
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnMirrorWest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.MirrorWest();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnMirrorEast_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.MirrorEast();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnMirrorNorth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.MirrorNorth();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnMirrorSouth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.MirrorSouth();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnRepeatHorizontally_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.RepeatHorizontally(int.Parse(txtRepeatHorTimes.Text));
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnRepeatVertically_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map = _Game.Map.RepeatVertically(int.Parse(txtRepeatVerTimes.Text));
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void btnFindFair_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Game == null || _Game.Map == null) return;
                BalanceCheckerSettings settings = GetBalanceSettings();
                if (_Placer != null)
                {
                    if (_Placer.IsRunning())
                    {
                        _Placer.Stop();
                        if (_Placer.TopResult == null) return;
                        _Placer.TopResult.ApplyToMap(_Game.Map);
                        BalanceReport report = BalanceChecker.CheckBalance(_Game.Map, settings, null);
                        ShowMap(true);
                        ShowBalanceReport(report);
                        btnFindFair.Content = "Continue search";
                    }
                    else
                    {
                        _Placer.Continue();
                        btnFindFair.Content = "Show best result found so far";
                    }
                }
                else
                {
                    _Placer = new StartingLocationPlacer();
                    _Placer.Progress += new EventHandler(_Placer_Progress);
                    List<int> fixedStarts = new List<int>();
                    if(!string.IsNullOrEmpty(txtFixedStartingPositions.Text))
                        foreach (string s in txtFixedStartingPositions.Text.Split(char.Parse(";")))
                            fixedStarts.Add(int.Parse(s));
                    _Placer.FindFairStartingLocations(_Game.Map, settings, fixedStarts, int.Parse(txtMinCapitalDistance.Text));
                    btnFindFair.Content = "Show best result found so far";
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        void _Placer_Progress(object sender, EventArgs e)
        {
            ShowSearchStatus();
        }

        private void ShowSearchStatus()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(new System.Threading.ThreadStart(ShowSearchStatus));
                return;
            }
            if (_Placer.IsRunning())
            {
                if (_Placer.TopResult == null) return;
                lblProgress.Text = _Placer.NumberOfPositionsConsidered + " positions considered... Current best score " + _Placer.TopResultUnfairness;
                _Placer.TopResult.ApplyToMap(_Game.Map);
                BalanceCheckerSettings settings = GetBalanceSettings();
                BalanceReport report = BalanceChecker.CheckBalance(_Game.Map, settings, null);
                ShowMap(true);
                ShowBalanceReport(report);
            }
        }

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _Game.Map.ClearSelection();
                ResizeAndShowMap();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
           
        }

    }
}
