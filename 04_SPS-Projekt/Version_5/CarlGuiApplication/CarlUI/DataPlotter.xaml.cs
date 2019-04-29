using System;
using System.Windows.Controls;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows.Threading;


namespace CarlUI
{
    /// <summary>
    /// Interaction logic for DataPlotter.xaml
    /// </summary>
    /// 

    public partial class DataPlotter : Page
    {
        public PlotModel DataPlot { get; set; }
        private double _xValue = 1;
        public FunctionSeries pHSeries;
        public FunctionSeries ch4Series;

        public DataPlotter()
        {
            InitializeComponent();
            DataContext = this;
            DataPlot = new PlotModel();

            // time axis
            DateTimeAxis xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = DateTimeAxis.ToDouble(DateTime.Now),
                Maximum = DateTimeAxis.ToDouble(DateTime.Now.AddHours(2)),
                MinorIntervalType = DateTimeIntervalType.Seconds,
                IntervalType = DateTimeIntervalType.Minutes,
                StringFormat = "HH:mm:ss"
            };
            DataPlot.Axes.Add(xAxis);

            // pH-axis 
            LinearAxis pHAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 5,
                Maximum = 9,
                MajorStep = 1,
                MinorStep = 0.1,
                TickStyle = TickStyle.Inside,
                AxislineColor = OxyColors.Red,
                Title = "pH",
                TitleColor = OxyColors.Red,
                TextColor = OxyColors.Red,
                IsZoomEnabled = false
            };
            DataPlot.Axes.Add(pHAxis);

            // ch4Axis
            LinearAxis ch4Axis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "ch4",
                Minimum = 0,
                Maximum = 100,
                MajorStep = 20,
                MinorStep = 5,
                AxislineColor = OxyColors.Green,
                AxisDistance = 20,
                TextColor = OxyColors.Green,
                TickStyle = TickStyle.Inside,
                Title = "CH4 [%]", 
                TitleColor = OxyColors.Green
            };
            DataPlot.Axes.Add(ch4Axis);

            // pH Data Series
            pHSeries = new FunctionSeries();
            pHSeries.TrackerFormatString = "{2:HH:mm:ss}";
            pHSeries.Color = OxyColors.Red;
            DataPlot.Series.Add(pHSeries);

            // volume Data Series
            ch4Series = new FunctionSeries();
            ch4Series.TrackerFormatString = "{2:HH:mm:ss}";
            ch4Series.Color = OxyColors.Green;
            ch4Series.YAxisKey = "ch4";
            DataPlot.Series.Add(ch4Series);
            
            var dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0,30) };
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Start();
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            double dtNowDbl = DateTimeAxis.ToDouble(DateTime.Now);
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                pHSeries.Points.Add(new DataPoint(dtNowDbl, MainWindow.df.ph));
                ch4Series.Points.Add(new DataPoint(dtNowDbl, MainWindow.df.gasAnalysisCh4Percent));
                DataPlot.InvalidatePlot(true);
                _xValue++;
            });
            // limit the number of points shown
            if ((DataPlot.Series[0] as FunctionSeries).Points.Count > 10000)
            {
                (DataPlot.Series[0] as FunctionSeries).Points.RemoveAt(0); //remove oldest point
                (DataPlot.Series[1] as FunctionSeries).Points.RemoveAt(0); //remove oldest point
            }
            // update the moving axes

            if ((dtNowDbl > DataPlot.Axes[0].Maximum))
            {
                //the pan is the actual max position of the observed Axis minus the maximum data position times the scaling factor
                var xPan = (DataPlot.Axes[0].ActualMaximum - DataPlot.Axes[0].DataMaximum) * DataPlot.Axes[0].Scale;
                DataPlot.Axes[0].Pan(xPan);
            }
        }

    }
}
