using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Media;

namespace PCAN.ViewModel.Window
{
    public class SignalSettingWindowViewModel:ReactiveObject
    {
        public SignalSettingWindowViewModel(WpfPlotGL wpfPlot)
        {
            WpfPlot = wpfPlot;
            SignalSourceList
             .Connect()
             .Bind(out _Signals)
             .Subscribe();
            wpfPlot.Plot.PlottableList.Where(o=>o is Signal).Select(o=>o as Signal).ToList().ForEach(o=>SignalSourceList.Add(o));
            this.WhenAnyValue(o=>o.SelectedSignal).Subscribe(signal =>
            {
                if (signal != null)
                {
                    XOFF = signal.Data.XOffset;
                    YOFF = signal.Data.YOffset;
                    Gain = signal.Data.YScale;
                    Color = Color.FromArgb(signal.Color.A, signal.Color.R, signal.Color.G, signal.Color.B);
                }
            });
            this.WhenAnyValue(o => o.Color).Subscribe(color =>
            {
                if (SelectedSignal != null)
                {
                    SelectedSignal.Color = new ScottPlot.Color(color.R, color.G, color.B, color.A);
                    WpfPlot.Refresh();
                }
            });
         
            this.XOFFChangeCommand = ReactiveCommand.Create<double>(xoff =>
            {
                if (SelectedSignal != null)
                {
                    SelectedSignal.Data.XOffset = xoff;
                    WpfPlot.Plot.Axes.AutoScale(); 
                    WpfPlot.Refresh();
                }
            });
            this.YOFFChangeCommand = ReactiveCommand.Create<double>(yoff =>
            {
                if (SelectedSignal != null)
                {
                    SelectedSignal.Data.YOffset = yoff;
                    WpfPlot.Plot.Axes.AutoScale(); 
                    WpfPlot.Refresh();
                }
            });
            this.GainChangeCommand = ReactiveCommand.Create<double>(gain =>
            {
                if (SelectedSignal != null)
                {
                    SelectedSignal.Data.YScale = gain;
                    WpfPlot.Plot.Axes.AutoScale();
                    WpfPlot.Refresh();
                }
            });
            this.RestCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedSignal != null)
                {
                    SelectedSignal.Data.XOffset = 0;
                    SelectedSignal.Data.YOffset = 0;
                    SelectedSignal.Data.YScale = 1;
                    XOFF = 0;
                    YOFF = 0;
                    Gain = 1;
                    WpfPlot.Plot.Axes.AutoScale();
                    WpfPlot.Refresh();
                }
            });
        }
        [Reactive]
        public Signal SelectedSignal { get; set; }
        [Reactive]
        public double XOFF { get; set; }
        [Reactive]
        public double YOFF { get; set; }
        [Reactive]
        public double Gain { get; set; } = 1;
        [Reactive] 
        public Color Color { get; set; }
        public ReactiveCommand<double, Unit> XOFFChangeCommand { get; }
        public ReactiveCommand<double, Unit> YOFFChangeCommand { get; }
        public ReactiveCommand<double, Unit> GainChangeCommand { get; }
        public ReactiveCommand<Unit,Unit> RestCommand { get; }
        public SourceList<Signal> SignalSourceList { get; set; }=new ();
        public ReadOnlyObservableCollection<Signal> _Signals;

        public ReadOnlyObservableCollection<Signal> Signals =>_Signals;
        public WpfPlotGL WpfPlot { get; set; }
    }
}
