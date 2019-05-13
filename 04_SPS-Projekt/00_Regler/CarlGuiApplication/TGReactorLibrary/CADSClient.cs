using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TwinCAT.Ads;
using System.Configuration;


namespace TGReactorLibrary
{
    public class AdsVariable
    {
        public CADSClient.AdsVariableName VariableNameEnum { set; get; }
        public string VariableName { set; get; }
        public int Handle { set; get; }
        public VariableType DataType { set; get; }
        public DataFlow Direction { set; get; }

        public enum VariableType
        {
            DT_BOOL
            , DT_INT16
            , DT_INT32
            , DT_BYTE
            , DT_DOUBLE
            , DT_SINGLE
            , DT_STRING
            , DT_ARRAYINT
        }

        public enum DataFlow
        {
            TOADS       //For SPS Input (Write) only
            , FROMADS   //For SPS Output (Read) only
            , TOFROMADS //To Read and Write from SPS
        }
    }


    //https://infosys.beckhoff.com/index.php?content=../content/1031/tcquickstart/html/tcquickstart_samplecsharp.htm&id=21386

    public class CADSClient
    {
        private string _netID;
        private int _srvPort;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TcAdsClient tcClient;
        private System.Threading.Thread tAdsConnectorThread;
        private DateTime dtLastConnectionAttempt;
        private bool bThreadAlive;
        private bool bAdsHasBeenStopped;

        public string tPhCurrent { get; set; }


        List<AdsVariable> varList = new List<AdsVariable>();

        public delegate void VariableUpdateHandler(object sender, AdsVariable var, AdsNotificationExEventArgs e);
        public event VariableUpdateHandler VariableUpdate;

        public delegate void AdsStatusUpdateHandler(object sender, AdsStateChangedEventArgs e);
        public event AdsStatusUpdateHandler AdsStatusUpdate;

        public enum AdsVariableName
        {
            COLDVALVE
            , HOTVALVE
            , STERIVALVE
            , MOTORSTARTBUTTON
            , MOTORSTART
            , MOTORSTOPBUTTON
            , CURRENTMOTORSETTING
            , MOTORRPM
            , MOTORRUNNING
            , TPHCURRENT
            , PHCURRENT
            , TORPCURRENT
            , ORPCURRENT
            , TARGETCONTROLTEMP
            , TEMPCONTROLSTART
            , TEMPCONTROLSTOP
            , TEMPCONTROLRUNNING
            , TEMPSAFETYENABLED
            , STERISTART
            , STERISTOP
            , STERIRUNNING
            , TARGETSTERITEMP
            , TARGETSTERITIMEMINUTES
            , REMAININGSTERITIMEMIN
            , PRESSURESTART
            , PRESSURESTOP
            , PRESSURERUNNING
            , PRESSUREBAR
            , PRESSURESETBAR
            , FLOWLPM
            , PHSTART
            , PHSTOP
            , PHRUNNING
            , PHPUMPMANUAL
            , GASSUPPLYPRESSURESETBAR
            , GASSUPPLYPRESSUREBAR
            , GASSUPPLYRUNNING
            , GASSUPPLYSTART
            , GASSUPPLYSTOP
            , GASANALYSISAIRVALVE
            , GASANALYSISCH4FLOW
            , GASANALYSISO2FLOW
            , GASANALYSISCH4VALUEPERCENT
            , GASANALYSISCH4VALUEPERCENTSET
            , GASANALYSISO2VALUEPERCENT
            , GASANALYSISCONTROLSTARTSTOP
            , GASANALYSISCONTROLMANUAL
            , GASANALYSISCONTROLRUNNING
            , ROOMT
            , ROOMRH
            , SPARGEVOLUME
            , RESETSPAREVOLUME
            , SPARGEVOLUMECUMULATIVE
            , RESETSPAREVOLUMECUMULATIVE
            , JACKETTEMPC
            , DUTYCYCLE
            , LOOPCOUNTER
            , P_C
            , I_C
            , D_C
            , P_J

        }

        public CADSClient(string netID, int srvPort)
        {
            _netID = netID;
            _srvPort = srvPort;

            // motor control
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.MOTORSTARTBUTTON,       VariableName = "GVL_MotorControl.motorStartButton", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.MOTORSTART,                      VariableName = "GVL_MotorControl.motorStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.MOTORSTOPBUTTON,        VariableName = "GVL_MotorControl.motorStopButton", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.CURRENTMOTORSETTING, VariableName = "GVL_MotorControl.currentMotorSpeedSetting", DataType = AdsVariable.VariableType.DT_INT16, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.MOTORRPM,                        VariableName = "GVL_MotorControl.motorRpm", DataType = AdsVariable.VariableType.DT_INT16, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.MOTORRUNNING,              VariableName = "GVL_MotorControl.motorRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });

            // pH & ORP & pH controller
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PHCURRENT, VariableName = "GVL_PhOrpControl.phCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.ORPCURRENT, VariableName = "GVL_PhOrpControl.orpCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PHSTART, VariableName = "GVL_PhOrpControl.phStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PHSTOP, VariableName = "GVL_PhOrpControl.phStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PHRUNNING, VariableName = "GVL_PhOrpControl.phRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PHPUMPMANUAL, VariableName = "GVL_PhOrpControl.phPumpManual", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });

            // temperature control & sterilization
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TPHCURRENT, VariableName = "GVL_TemperatureControl.tPhCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TORPCURRENT, VariableName = "GVL_TemperatureControl.tOrpCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.COLDVALVE, VariableName = "GVL_TemperatureControl.coldValve", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.HOTVALVE, VariableName = "GVL_TemperatureControl.hotValve", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.STERIVALVE, VariableName = "GVL_TemperatureControl.steriValve", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TARGETCONTROLTEMP, VariableName = "GVL_TemperatureControl.targetControlTemp", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TEMPCONTROLSTART, VariableName = "GVL_TemperatureControl.tempControlStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TEMPCONTROLSTOP, VariableName = "GVL_TemperatureControl.tempControlStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TEMPCONTROLRUNNING, VariableName = "GVL_TemperatureControl.tempControlRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TEMPSAFETYENABLED, VariableName = "GVL_TemperatureControl.tempSafetyEnabled", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.STERISTART, VariableName = "GVL_TemperatureControl.steriStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.STERISTOP, VariableName = "GVL_TemperatureControl.steriStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.STERIRUNNING, VariableName = "GVL_TemperatureControl.steriRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TARGETSTERITEMP, VariableName = "GVL_TemperatureControl.targetSteriTemp", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TARGETSTERITIMEMINUTES, VariableName = "GVL_TemperatureControl.targetSteriTimeMinutes", DataType = AdsVariable.VariableType.DT_INT16, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.REMAININGSTERITIMEMIN, VariableName = "GVL_TemperatureControl.remainingSteriTimeMinutes", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.JACKETTEMPC, VariableName = "GVL_TemperatureControl.jacketTempC", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });


            // reactor pressure control 
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PRESSURESTART, VariableName = "GVL_ReactorPressureControl.pressureStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PRESSURESTOP, VariableName = "GVL_ReactorPressureControl.pressureStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PRESSURERUNNING, VariableName = "GVL_ReactorPressureControl.pressureRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PRESSUREBAR, VariableName = "GVL_ReactorPressureControl.pressureBar", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.PRESSURESETBAR, VariableName = "GVL_ReactorPressureControl.pressureSetBar", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.TOFROMADS });

            // gas input supply 
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASSUPPLYPRESSURESETBAR, VariableName = "GVL_GasSupply.gasSupplyPressureSetBar", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASSUPPLYPRESSUREBAR, VariableName = "GVL_GasSupply.gasSupplyPressureBar", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASSUPPLYRUNNING, VariableName = "GVL_GasSupply.gasSupplyRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASSUPPLYSTART, VariableName = "GVL_GasSupply.gasSupplyStart", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASSUPPLYSTOP, VariableName = "GVL_GasSupply.gasSupplyStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });

            // flow control
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.FLOWLPM, VariableName = "GVL_FlowControl.flowRateLitersPerMinute", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });

            // gas analysis
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCH4FLOW, VariableName = "GVL_GasAnalysis.ch4FlowSensor", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCH4VALUEPERCENT, VariableName = "GVL_GasAnalysis.ch4ValuePercent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCH4VALUEPERCENTSET, VariableName = "GVL_GasAnalysis.ch4ValuePercentSet", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISO2FLOW, VariableName = "GVL_GasAnalysis.o2FlowSensor", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISO2VALUEPERCENT, VariableName = "GVL_GasAnalysis.o2ValuePercent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCONTROLSTARTSTOP, VariableName = "GVL_GasAnalysis.gasAnalysisControlStartStop", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCONTROLMANUAL, VariableName = "GVL_GasAnalysis.gasAnalysisControlManual", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.GASANALYSISCONTROLRUNNING, VariableName = "GVL_GasAnalysis.gasAnalysisControlRunning", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOADS });

            // flow control
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.SPARGEVOLUME, VariableName = "GVL_FlowControl.spargeVolume", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.RESETSPAREVOLUME, VariableName = "GVL_FlowControl.resetSpargeVolume", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.SPARGEVOLUMECUMULATIVE, VariableName = "GVL_FlowControl.spargeVolumeCumulative", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.RESETSPAREVOLUMECUMULATIVE, VariableName = "GVL_FlowControl.resetSpargeVolumeCumulative", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });

            // room T, rH
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.ROOMT, VariableName = "GVL_TempRHSensor.roomTemp", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.ROOMRH, VariableName = "GVL_TempRHSensor.roomRh", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });

            // Control variable
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.P_C, VariableName = "PRG_BAT_TemperatureControl.CAS_Controller.P_C", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.I_C, VariableName = "PRG_BAT_TemperatureControl.CAS_Controller.I_C", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.D_C, VariableName = "PRG_BAT_TemperatureControl.CAS_Controller.D_C", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.P_J, VariableName = "PRG_BAT_TemperatureControl.CAS_Controller.J_Controll.P_J", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });


            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.LOOPCOUNTER, VariableName = "GVL_Common.loopCounter", DataType = AdsVariable.VariableType.DT_INT16, Direction = AdsVariable.DataFlow.FROMADS });



            // temp % duty cycle for testing
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.DUTYCYCLE, VariableName = "GVL_TemperatureControl.pwmDutyCycle", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
            varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.RESETSPAREVOLUME, VariableName = "GVL_FlowControl.resetSpargeVolume", DataType = AdsVariable.VariableType.DT_BOOL, Direction = AdsVariable.DataFlow.TOFROMADS });

            //tAdsConnectorThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadStart(netID, srvPort)));
            tAdsConnectorThread = new System.Threading.Thread( () => AdsThreadStart(netID, srvPort));
            bThreadAlive = true;
            tAdsConnectorThread.IsBackground = true;
            tAdsConnectorThread.Start();
            System.Threading.Thread.Sleep(2000); //We wait 2 seconds to let ADS initalize
        }

        public void Close()
        {
            DateTime start = DateTime.UtcNow;
            bThreadAlive = false;
            while (tAdsConnectorThread.ThreadState != System.Threading.ThreadState.Stopped)
            {
                System.Threading.Thread.Sleep(10);
                if((DateTime.UtcNow - start).TotalSeconds > 30)
                {
                    log.Warn("Thread had to be aborted");
                    tAdsConnectorThread.Abort();
                    break;
                }
            }
            tAdsConnectorThread = null;
        }

        public bool IsConnected
        {
            get
            {
                if(tcClient == null)
                {
                    return false;
                }else
                {
                    return tcClient.IsConnected;
                }
            }
        }

        public string AdsClientNetId
        {
            get
            {
                if (tcClient == null)
                {
                    return "N/A";
                }
                else
                {
                    return tcClient.ClientNetID;
                }
            }
        }

        public string AdsServerNetId
        {
            get
            {
                if (tcClient == null)
                {
                    return "N/A";
                }
                else
                {
                    return tcClient.ServerNetID;
                }
            }
        }

        public bool ReadBooleanFromAds(AdsVariableName Parameter, out bool success)
        {
            success = false;
            bool returnValue = false;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if(obj != null)
                    {
                        returnValue = Boolean.Parse(tcClient.ReadAny(obj.Handle, typeof(Boolean)).ToString());
                        success = true;
                    }

                } catch (Exception Err)
                {
                    log.Error("Error while reading boolean value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }

            }
            return returnValue;
        }

        public byte ReadByteFromAds(AdsVariableName Parameter, out bool success)
        {
            success = false;
            byte returnValue = 0;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        returnValue = Byte.Parse(tcClient.ReadAny(obj.Handle, typeof(Byte)).ToString());
                        success = true;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while reading byte value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }

            }
            return returnValue;
        }

        public short ReadInt16FromAds(AdsVariableName Parameter, out bool success)
        {
             success = false;
            short returnValue = 0;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        returnValue = short.Parse(tcClient.ReadAny(obj.Handle, typeof(short)).ToString());
                        success = true;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while reading int16 value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }
            }
            return returnValue;
        }

        public int ReadInt32FromAds(AdsVariableName Parameter, out bool success)
        {
            success = false;
            int returnValue = 0;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        returnValue = int.Parse(tcClient.ReadAny(obj.Handle, typeof(int)).ToString());
                        success = true;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while reading int32 value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }
            }
            return returnValue;
        }

        public float ReadSingleFromAds(AdsVariableName Parameter, out bool success)
        {
            success = false;
            float returnValue = 0.0F;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        returnValue = float.Parse(tcClient.ReadAny(obj.Handle, typeof(float)).ToString());
                        success = true;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while reading float value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }
            }
            return returnValue;
        }

        public double ReadDoubleFromAds(AdsVariableName Parameter, out bool success)
        {
             success = false;
            double returnValue = 0.0;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        returnValue = double.Parse(tcClient.ReadAny(obj.Handle, typeof(double)).ToString());
                        success = true;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while reading double value from ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                    success = false;
                }
            }
            return returnValue;
        }
        
        public bool WriteBooleanToAds(AdsVariableName Parameter, bool val)
        {
            bool success = false;
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if(obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, val);
                        success = true;
                    }
                    else
                    {
                        log.Warn("Could not write boolean value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                        success = false;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while writing boolean value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString() + ", Err:" + Err.Message);
                    success = false;
                }
            }
            return success;
        }

        public bool WriteByteToAds(AdsVariableName Parameter, Byte val)
        {
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, val);
                        return true;
                    }
                    else
                    {
                        log.Warn("Could not write boolean value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                        return false;
                    }

                    //Not used as SPS might change values quite fast
                    //if (ReadBooleanFromAds(Parameter) == val)
                    //{
                    //    return true;
                    //}
                    //else
                    //{
                    //    log.Warn("Could not write boolean value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                    //    return false;
                    //}

                }
                catch (Exception Err)
                {
                    log.Error("Error while writing boolean value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString() + ", Err:" + Err.Message);
                    //MessageBox.Show("Error while writing to ADS: " + Err.Message);
                }
            }
            return false;
        }

        public bool WriteInt16ToAds(AdsVariableName Parameter, short val)
        {
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, val);
                        return true;
                    }
                    else
                    {
                        log.Warn("Could not write int16 value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                        return false;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while writing int16 value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString() + ", Err:" + Err.Message);
                }
            }
            return false;
        }


        public bool WriteSingleToAds(AdsVariableName Parameter, float val)
        {
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, val);
                        return true;
                    }
                    else
                    {
                        log.Warn("Could not write float value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                        return false;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while writing float value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString() + ", Err:" + Err.Message);
                }
            }
            return false;
        }

        public bool WriteDoubleToAds(AdsVariableName Parameter, double val)
        {
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, val);
                        return true;
                    }
                    else
                    {
                        log.Warn("Could not write double value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString());
                        return false;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while writing double value to ADS Server. Parameter: " + Parameter.ToString() + ", Value: " + val.ToString() + ", Err:" + Err.Message);
                }
            }
            return false;
        }

        public bool WriteIntArrayToAds(AdsVariableName Parameter, int[] vals)
        {
            if (this.IsConnected)
            {
                try
                {
                    var obj = varList.Where(element => element.VariableNameEnum == Parameter).FirstOrDefault();
                    if (obj != null)
                    {
                        tcClient.WriteAny(obj.Handle, vals);
                        return true;
                    }
                    else
                    {
                        log.Warn("Could not write double value to ADS Server. Parameter not found. Parameter: " + Parameter.ToString());
                        return false;
                    }
                }
                catch (Exception Err)
                {
                    log.Error("Error while writing double value to ADS Server. Parameter: " + Parameter.ToString() + ", Err:" + Err.Message);
                }
            }
            return false;
        }

        //public void UpdateADSValules()
        //{

        //    tPhCurrent = ReadInt16FromAds(AdsVariableName.TPHCURRENT, ).ToString();

        //    varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TPHCURRENT, VariableName = "GVL_InputsOutputs.tPhCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });
        //    varList.Add(new AdsVariable() { VariableNameEnum = AdsVariableName.TORPCURRENT, VariableName = "GVL_InputsOutputs.tOrpCurrent", DataType = AdsVariable.VariableType.DT_SINGLE, Direction = AdsVariable.DataFlow.FROMADS });


        //    foreach (AdsVariable var in varList)
        //    {
        //        if( var.DataType == AdsVariable.VariableType.DT_BOOL)
        //        {

        //        } else if (var.DataType == AdsVariable.VariableType.DT_BOOL)
        //        {

        //        }
        //    }
        //}

        /*
         * var fibNumbers = new List<int> { 0, 1, 1, 2, 3, 5, 8, 13 };
int count = 0;
foreach (int element in fibNumbers)
{
    count++;
    Console.WriteLine($"Element #{count}: {element}");
}
Console.WriteLine($"Number of elements: {count}");
*/

        private void AdsThreadStart( string netID, int srvPort)
        {
            log.Info("AdsClient Thread started");
            //Properties.Settings.Default.ADS_Connection_IP, Properties.Settings.Default.ADS_Connection_Port
            ConnectAdsClient(netID, srvPort);
            while (bThreadAlive)
            {
                System.Threading.Thread.Sleep(100);
                //If AdsClient is not connected, it will try to reconnect every 5 seconds
                if (this.IsConnected == false && (DateTime.UtcNow - dtLastConnectionAttempt).TotalSeconds > 5)
                {
                    ConnectAdsClient(netID, srvPort);
                } else if (this.IsConnected)
                {
                    //Try to read state
                    StateInfo state;
                    AdsErrorCode res;
                    res = tcClient.TryReadState(out state);
                    if(res != AdsErrorCode.NoError || state.AdsState != AdsState.Run)
                    {
                        //Error while reading state
                        tcClient.Dispose();
                        tcClient = null;
                        ConnectAdsClient(netID, srvPort);
                    }
                    //Update KeepAlive on SPS
                    //WriteDoubleToAds(AdsVariableName.KEEPALIVE_GUI, DateTime.UtcNow.Ticks);
                }
            }
            if (tcClient != null)
            {
                tcClient.Dispose();
            }
            log.Info("AdsClient Thread stopped");
        }

        private void ConnectAdsClient(string netID, int srvPort)
        {
            bAdsHasBeenStopped = false;
            try
            {
                dtLastConnectionAttempt = DateTime.UtcNow;
                tcClient = new TcAdsClient();

                //tcClient.Connect(Properties.Settings.Default.ADS_Connection_IP, Properties.Settings.Default.ADS_Connection_Port);
                tcClient.Connect(netID, srvPort);
                tcClient.Timeout = 500;
                tcClient.AdsNotificationEx += new AdsNotificationExEventHandler(adsClient_AdsNotificationEx);
                
                tcClient.Synchronize = false; //Otherwise, NotificationEvent will be raised only in main thread
                tcClient.AdsStateChanged += new AdsStateChangedEventHandler(adsClient_AdsStateChanged);

                //TcAdsSymbolInfoLoader loader;
                //loader = tcClient.CreateSymbolInfoLoader();
                //string var = "";
                //int cnt = 0;
                //foreach (ITcAdsSymbol symbol in loader)
                //{
                //    System.Diagnostics.Debug.WriteLine("Variable: " + symbol.Name + ", Type: " + symbol.Type);
                //    log.Debug("Variable: " + symbol.Name + ", Type: " + symbol.Type);
                //    if (symbol.Name.Contains("Cold"))
                //    {
                //        System.Diagnostics.Debug.WriteLine("Variable: " + symbol.Name + ", Type: " + symbol.Type);
                //    }
                //}

                foreach (AdsVariable element in varList)
                {
                    element.Handle = tcClient.CreateVariableHandle(element.VariableName);
                    log.Debug(element.VariableName);
                    if(element.Direction == AdsVariable.DataFlow.TOADS || element.Direction == AdsVariable.DataFlow.TOFROMADS)
                    {
                        switch (element.DataType)
                        {
                            case AdsVariable.VariableType.DT_BOOL:
                                tcClient.AddDeviceNotificationEx(element.VariableName, AdsTransMode.OnChange, 10, 0, element, typeof(Boolean));
                                break;
                            case AdsVariable.VariableType.DT_SINGLE:
                                tcClient.AddDeviceNotificationEx(element.VariableName, AdsTransMode.OnChange, 10, 0, element, typeof(float));
                                break;
                            case AdsVariable.VariableType.DT_DOUBLE:
                                tcClient.AddDeviceNotificationEx(element.VariableName, AdsTransMode.OnChange, 10, 0, element, typeof(Double));
                                break;
                        }
                    }
                }


                log.Info("ADS Server has been connected");

            }
            catch (Exception Err)
            {
                if(tcClient != null)
                {
                    tcClient.Dispose();
                }
                tcClient = null;
                log.Error("Error while connecting ADS Server: " + Err.Message);
            }
        }

        private void adsClient_AdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            //Raise event
            AdsStatusUpdateHandler handler = AdsStatusUpdate;
            if (handler != null)
            {
                handler(this, e);
            }


            log.Debug(e.State.AdsState.ToString());
            if (e.State.AdsState == AdsState.Run && bAdsHasBeenStopped == true)
            {
                tcClient.Dispose();
                tcClient = null;
                ConnectAdsClient(_netID,_srvPort);
            }
            else if(e.State.AdsState == AdsState.Stop)
            {
                bAdsHasBeenStopped = true;
                //MainWindow.AppWindow.StopSequence();
            }
        }

        private void adsClient_AdsNotificationEx(object sender, AdsNotificationExEventArgs e)
        {

            VariableUpdateHandler handler = VariableUpdate;
            if (handler != null)
            {
                AdsVariable var = (AdsVariable)e.UserData;
                if (var.VariableNameEnum == AdsVariableName.COLDVALVE)
                {
                    log.Debug("coldValve has been set to " + e.Value.ToString());
                }

                if (var.VariableNameEnum == AdsVariableName.HOTVALVE)
                {
                    log.Debug("hotValve has been set to " + e.Value.ToString());
                }
                if (var.VariableNameEnum == AdsVariableName.MOTORRPM)
                {
                    log.Debug("current motorspeed" + e.Value.ToString());
                }
                if (var.VariableNameEnum == AdsVariableName.LOOPCOUNTER)
                {
                    log.Debug("loopCounter" + e.Value.ToString());
                }
                handler(this, var, e);


                
            }

        }

    }
}
