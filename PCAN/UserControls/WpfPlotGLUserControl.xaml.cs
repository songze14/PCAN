using PCAN.Tools;
using ReactiveUI;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;
using Splat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Label = System.Windows.Controls.Label;

namespace PCAN.UserControls
{
    /// <summary>
    /// XYLable.xaml 的交互逻辑
    /// </summary>
    public partial class WpfPlotGLUserControl : UserControl
    {
        private Dictionary<string, Signal> _signals = new Dictionary<string, Signal>();
        Dictionary<string, Crosshair> _crosshairs = new Dictionary<string, Crosshair>();
        Dictionary<string, Label> _labelCs = new Dictionary<string, Label>();
        Dictionary<string, LeftAxis> _leftAxis = new Dictionary<string, LeftAxis>();

        private int PlotCount;
        public WpfPlotGLUserControl()
        {
            InitializeComponent();
            this.DataContext = this;
            AddMenu();

        }

        private void WpfPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var posiWithPlot = e.GetPosition(this);

                Pixel mousePixel = new(posiWithPlot.X, posiWithPlot.Y);
                Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);
                foreach (var item in _signals)
                {
                    var _datastreamer = item.Value;
                    var MyCrosshair = _crosshairs[item.Key];
                    var XYLableC = _labelCs[item.Key];
                    DataPoint nearest = _datastreamer.GetNearestX(mouseLocation, WpfPlot1.Plot.LastRender);


                    //// place the crosshair over the highlighted point
                    if (nearest.IsReal)
                    {
                        XYLableC.Visibility = Visibility.Visible;
                        XYLableC.Content = $"X:{nearest.X},Y:{nearest.Y}";

                        var x = MyCrosshair.Axes.GetPixel(nearest.Coordinates);

                        XYLableC.Margin = new Thickness(x.X - 30, x.Y - 30, 0, 0);
                        MyCrosshair.Position = nearest.Coordinates;
                        MyCrosshair.IsVisible = true;
                        WpfPlot1.Refresh();

                    }

                    // hide the crosshair when no point is selected
                    if (!nearest.IsReal && MyCrosshair.IsVisible)
                    {
                        XYLableC.Visibility = Visibility.Hidden;

                        MyCrosshair.IsVisible = false;
                        WpfPlot1.Refresh();

                    }
                }

            }
            catch (Exception ex)
            {


            }
        }
        /// <summary>
        /// 添加一条线
        /// </summary>
        /// <param name="ys"></param>
        /// <returns>返回线的名称</returns>
        public (bool, string) AddSignal(List<double> ys, string? key = null)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    key = $"sig{_signals.Count}";
                }
                var color = WpfPlot1.Plot.Add.Palette.GetColor(_signals.Count);
                var singnal = WpfPlot1.Plot.Add.Signal(ys, color: color);
                if (PlotCount >= 1)
                {
                    var yaxis = WpfPlot1.Plot.Axes.AddLeftAxis();
                    yaxis.Color(color);
                    singnal.Axes.YAxis = yaxis;
                    _leftAxis.Add(key, yaxis);
                }
                else
                {

                    singnal.Axes.YAxis = WpfPlot1.Plot.Axes.Left;

                }
                _signals.Add(key, singnal);
                var crosshair = WpfPlot1.Plot.Add.Crosshair(0, 0);
                crosshair.IsVisible = false;
                crosshair.MarkerShape = MarkerShape.OpenCircle;
                crosshair.MarkerSize = 15;
                crosshair.LineWidth = 2;
                crosshair.LineColor = color;
                crosshair.HorizontalLine.LinePattern = LinePattern.Dotted;
                crosshair.Axes.YAxis = singnal.Axes.YAxis;
                _crosshairs.Add(key, crosshair);
                var labelC = new Label();
                labelC.Visibility = Visibility.Hidden;
                labelC.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                labelC.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                labelC.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                labelC.FontSize = 15;
                labelC.Width = 100;
                labelC.Height = 50;
                wpfplotdock.Children.Add(labelC);
                _labelCs.Add(key, labelC);
                PlotCount++;
                return (true, key);
            }
            catch (Exception ex)
            {

                return (false, ex.Message);
            }

        }
        /// <summary>
        /// 移除一条线
        /// </summary>
        /// <param name="key">线的名称</param>
        /// <returns></returns>
        public bool RemoveSignal(string key)
        {
            try
            {
                if (_signals.ContainsKey(key))
                {
                    WpfPlot1.Plot.Remove(_signals[key]);
                    WpfPlot1.Plot.Remove(_crosshairs[key]);

                    wpfplotdock.Children.Remove(_labelCs[key]);
                    if (_leftAxis.ContainsKey(key))
                    {
                        WpfPlot1.Plot.Axes.Remove(_leftAxis[key]);
                        _leftAxis.Remove(key);
                    }
                    _signals.Remove(key);
                    _crosshairs.Remove(key);
                    _labelCs.Remove(key);
                    WpfPlot1.Refresh();
                    PlotCount--;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void ClearAllSignal()
        {
            foreach (var item in _signals)
            {
                RemoveSignal(item.Key);
            }



        }
        public void SetLimit(int limitcount = 1000)
        {
            UIHelper.RunInUIThread((d) =>
            {
                WpfPlot1.Plot.Axes.AutoScaleExpandY();
                var xlimit = WpfPlot1.Plot.Axes.GetDataLimits().XRange;
                WpfPlot1.Plot.Axes.SetLimitsX(xlimit.Max - limitcount, xlimit.Max);
                WpfPlot1.Refresh();

            });
        }
        public void ResetPlot()
        {
            WpfPlot1.Plot.Axes.AutoScaleExpandY();
        }
        private void AddMenu()
        {
            this.WpfPlot1.Menu?.Clear();
            this.WpfPlot1.Menu?.Add("重置视图", plot =>
            {
                plot.Axes.AutoScale();
            });
        }
    }
}
