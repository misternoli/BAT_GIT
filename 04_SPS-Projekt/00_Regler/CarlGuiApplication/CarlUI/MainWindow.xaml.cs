using System.Windows;
using System.Timers;
using TGReactorLibrary;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CarlUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static public  DataFields df = new DataFields();
        DataPlotter dpl = new DataPlotter();
        
        public static MainWindow AppWindow;
        public CADSClient myADSClient;

        private bool initialDataFromPlcReceived = false;

        private Timer getNumbersTimer;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly log4net.ILog datalog = log4net.LogManager.GetLogger("datalogger");


        public MainWindow()
        {
            InitializeComponent();
            myADSClient = new CADSClient(Properties.Settings.Default.ADS_Connection_IP, Properties.Settings.Default.ADS_Connection_Port);

            //getInitialSettingsFromPlc();

            getNumbersTimer = new Timer();
            getNumbersTimer.Elapsed += new ElapsedEventHandler(onGetNumbersTimerElapsed);
            getNumbersTimer.Interval = 500;
            getNumbersTimer.Enabled = true;


            this.Closing += new System.ComponentModel.CancelEventHandler(GuiClosing);

            this.DataContext = df;
            //this.DataContext = myADSClient;

            log.Info("GUI Started");
        }
        
        private void GuiClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            System.Windows.Application.Current.Shutdown();

        }
        
        private void onGetNumbersTimerElapsed(object sender, ElapsedEventArgs e)
        {

            float pwmDutyCycle = 0.0f;
            float jacketTempC = 0.0f;
            bool hotvalve = false;
            bool coldvalve = false;
            bool sterivalve = false;

            bool success = false;
            bool successTracker = true;
            df.tPh = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.TPHCURRENT, out success);
            successTracker = successTracker & success;
            df.tOrp = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.TORPCURRENT, out success);
            successTracker = successTracker & success;
            df.ph   = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.PHCURRENT, out success);
            successTracker = successTracker & success;
            df.orp  = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.ORPCURRENT, out success);
            successTracker = successTracker & success;
            df.rpm = myADSClient.ReadInt16FromAds(CADSClient.AdsVariableName.MOTORRPM, out success);
            successTracker = successTracker & success;
            df.tempControlRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.TEMPCONTROLRUNNING, out success);
            successTracker = successTracker & success;
            df.steriRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.STERIRUNNING, out success);
            successTracker = successTracker & success;
            df.stirringRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.MOTORRUNNING, out success);
            successTracker = successTracker & success;
            df.steriTimeToGoMinutes = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.REMAININGSTERITIMEMIN, out success);
            successTracker = successTracker & success;
            df.pressureBar = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.PRESSUREBAR, out success);
            successTracker = successTracker & success;
            df.pressureRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.PRESSURERUNNING, out success);
            successTracker = successTracker & success;
            df.flowLpm = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.FLOWLPM, out success);
            successTracker = successTracker & success;
            df.phRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.PHRUNNING, out success);
            successTracker = successTracker & success;
            df.gasSupplyPressureBar = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.GASSUPPLYPRESSUREBAR, out success);
            successTracker = successTracker & success;
            df.gasSupplyRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.GASSUPPLYRUNNING, out success);
            successTracker = successTracker & success;
            df.gasAnalysisCh4Percent = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.GASANALYSISCH4VALUEPERCENT, out success);
            successTracker = successTracker & success;
            df.gasAnalysisO2Percent = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.GASANALYSISO2VALUEPERCENT, out success);
            successTracker = successTracker & success;
            //df.gasAnalysisAirValve = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.GASANALYSISAIRVALVE, out success);
            //successTracker = successTracker & success;
            df.gasAnalysisRunning = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.GASANALYSISCONTROLRUNNING, out success);
            successTracker = successTracker & success;
            df.spargeVolume = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.SPARGEVOLUME, out success);
            successTracker = successTracker & success;
            df.spargeVolumeCumulative = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.SPARGEVOLUMECUMULATIVE, out success);
            successTracker = successTracker & success;
            df.roomTempC = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.ROOMT, out success);
            successTracker = successTracker & success;
            df.roomRhPercent = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.ROOMRH, out success);
            successTracker = successTracker & success;


            if (initialDataFromPlcReceived)            {
                myADSClient.WriteInt16ToAds(CADSClient.AdsVariableName.CURRENTMOTORSETTING, df.rpmSet);
                myADSClient.WriteSingleToAds(CADSClient.AdsVariableName.TARGETCONTROLTEMP, df.targetControlTemp);
                myADSClient.WriteSingleToAds(CADSClient.AdsVariableName.TARGETSTERITEMP, df.targetSteriTemp);
                myADSClient.WriteInt16ToAds(CADSClient.AdsVariableName.TARGETSTERITIMEMINUTES, df.targetSteriTimeMinutes);
                myADSClient.WriteSingleToAds(CADSClient.AdsVariableName.PRESSURESETBAR, df.pressureSetBar);
                myADSClient.WriteSingleToAds(CADSClient.AdsVariableName.GASSUPPLYPRESSURESETBAR, df.gasSupplyPressureSetBar);
                myADSClient.WriteSingleToAds(CADSClient.AdsVariableName.GASANALYSISCH4VALUEPERCENTSET, df.gasAnalysisCh4PercentSet);
            }
            else
            {
                getInitialSettingsFromPlc();
            }

            
            pwmDutyCycle = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.DUTYCYCLE, out success);
            jacketTempC = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.JACKETTEMPC, out success);
            //hotvalve = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.HOTVALVE, out success);
            //coldvalve = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.COLDVALVE, out success);
            //sterivalve = myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.STERIVALVE, out success);

            datalog.Info($" { df.tPh } , { df.tOrp } , {df.targetControlTemp}, { df.ph } , { df.orp } , { df.rpm }, { df.rpmSet }, {df.targetSteriTemp}, {df.targetSteriTimeMinutes}, {df.steriTimeToGoMinutes}, {df.gasSupplyPressureSetBar}, {df.gasSupplyPressureBar}, ,{df.gasAnalysisCh4PercentSet}, {df.gasAnalysisCh4Percent}, {df.gasAnalysisO2Percent}, ,{df.spargeVolume},{df.roomTempC}, {df.roomRhPercent}, {jacketTempC}, {pwmDutyCycle}");
        }

        private void getInitialSettingsFromPlc()
        {
            bool success = false;
            bool successTracker = true;
            df.rpmSet = myADSClient.ReadInt16FromAds(CADSClient.AdsVariableName.CURRENTMOTORSETTING,  out success);
            successTracker = success;
            df.targetControlTemp = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.TARGETCONTROLTEMP, out success);
            successTracker = successTracker & success;
            df.targetSteriTemp = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.TARGETSTERITEMP, out success);
            successTracker = successTracker & success;
            df.targetSteriTimeMinutes = myADSClient.ReadInt16FromAds(CADSClient.AdsVariableName.TARGETSTERITIMEMINUTES, out success);
            successTracker = successTracker & success;
            df.pressureSetBar = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.PRESSURESETBAR, out success);
            successTracker = successTracker & success;
            df.gasSupplyPressureSetBar = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.GASSUPPLYPRESSURESETBAR, out success);
            successTracker = successTracker & success;
            df.gasAnalysisCh4PercentSet = myADSClient.ReadSingleFromAds(CADSClient.AdsVariableName.GASANALYSISCH4VALUEPERCENTSET, out success);
            successTracker = successTracker & success;
            if (successTracker)
            {
                initialDataFromPlcReceived = true;
            }
        }

        private void buttonMotorStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.MOTORSTARTBUTTON, true);
        }

        private void buttonMotorStop_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.MOTORSTOPBUTTON, true);

        }

        private void buttonColdValve_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.COLDVALVE, true);
        }

        private void buttonHotValve_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.HOTVALVE, true);
        }

        private void buttonTempControlStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.TEMPCONTROLSTART, true);
        }

        private void buttonTempControlStop_Click(object sender, RoutedEventArgs e )
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.TEMPCONTROLSTOP, true);
        }

        private void buttonTempSafetyEnabled_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.TEMPSAFETYENABLED, true);
        }

        private void buttonSteriControlStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.STERISTART, true);
        }

        private void buttonSteriControlStop_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.STERISTOP, true);
        }

        private void buttonPressureStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.PRESSURESTART, true);
        }

        private void buttonPressureStop_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.PRESSURESTOP, true);
        }

        private void buttonPhPumpManual_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.PHPUMPMANUAL, true);
        }

        private void buttonPhControlStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.PHSTART, true);
        }

        private void buttonPhControlStop_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.PHSTOP, true);
        }

        private void buttonGasSupplyStart_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASSUPPLYSTART, true);
        }
        private void buttonGasSupplyStop_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASSUPPLYSTOP, true);
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void buttonGasAnalysisStartStop_Click(object sender, RoutedEventArgs e)
        {
            bool success = false;
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASANALYSISCONTROLSTARTSTOP,  !myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.GASANALYSISCONTROLSTARTSTOP, out success));
        }

        private void buttonGasAnalysisManual_Click(object sender, RoutedEventArgs e)
        {
            bool  success = false;
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASANALYSISCONTROLMANUAL, !myADSClient.ReadBooleanFromAds(CADSClient.AdsVariableName.GASANALYSISCONTROLMANUAL, out success));
        }


        //private void buttonGasAnalysisStart_Click(object sender, RoutedEventArgs e)
        //{
        //    myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASANALYSISAIRVALVE, true);
        //}

        //private void buttonGasAnalysisStop_Click(object sender, RoutedEventArgs e)
        //{
        //    myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.GASANALYSISAIRVALVE,  false);
        //}

        private void buttonResetVolumeCounter_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.RESETSPAREVOLUME, true);
        }

        private void buttonResetTotalVolumeCounter_Click(object sender, RoutedEventArgs e)
        {
            myADSClient.WriteBooleanToAds(CADSClient.AdsVariableName.RESETSPAREVOLUMECUMULATIVE, true);
        }

    }
}
