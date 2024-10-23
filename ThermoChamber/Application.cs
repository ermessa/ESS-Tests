using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime;
using System.ComponentModel.Design;

namespace RemoteTempChamber
{
    internal class Application
    {
        static void Main()
        {
            while(true) 
            {
                Application test = new Application();
                test.menu();
            }
        }
        public void menu()
        {
            Console.WriteLine("Application Init\n");
            string ip = "192.168.1.200";
            int port = 8888;

            ChamberControllerIoUsingTcpSocket chamber = new ChamberControllerIoUsingTcpSocket();
            
            Socket soc = chamber.socket();

            eResult result = chamber.socConnect(ip, port, soc);
            Console.WriteLine(result);
            
            int option = 0;
            do
            {

                Console.Write("CONTROL COMMANDS:\n1 - Run Manual Mode\n2 - Run Program Mode\n3 - Hold\n4 - Stop\n" +
                    "5 - Resume Run Mode\n\nPROGRAM STATUS AND EDIT FROM HOLD COMMANDS\n6 - Auxiliaries Command\n7 - Auxiliaries Query\n" +
                    "8 - Deviation Command\n9 - Deviation Query\n10 - Final Value Command\n" +
                    "11 - Final Value Query\n12 - Interval Number\n13 - Interval Time\n" +
                    "14 - Program Loops Left Command\n15 - Program Loops Left Query\n16 - Number of Loops\n" +
                    "17 - Next Interval\n18 - High Process Alarm Limit Command\n19 - High Process Alarm Limit Query\n" +
                    "20 - Low Process Alarm Limit Command\n21 - Low Process Alarm Limit Query\n22 - Program Name\n" +
                    "23 - Param Group Command\n24 - Param Group Query\n25 - Program Time\n26 - Program Time Remaining\n" +
                    "27 - Time Left Command\n28 - Time Left Query\n\nPROGRAMMING COMMANDS\n29 - Program Directory\n30 - Interval String Cmd\n" +
                    "31 - Interval String Query\n32 - Program Data String Command\n33 - Program Data String Query\n\nSYSTEM STATUS COMMANDS\n" +
                    "34 - Therm Alarm Analog Input\n35 - Alarm Status\n36 - Process Var Units\n37 - Channel Config Info\n38 - Channel Runtime\n" +
                    "39 - Channel On and Config Status\n40 - Command Status Command\n41 - Command Status Query\n42 - Version Num Control Module\n" +
                    "43 - Name of Channel\n44 - Configured Options\n45 - Active Channel Status\n46 - Digital Input Status\n47 - Digital Output Status\n" +
                    "48 - Door Status\n49 - Reference Channel Datatype\n50 - Channel Datatype\n51 - Last Error Code\n52 - Init Value Parameter\n" +
                    "53 - Language\n54 - Chamber Light Command\n55 - Chamber Light Query\n56 - Access Level Command\n57 - Access Level Query\n" +
                    "58 - Match Interval Command\n59 - Match Interval Query\n60 - Operating Mode\n61 - Program Number\n62 - Refrigeration System Status\n" +
                    "63 - Refrigeration System Pressure\n64 - RTC Command\n65 - RTC Query\n66 - Stop Code\n67 - Service Request Status\n" +
                    "68 - Service Request Event Mask Byte Command\n69 - Service Request Event Mask Byte Query\n70 - Status Word\n71 - Therm Alarm Status Flags\n" +
                    "\nVARIABLE COMMANDS\n72 - Analog Thermal Settings Cmd\n73 - Analog Thermal Settings Qry\n74 - Therm Alarm Analog Input Channel Available\n" +
                    "75 - Live Auxiliaries Settings Command\n76 - Live Auxiliaries Settings Query\n77 - Monitor Channel Value\n78 - Manual Ramp Setting Command\n" +
                    "79 - Manual Ramp Setting Query\n80 - Controller Options Command\n81 - Controller Options Query\n82 - Parameter Values Command\n" +
                    "83 - Parameter Values Query\n84 - Parameter Group Command\n85 - Parameter Group Query\n86 - Process Variable Channel\n" +
                    "87 - Power Fail Recovery Command\n88 - Power Fail Recovery Query\n89 - Set Point Command\n90 - Set Point Query\n91 - Safety Stop Command\n" +
                    "92 - Safety Stop Query\n93 - Therm-Alarm Settings Command\n94 - Therm-Alarm Settings Query\n95 - Therm-Alarm Analog Input Value Query\n" +
                    "96 - Therm-Alarm Temperature Query\n97 - Throttle Reading Query\n98 - Temperature Scale Command\n99 - Temperature Scale Query\n" +
                    "0 - Exit\n\n");
                option = int.Parse(Console.ReadLine());
                Console.Clear();
                Console.WriteLine(option);

                switch(option)
                {
                    case 1: chamber.RunManMode(soc); break;
                    case 2: chamber.RunProgMode(soc, "test", 0, "RESM"); break;
                    case 3: chamber.Hold(soc); break;
                    case 4: chamber.Stop(soc); break;
                    case 5: chamber.ResumeRunMode(soc); break;
                    case 6: chamber.StatusAux_Command(soc, 1, 7); break;
                    case 7: chamber.StatusAux_Query(soc, 1); break;
                    case 8: chamber.Deviation_Command(soc, 1, 4); break;    //testar em modo manual ou programa em hold mode
                    case 9: chamber.Deviation_Query(soc, 1); break;
                    case 10: chamber.FinalValue_Command(soc, 1, 25); break; //testar em modo manual ou programa em hold mode
                    case 11: chamber.FinalValue_Query(soc, 1); break;
                    case 12: chamber.IntervalNumber(soc); break;
                    case 13: chamber.IntervalTime(soc); break;
                    case 14: chamber.ProgLoopsLeft_Command(soc, 3); break;
                    case 15: chamber.ProgLoopsLeft_Query(soc); break;
                    case 16: chamber.NumLoops(soc); break;
                    case 17: chamber.NextInterval(soc); break;
                    case 18: chamber.HighProcessAlarmLimit_Command(soc, 1, 180); break; //não consegui alterar a temperatura na camara
                    case 19: chamber.HighProcessAlarmLimit_Query(soc, 1); break;
                    case 20: chamber.LowProcessAlarmLimit_Command(soc, 1, -80); break;  //não consegui alterar a temperatura na camara
                    case 21: chamber.LowProcessAlarmLimit_Query(soc, 1); break;
                    case 22: chamber.ProgramName(soc); break;
                    case 23: chamber.ParamGroup_Command(soc, 1); break;
                    case 24: chamber.ParamGroup_Query(soc); break;
                    case 25: chamber.ProgTime(soc); break;
                    case 26: chamber.ProgTimeRemaining(soc); break;
                    case 27: chamber.TimeLeft_Command(soc, 400); break;
                    case 28: chamber.TimeLeft_Query(soc); break;
                    case 29: chamber.ProgramDirectory(soc, ""); break;
                    case 30: chamber.IntervalString_Command(soc, 0, 23F, 0F, 0F, 0F, 2F, 0F, 0F, 0F, 300, 1, 3, 15, 3, 0, 1); break;//Illegal program
                    case 31: chamber.IntervalString_Query(soc, 0); break;
                    case 32: chamber.ProgDataString_Command(soc, "test", 0); break;
                    case 33: chamber.ProgDataString_Query(soc, "test"); break;
                    case 34: chamber.ThermAlarmAnalogInput(soc, 1); break;
                    case 35: chamber.AlarmStatus(soc, 1); break;
                    case 36: chamber.ProcessVarUnits(soc, 1); break; 
                    case 37: chamber.ChannelConfigInfo(soc, 1); break;
                    case 38: chamber.ChannelRuntime(soc, 1); break;
                    case 39: chamber.ChannelOnAndConfigSts(soc); break;
                    case 40: chamber.CommandStatus_Command(soc, 1); break;
                    case 41: chamber.CommandStatus_Query(soc); break;
                    case 42: chamber.VersionNumControlModule(soc, 1); break;
                    case 43: chamber.NameOfChannel(soc, 1); break;
                    case 44: chamber.ConfiguredOptions(soc); break;
                    case 45: chamber.ActiveChannelStatus(soc); break;
                    case 46: chamber.DigitalInputStatus(soc); break;
                    case 47: chamber.DigitalOutputStatus(soc, 0, '0'); break;
                    case 48: chamber.DoorStatus(soc); break;
                    case 49: chamber.ReferenceChannelDatatype(soc, 1); break;
                    case 50: chamber.ChannelDatatype(soc, 1); break;
                    case 51: chamber.LastErrorCode(soc); break;
                    case 52: chamber.InitValueParameter(soc, 1); break;
                    case 53: chamber.Language(soc); break;
                    case 54: chamber.ChamberLight_Command(soc, 1); break;
                    case 55: chamber.ChamberLight_Query(soc); break;
                    case 56: chamber.AccessLevel_Command(soc, 4); break;
                    case 57: chamber.AccessLevel_Query(soc); break;
                    case 58: chamber.MatchInterval_Command(soc, 0); break;
                    case 59: chamber.MatchInterval_Query(soc); break;
                    case 60: chamber.OperatingMode(soc); break;
                    case 61: chamber.ProgramNumber(soc); break;
                    case 62: chamber.RefrigerationSystemSts(soc, 1); break;
                    case 63: chamber.RefrigerationSysPressure(soc, 1); break;
                    case 64: chamber.RTC_Command(soc, 6, 29, 10, 50); break; //não testado
                    case 65: chamber.RTC_Query(soc); break;
                    case 66: chamber.StopCode(soc); break;
                    case 67: chamber.ServiceRequestSts(soc); break;
                    case 68: chamber.ServiceRequestEventMaskByte_Command(soc, 255); break;
                    case 69: chamber.ServiceRequestEventMaskByte_Query(soc); break;
                    case 70: chamber.StatusWord(soc); break;
                    case 71: chamber.ThermAlarmStatusFlags(soc); break; //parei aqui
                    case 72: chamber.AnalogThermAlarmSettings_Command(soc, 1, 0, 0F, 100F, "C", 0F, 0, 0, 10, 0, 0); break;
                    case 73: chamber.AnalogThermAlarmSettings_Query(soc, 1); break;
                    case 74: chamber.ThermAlarmAnalogInputChanAvailab(soc, 1); break;
                    case 75: chamber.LiveAuxSettings_Command(soc, 8, 1); break;
                    case 76: chamber.LiveAuxSettings_Query(soc, 8); break;
                    case 77: chamber.MonitorChannelValue(soc, 1); break;
                    case 78: chamber.ManualRampSetting_Command(soc, 1, 30); break;
                    case 79: chamber.ManualRampSetting_Query(soc, 1); break;
                    case 80: chamber.ControllerOptions_Command(soc, 1); break;
                    case 81: chamber.ControllerOptions_Query(soc); break;
                    case 82: chamber.ParameterValues_Command(soc, 1, 1, 999F, 1000, 0.5F, -30.5F); break;
                    case 83: chamber.ParameterValues_Query(soc, 1, 1); break;
                    case 84: chamber.ParameterGroup_Command(soc, 1); break;
                    case 85: chamber.ParameterGroup_Query(soc); break;
                    case 86: chamber.ProcessVariableChannel(soc, 1); break;
                    case 87: chamber.PwrFailRecovery_Command(soc, 1, 150, 0); break;
                    case 88: chamber.PwrFailRecovery_Query(soc); break;
                    case 89: chamber.Setpoint_Command(soc, 1, 26.0F); break;
                    case 90: chamber.Setpoint_Query(soc, 1); break;
                    case 91: chamber.SafetyStop_Command(soc, 1, 20.5F, 100, 3.5F, 1); break;
                    case 92: chamber.SafetyStop_Query(soc); break;
                    case 93: chamber.ThermAlarmSettings_Command(soc, 1, 5, 90, 10, 5.5F, 0, 0); break;
                    case 94: chamber.ThermAlarmSettings_Query(soc, 1); break;
                    case 95: chamber.ThermAlrmAnalogInputVal_Query(soc, 1); break;
                    case 96: chamber.ThermAlrmTemperature_Query(soc, 1); break;
                    case 97: chamber.ThrottleReading_Query(soc, 1); break;
                    case 98: chamber.TemperatureScale_Command(soc, 0); break;
                    case 99: chamber.TemperatureScale_Query(soc); break;

                    case 0: option = 0; System.Environment.Exit(0); break;
                    default: Console.WriteLine("Invalid Option\n"); break;
                }
            }
            while (option != 0);

            System.Environment.Exit(0);
        }
    }
}
