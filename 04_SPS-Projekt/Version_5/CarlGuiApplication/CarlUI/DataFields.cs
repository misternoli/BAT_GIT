using System.ComponentModel;
using System.Windows.Media;

namespace CarlUI
{
    public class DataFields : INotifyPropertyChanged
    {
        // pH probe + integrated temp 
        private float _tPh;
        public float tPh
        {
            get { return _tPh; }
            set { _tPh = value; Notify("tPh"); }
        }

        private float _ph;
        public float ph
        {
            get { return _ph; }
            set { _ph = value; Notify("ph"); }
        }

        // orp probe + integrated temp
        private float _tOrp;
        public float tOrp
        {
            get { return _tOrp; }
            set { _tOrp = value; Notify("tOrp"); }
        }

        private float _orp;
        public float orp
        {
            get { return _orp; }
            set { _orp = value; Notify("orp"); }
        }

        // rpm control
        private short _rpm;
        public short rpm
        {
            get { return _rpm; }
            set { _rpm = value; Notify("rpm"); }
        }

        private short _rpmSet;
        public short rpmSet
        {
            get { return _rpmSet; }
            set { _rpmSet = value; Notify("rpmSet"); }
        }

        private bool _stirringRunning;
        public bool stirringRunning
        {
            get { return _stirringRunning; }
            set
            {
                _stirringRunning = value;
                if (_stirringRunning)
                {
                    bgColorStirring = "Lightgreen";
                }
                else
                {
                    bgColorStirring = "Red";
                }
                Notify("bgColorStirring");
            }
        }

        private string _bgColorStirring = "Red";
        public string bgColorStirring
        {
            get { return _bgColorStirring; }
            set { _bgColorStirring = value; Notify("bgColorStirring"); }
        }

        // temperature control normal operation
        private float _targetControlTemp;
        public float targetControlTemp
        {
            get { return _targetControlTemp; }
            set { _targetControlTemp = value; Notify("targetControlTemp"); }
        }

        private bool _tempControlRunning;
        public bool tempControlRunning
        {
            get { return _tempControlRunning; }
            set
            {
                _tempControlRunning = value;
                if (_tempControlRunning)
                {
                    bgColorHeatingCooling = "Lightgreen";
                }
                else
                {
                    bgColorHeatingCooling = "Red";
                }
                Notify("tempControlRunning");
            }
        }

        private string _bgColorHeatingCooling = "Red";
        public string bgColorHeatingCooling
        {
            get { return _bgColorHeatingCooling; }
            set { _bgColorHeatingCooling = value; Notify("bgColorHeatingCooling"); }
        }

        // sterilization control 
        private float _targetSteriTemp;
        public float targetSteriTemp
        {
            get { return _targetSteriTemp; }
            set { _targetSteriTemp = value; Notify("targetSteriTemp"); }
        }

        private short _targetSteriTimeMinutes;
        public short targetSteriTimeMinutes
        {
            get { return _targetSteriTimeMinutes; }
            set { _targetSteriTimeMinutes = value; Notify("targetSteriTimeMinutes"); }
        }

        private bool _steriRunning;
        public bool steriRunning
        {
            get { return _steriRunning; }
            set
            {
                _steriRunning = value;
                if (_steriRunning)
                {
                    bgColorSteri = "Lightgreen";
                }
                else
                {
                    bgColorSteri = "Red";
                }
                Notify("bgColorSteri");
            }
        }

        private float _steriTimeToGoMinutes;
        public float steriTimeToGoMinutes
        {
            get { return _steriTimeToGoMinutes; }
            set { _steriTimeToGoMinutes = value; Notify("steriTimeToGoMinutes"); }
        }

        private string _bgColorSteri = "Red";
        public string bgColorSteri
        {
            get { return _bgColorSteri; }
            set { _bgColorSteri = value; Notify("bgColorSteri"); }
        }


        // pressure and flow control
        private float _pressureSetBar;
        public float pressureSetBar
        {
            get { return _pressureSetBar; }
            set { _pressureSetBar = value; Notify("pressureSetBar"); }
        }

        private float _pressureBar;
        public float pressureBar
        {
            get { return _pressureBar; }
            set { _pressureBar = value; Notify("pressureBar"); }
        }

        private bool _pressureRunning;
        public bool pressureRunning
        {
            get { return _pressureRunning; }
            set
            {
                _pressureRunning = value;
                if (_pressureRunning)
                {
                    bgColorPressureRunning = "Lightgreen";
                }
                else
                {
                    bgColorPressureRunning = "Red";
                }
                Notify("bgColorPressureRunning");
            }
        }
        
        private string _bgColorPressureRunning = "Red";
        public string bgColorPressureRunning
        {
            get { return _bgColorPressureRunning; }
            set { _bgColorPressureRunning = value; Notify("bgColorPressureRunning"); }
        }

        private float _flowLpm;
        public float flowLpm
        {
            get { return _flowLpm; }
            set { _flowLpm = value; Notify("flowLpm"); }
        }

        // pH Control
        private bool _phRunning;
        public bool phRunning
        {
            get { return _phRunning; }
            set
            {
                _phRunning = value;
                if (_phRunning)
                {
                    bgColorPhRunning = "Lightgreen";
                }
                else
                {
                    bgColorPhRunning = "Red";
                }
                Notify("bgColorPhRunning");
            }
        }

        private string _bgColorPhRunning = "Red";
        public string bgColorPhRunning
        {
            get { return _bgColorPhRunning; }
            set { _bgColorPhRunning = value; Notify("bgColorPhRunning"); }
        }

        // gas supply pressure
        private float _gasSupplyPressureSetBar;
        public float gasSupplyPressureSetBar
        {
            get { return _gasSupplyPressureSetBar; }
            set { _gasSupplyPressureSetBar = value; Notify("gasSupplyPressureSetBar"); }
        }

        private float _gasSupplyPressureBar;
        public float gasSupplyPressureBar
        {
            get { return _gasSupplyPressureBar; }
            set { _gasSupplyPressureBar = value; Notify("gasSupplyPressureBar"); }
        }

        private bool _gasSupplyRunning;
        public bool gasSupplyRunning
        {
            get { return _gasSupplyRunning; }
            set
            {
                _gasSupplyRunning = value;
                if (_gasSupplyRunning)
                {
                    bgColorGasSupplyRunning = "Lightgreen";
                }
                else
                {
                    bgColorGasSupplyRunning = "Red";
                }
                Notify("bgColorGasSupplyRunning");
            }
        }

        private string _bgColorGasSupplyRunning = "Red";
        public string bgColorGasSupplyRunning
        {
            get { return _bgColorGasSupplyRunning; }
            set { _bgColorGasSupplyRunning = value; Notify("bgColorGasSupplyRunning"); }
        }

        // gas analysis
        private float _gasAnalysisCh4Percent;
        public float gasAnalysisCh4Percent
        {
            get { return _gasAnalysisCh4Percent; }
            set { _gasAnalysisCh4Percent = value; Notify("gasAnalysisCh4Percent"); }
        }

        private float _gasAnalysisCh4PercentSet;
        public float gasAnalysisCh4PercentSet
        {
            get { return _gasAnalysisCh4PercentSet; }
            set { _gasAnalysisCh4PercentSet = value; Notify("gasAnalysisCh4PercentSet"); }
        }

        private float _gasAnalysisO2Percent;
        public float gasAnalysisO2Percent
        {
            get { return _gasAnalysisO2Percent; }
            set { _gasAnalysisO2Percent = value; Notify("gasAnalysisO2Percent"); }
        }


        //private bool _gasAnalysisAirValve;
        //public bool gasAnalysisAirValve
        //{
        //    get { return _gasAnalysisAirValve; }
        //    set
        //    {
        //        _gasAnalysisAirValve = value;
        //        if (_gasAnalysisAirValve)
        //        {
        //            bgColorGasAnalysisRunning = "Lightgreen";
        //        }
        //        else
        //        {
        //            bgColorGasAnalysisRunning = "Red";
        //        }
        //        Notify("bgColorGasAnalysisRunning");
        //    }
        //}

        private bool _gasAnalysisRunning;
        public bool gasAnalysisRunning
        {
            get { return _gasAnalysisRunning; }
            set
            {
                _gasAnalysisRunning = value;
                if (_gasAnalysisRunning)
                {
                    bgColorGasAnalysisRunning = "Lightgreen";
                }
                else
                {
                    bgColorGasAnalysisRunning = "Red";
                }
                Notify("bgColorGasAnalysisRunning");
            }
        }

        private string _bgColorGasAnalysisRunning = "Red";
        public string bgColorGasAnalysisRunning
        {
            get { return _bgColorGasAnalysisRunning; }
            set { _bgColorGasAnalysisRunning = value; Notify("bgColorGasAnalysisRunning"); }
        }

        private bool _gasAnalysisCh4FlowOk;
        public bool gasAnalysisCh4FlowOk
        {
            get { return _gasAnalysisCh4FlowOk; }
            set
            {
                _gasAnalysisCh4FlowOk = value;
                if (_gasAnalysisCh4FlowOk)
                {
                    bgColorGasAnalysisCh4FlowOk = "Lightgreen";
                }
                else
                {
                    bgColorGasAnalysisCh4FlowOk = "Red";
                }
                Notify("bgColorGasAnalysisCh4FlowOk");
            }
        }

        private string _bgColorGasAnalysisCh4FlowOk = "Red";
        public string bgColorGasAnalysisCh4FlowOk
        {
            get { return _bgColorGasAnalysisCh4FlowOk; }
            set { _bgColorGasAnalysisCh4FlowOk = value; Notify("bgColorGasAnalysisCh4FlowOk"); }
        }

        private bool _gasAnalysisO2FlowOk;
        public bool gasAnalysisO2FlowOk
        {
            get { return _gasAnalysisO2FlowOk; }
            set
            {
                _gasAnalysisO2FlowOk = value;
                if (_gasAnalysisO2FlowOk)
                {
                    bgColorGasAnalysisO2FlowOk = "Lightgreen";
                }
                else
                {
                    bgColorGasAnalysisO2FlowOk = "Red";
                }
                Notify("bgColorGasAnalysisO2FlowOk");
            }
        }

        private string _bgColorGasAnalysisO2FlowOk = "Red";
        public string bgColorGasAnalysisO2FlowOk
        {
            get { return _bgColorGasAnalysisO2FlowOk; }
            set { _bgColorGasAnalysisO2FlowOk = value; Notify("bgColorGasAnalysisO2FlowOk"); }
        }

        private float _spargeVolume;
        public float spargeVolume
        {
            get { return _spargeVolume; }
            set { _spargeVolume = value; Notify("spargeVolume"); }
        }

        private float _spargeVolumeCumulative;
        public float spargeVolumeCumulative
        {
            get { return _spargeVolumeCumulative; }
            set { _spargeVolumeCumulative = value; Notify("spargeVolumeCumulative"); }
        }

        // room sensor T, RH
        private float _roomTempC;
        public float roomTempC
        {
            get { return _roomTempC; }
            set { _roomTempC = value; Notify("roomTempC"); }
        }

        private float _roomRhPercent;
        public float roomRhPercent
        {
            get { return _roomRhPercent; }
            set { _roomRhPercent = value; Notify("roomRhPercent"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string argument)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(argument));
            }
        }
        
    }
}
