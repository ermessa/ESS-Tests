using System.Text;
using System.Net.Sockets;
using System.Threading.Channels;
using System;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Claims;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using Microsoft.Win32;
using System.Numerics;
using System.Reflection.Metadata;
using System.Data.Common;
using System.Security.Cryptography;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace RemoteTempChamber
{
    enum eResult
    {
        PASS = 0,
        FAIL = 1
    }
    class ChamberControllerIoUsingTcpSocket
    {        
        /// <summary>
        /// This function configure the socket.
        /// </summary>
        /// <returns>socket params</returns>
        public Socket socket()
        {
            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return soc;
        }
        
        /// <summary>
        /// This function connect the socket.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult socConnect(string ip, int port, Socket soc)
        {
            string IP = ip;
            int tcpPortConfig = port;
            Socket socket = soc;

            socket.Connect(IP, tcpPortConfig);

            if (socket.Connected)
            {
                return eResult.PASS;
            }
            else
            {
                return eResult.FAIL;
            }
        }
        
        /// <summary>
        /// This function disconnect the socket.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult socDisconnect(Socket soc)
        {
            Socket socket = soc;

            socket.Disconnect(true);
            
            if (!socket.Connected)
            {
                return eResult.PASS;
            }
            else
            {
                return eResult.FAIL;
            }
        }

        /// <summary>
        /// Buffer Command: Used only when is necessary to do a command to the chamber
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="parameter"></param>
        /// <returns>return a string with the chamber response.</returns>
        private string bufferCommand(Socket socket, string parameter)
        {
            string strValue = "";

            //Create new buffer to hold response from Chamber Controller
            byte[] Received = new byte[128];

            if (socket.Connected)
            {
                byte[] Command = ASCIIEncoding.ASCII.GetBytes(parameter + "\r");

                //Send the Query Command
                socket.Send(Command);
                //Receive response
                int len = socket.Receive(Received);

                //Convert received ASCII bytes to string
                strValue = ASCIIEncoding.ASCII.GetString(Received, 0, len);
                Console.WriteLine("Chamber Response: " + strValue + "\n");
            }
            else
            {
                Console.WriteLine("The socket is disconnected");
                socket.Close();
            }
            return strValue;
        }

        /*------------------- CONTROL COMMANDS -----------------------------------------------------*/
        /// <summary>
        /// Hold: Places a running program or test in hold mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult Hold(Socket soc)
        {
            string param = "HOLD", strReturn = "";

            strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Initialize: This function is for programs written on obsolete controllers and will ensure 
        /// no invalid commands are programmed to the 8800.The 8800 does not perform an 
        /// action when receiving this command. 
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>///PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult Init(Socket soc)
        {
            string param = "IDEN?", strReturn = "";
            byte[] Command = ASCIIEncoding.ASCII.GetBytes(param+"\r");

            strReturn = bufferCommand(soc, param);

            Console.WriteLine("Chamber ID: " + strReturn + "\n");

            if (strReturn == "8800 Chamber Controller\r")
            {
                Console.WriteLine("The 8800 does not perform an action when receiving this command.");
                return eResult.FAIL; //confirmar se para esse caso é uma falha
            }
            else
            {
                param = "INIT";
                
                strReturn = bufferCommand(soc, param);
                Console.WriteLine("Initialize Controller Sent to Chamber\n");
                return eResult.PASS;
            }
        }

        /// <summary>
        /// Resume Run Mode: Returns a program or test from hold mode to its run mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ResumeRunMode(Socket soc)
        {
            string param = "RESM", strReturn = "";

            strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }   
        
        /// <summary>
        /// Run Manual Mode:This Function Places a stopped 8800 in run manual mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult RunManMode(Socket soc)
        {
            string param = "RUNM", strReturn = "";

            strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Run Program Mode: This function places a stopped 8800 in run program mode, and specifies 
        /// the program and starting interval
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="progName">The program must be in the root directory of controller hard drive.</param>
        /// <param name="intervalNum">Interval Number (0 - 600)</param>
        /// <param name="mode">Single-step Mode: S, Continuous Execution: RESM</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult RunProgMode(Socket soc, string progName, int intervalNum, string mode)
        {
            string param = "RUNP", strValue = "";
            int verif = 0;

            byte[] Received = new byte[128];

            if (soc.Connected)
            {
                if ((intervalNum < 0 || intervalNum > 600) || (mode != "S" || mode != "RESM"))
                {
                    return eResult.FAIL;
                }

                if ((intervalNum >=0 && intervalNum <=600) && (mode == "S" || mode == "RESM"))
                {
                    byte[] Command = ASCIIEncoding.ASCII.GetBytes(param + progName + "," + intervalNum + "," + mode + "\r");
                    
                    soc.Send(Command);
                    
                    int len = soc.Receive(Received);

                    strValue = ASCIIEncoding.ASCII.GetString(Received, 0, len);
                    Console.WriteLine("Chamber Response: " + strValue + "\n");

                    if (strValue == "0\r")
                    {
                        verif = 0;
                    }
                    else
                    {
                        verif = 1;                        
                    }
                }
            }
            else
            {
                Console.WriteLine("The socket is disconnected");
                soc.Close();
                return eResult.FAIL;
            }

            if (verif == 1)
            {
                return eResult.FAIL;
            }
            else
            {
                return eResult.PASS;
            }
        }

        /// <summary>
        /// Reset Screen: Forces the 8800 to go to the main screen.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ResetScreen(Socket soc)
        {
            ///Forces the 8800 to go to the main screen.
            string param = "SCRR", strReturn = "";

            strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Stop: Places the 8800 in stop mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult Stop(Socket soc)
        {
            string param = "STOP", strReturn = "";

            strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Watchdog: provides an extra level of protection for users operating the chamber. The watchdog
        /// operates as a “everything is OK” command. The syntax for the command is WDOGn, where n is a
        ///number of seconds between 0 and 600.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="seconds">0:Disable Watchdog Timer, 1 - 600: enabled in seconds</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult WatchDog(Socket soc, int seconds)
        {
            string param = "WDOG", strReturn = "";

            if (soc.Connected)
            {
                Console.WriteLine("Determine Watchdog Time in Seconds:\nNOTE: Min. Value = 0, Max. Value = 600");
                
                param = param + seconds;

                strReturn = bufferCommand(soc, param);
            }
            else
            {
                return eResult.FAIL;
            }

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /*------------------- PROGRAM STATUS & EDIT FROM HOLD COMMANDS -----------------------------*/

        /// <summary>
        /// Auxiliar Status Command: Allows to change the auxiliary states for run manual mode operations
        /// and / or edit from hold operations.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="auxGroup">auxiliar group value: 1 - 2</param>
        /// <param name="auxValue">The sum of auxiliaries values to be turned on.
        /// Sample: 097 (97 = 64 + 32 + 1), indicates that AUX 7(64), AUX 6(32) and AUX 1(1) are going to turn on.</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult StatusAux_Command(Socket soc, int auxGroup, int auxValue)
        {
            /*Group 1              Group 2
             *Value    Aux         Value    Aux
             *1        AUX 1       1        AUX 9
             *2        AUX 2       2        AUX 10
             *4        AUX 3       4        AUX 11
             *8        AUX 4       8        AUX 12
             *16       AUX 5       16       AUX 13
             *32       AUX 6       32       AUX 14
             *64       AUX 7       64       AUX 15
             *128      AUX 8       128      AUX 16 
             */
            string param = "AUXE";
            
            if ((auxValue < 0 || auxValue > 128) || (auxGroup < 1 || auxGroup > 2))
            {
                return eResult.FAIL;
            }
            else
            {
                param = param + auxGroup + "," + auxValue;
                string strReturn = bufferCommand(soc, param);
                return "0\r" == strReturn?eResult.PASS:eResult.FAIL;
            }    
        }

        /// <summary>
        /// Auxiliar Status Query: Allows to read the on and off states of the auxiliary groups.
        ///Auxiliary group 1 refers to auxiliaries 1 - 8 and auxiliary group 2 refers to auxiliaries 9 - 16.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="auxGroup">auxiliar group value: 1 - 2</param>
        /// <returns>Auxiliaries Values. Sample: 097 (97 = 64 + 32 + 1), indicates that AUX 7 (64),
        /// AUX 6 (32), and AUX 1 (1) are on.</returns>
        public int StatusAux_Query(Socket soc, int auxGroup)
        {
            /*Group 1              Group 2
             *Value    Aux         Value    Aux
             *1        AUX 1       1        AUX 9
             *2        AUX 2       2        AUX 10
             *4        AUX 3       4        AUX 11
             *8        AUX 4       8        AUX 12
             *16       AUX 5       16       AUX 13
             *32       AUX 6       32       AUX 14
             *64       AUX 7       64       AUX 15
             *128      AUX 8       128      AUX 16 
             */
            string param = "AUXE";

            if (auxGroup < 1 || auxGroup > 2)
            {
                return -1;
            }
            else
            {
                param = param + auxGroup + "?";

                string strReturn = bufferCommand(soc, param);
                return int.Parse(strReturn);
            }            
        }

        /// <summary>
        /// Deviation Command: The command loads a deviation setting into the chamber
        /// for the current manual mode operation, or sends a temporary deviation value
        /// during an edit from hold operation.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <param name="deviation">temperature deviation</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult Deviation_Command(Socket soc, int channel, float deviation)
        {
            if (channel < 1 || channel > 4)
            {
                return eResult.FAIL;
            }
            else
            {
                string param = "DEVN";
                param += channel + "," + deviation;
                string strReturn = bufferCommand(soc, param);
                return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
            }
        }

        /// <summary>
        /// Deviation Query: The query command asks the 8800 for the current deviation
        /// reading from a selected channel.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <returns>float value corresponding to deviation.</returns>
        public float Deviation_Query(Socket soc, int channel)
        {
            if (channel < 1 || channel > 4)
            {
                return -1F;
            }
            else
            {
                string param = "DEVN";
                param += "?";
                string strReturn = bufferCommand(soc, param);
                return float.Parse(strReturn);
            }
        }

        /// <summary>
        /// Status Final Value Command: The edit from hold operation command temporarily changes the current interval’s final value.
        /// NOTE: When not running a program, the command returns the manual mode new setpoint.
        /// </summary> 
        /// <param name="soc"></param>
        /// <param name="channel">value: 1-4</param>
        /// <param name="value">setpoint temperature</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult FinalValue_Command(Socket soc, int channel, float value)
        {
            //only possible edit value if the chamber is in hold mode
            string param = "FVAL";
            if (channel < 1 || channel > 4)
            {
                return eResult.FAIL;
            }
            else
            {
                param += channel + "," + value;

                string strReturn = bufferCommand(soc, param);
                return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
            }
        }

        /// <summary>
        /// Final Value Query: The query command asks the 8800 for the current interval’s final value for channel n
        /// (1 through 4). The 8800 sends a decimal value for the selected channel.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">value: 1-4</param>
        /// <returns>Float Final Value</returns>
        public float FinalValue_Query(Socket soc, int channel)
        {
            string param = "FVAL";
            if (channel < 1 || channel > 4)
            {
                return -1F;
            }
            else
            {
                param = param + channel + "?";
                string strReturn = bufferCommand(soc, param);
                return float.Parse(strReturn);
            }
        }

        /// <summary>
        /// Interval Number: Queries the chamber for the current interval number.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>Integer value to indicate the interval number</returns>
        public int IntervalNumber(Socket soc)
        {
            string param = "INTN";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Status Interval Time: Queries the 8800 for the programmed time for the current interval.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>value corresponding to current interval time</returns>
        public string IntervalTime(Socket soc)
        {
            string param = "ITIM";

            param += "?";
            string strReturn = bufferCommand(soc, param);
            
            return strReturn;
        }
        
        /// <summary>
        /// Program Loops Left Command: The edit from hold operation command temporarily
        /// changes the current interval’s loop counter.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="numLoops">number of loops</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ProgLoopsLeft_Command(Socket soc, int numLoops)
        {
            string param = "LLFT";
            param += numLoops;
            string strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Program Loops Left Query: The query command asks the chamber for the number of loops
        /// left to be executed for the current loop. On nested looping, the value is for the inside loop.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>Returns an integer to indicate the number of loops left.</returns>
        public int ProgLoopsLeft_Query(Socket soc)
        {
            string param = "LLFT";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Number of Loops: Queries the programmed number of loops assigned to the current loop.
        /// For nested looping, the value is for the inside loop.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>returns an integer indicating the number of loops assigned to the current loop.</returns>
        public int NumLoops(Socket soc)
        {
            string param = "NUML";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Next Interval: Queries the chamber for the next interval that will be executed.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>integer corresponding to the next interval number</returns>
        public int NextInterval(Socket soc)
        {
            string param = "NXTI";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// High Process Alarm Limit Command: The operation command sends a new high process alarm
        /// limit to the chamber. USE THE OPERATION COMMAND WITH CAUTION!
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <param name="alarmVal">float val</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult HighProcessAlarmLimit_Command(Socket soc,int channel, int alarmVal)
        {
            if (channel < 1 || channel > 4)
            {
                return eResult.FAIL;
            }
            else
            {
                string param = "PALH";
                param += "," + channel + "," + alarmVal; //testar o comando
                string strReturn = bufferCommand(soc, param);

                return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
            }
        }

        /// <summary>
        /// High Process Alarm Limit Query: Queries the chamber for a channel’s high process alarm limit.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel"></param>
        /// <returns>high process alarm limit value</returns>
        public float HighProcessAlarmLimit_Query(Socket soc, int channel)
        {
            if (channel < 1 || channel > 4)
            {
                return -1;
            }
            else
            {
                string param = "PALH";
                param += channel + "?";
                string strReturn = bufferCommand(soc, param);
                return float.Parse(strReturn);
            }
        }

        /// <summary>
        /// Low Process Alarm Limit Command: The function sends a new low process alarm limit
        /// to the chamber.USE THE OPERATION COMMAND WITH CAUTION!
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <param name="alarmVal">float val</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult LowProcessAlarmLimit_Command( Socket soc,int channel, int alarmVal)
        {
            if (channel < 1 || channel > 4)
            {
                return eResult.FAIL;
            }
            else
            {
                string param = "PALL";
                param += channel + "," + alarmVal;
                string strReturn = bufferCommand(soc, param);

                return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
            }
        }

        /// <summary>
        /// Low Process Alarm Limit Query: Queries the chamber for a channel’s low process alarm limit.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <returns>low process alarm limit value</returns>
        public float LowProcessAlarmLimit_Query( Socket soc, int channel)
        {
            if (channel < 1 || channel > 4)
            {
                return -1;
            }
            else
            {
                string param = "PALL";
                param += channel + "?";
                string strReturn = bufferCommand(soc, param);
                return float.Parse(strReturn);
            }
        }
        
        /// <summary>
        /// Program Name: Queries the 8800 for the name of the currently loaded program.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>string with the program’s assigned name(up to 15 char long).
        /// "Untitled" case there is no currently loaded program name available</returns>
        public string ProgramName(Socket soc)
        {
            string param = "PNAM";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Parameter Group Command: If the chamber is in manual mode, the operation command selects
        /// the parameter group(1 through 4) that will use to control the channels. Edit from hold 
        /// operations temporarily change the parameter group for the program.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="group">1-4</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ParamGroup_Command(Socket soc, int group)
        {
            string param = "PRMG";
            param += group;
            string strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Parameter Group Query: Queries the Chamber for the number of the parameter group
        /// that it is currently using.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>int value correspondig to parameter group.</returns>
        public int ParamGroup_Query(Socket soc)
        {
            string param = "PRMG";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Program Time:Queries the 8800 for the total estimated time for the current program.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>string correponding to total estimated time for the current program</returns>
        public string ProgTime(Socket soc)
        {
            string param = "PTIM";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Program Time Remaining: Queries the chamber for the estimated time left in the current program.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>time remaining value.</returns>
        public string ProgTimeRemaining(Socket soc)
        {
            string param = "PTLF";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Time Left Command: edit from hold operation command temporarily changes 
        /// the current interval’s time left counter.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="timeSec">time in seconds</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult TimeLeft_Command(Socket soc, int timeSec)
        {
            string param = "TLFT";
            param += "::" + timeSec;
            string strReturn = bufferCommand(soc, param);
            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Time Left Query: Queries the 8800 for the time left in the current interval.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>value corresponding to the time.</returns>
        public string TimeLeft_Query(Socket soc)
        {
            string param = "TLFT";
            param += "?";
            string strReturn = bufferCommand(soc,param);
            return strReturn;
        }

        /*------------------- PROGRAMMING COMMANDS -------------------------------------------------*/

        /// <summary>
        /// Program Directory: This command queries a specific directory on the 8800 for a list of
        /// program files and directories.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="path">Insert the Path on the Controller or empty for the root path</param>
        /// <returns>return the name of the program or directory followed by a number.
        /// (directory = -2, )</returns>
        public string ProgramDirectory(Socket soc, string path)
        {
            string param = "DIRP";
            param += "\\" + path + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;   //repeat this command until receive the message: No More Files, -1
        }

        /// <summary>
        /// Interval String Command:  Sends an interval string to initialize the program (INTV0)
        /// or one of the program intervals(INTVn)
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="interval">Interval Value</param>
        /// <param name="fv1">Decimal Final Value for Channel 1</param>
        /// <param name="fv2">Decimal Final Value for Channel 2</param>
        /// <param name="fv3">Decimal Final Value for Channel 3</param>
        /// <param name="fv4">Decimal Final Value for Channel 4</param>
        /// <param name="dv1">Decimal Deviation Value for Channel 1</param>
        /// <param name="dv2">Decimal Deviation Value for Channel 2</param>
        /// <param name="dv3">Decimal Deviation Value for Channel 3</param>
        /// <param name="dv4">Decimal Deviation Value for Channel 4</param>
        /// <param name="timeSecInterval">Time in Seconds for Interval</param>
        /// <param name="paramGroup">1-4</param>
        /// <param name="numLoops">Number of Loops</param>
        /// <param name="nextInterval">Interval Value</param>
        /// <param name="aux1">Auxiliar Enable Group 1 (1, 2, 4, 8, 16, 32, 64, 128)</param>
        /// <param name="aux2">Auxiliar Enable Group 2 (1, 2, 4, 8, 16, 32, 64, 128)</param>
        /// <param name="optionControl">1:Product temperature control, 2:Humidity system
        /// (NOTE: NEVER assign a PTC channel with a humidity channel.), 4:Low humidity system,
        /// 8:GSoak, 16:Purge, 32:Cascade refrigeration (SE-Series chambers only),
        /// 64:Power Save mode (if SE-Series; otherwise sets cascade mode),
        /// 128:Single-stage refrigeration (SE-Series only), 512:Altitude, 1024:Mechanical boost,
        /// 32768:Product dew point control</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult IntervalString_Command(Socket soc, int interval, float fv1, float fv2, float fv3, float fv4,
            float dv1, float dv2, float dv3, float dv4,int timeSecInterval, int paramGroup, int numLoops,
            int nextInterval, int aux1, int aux2, int optionControl)
        {
            string param = "INTV";
            param += interval + "," + fv1 + "," + fv2 + "," + fv3 + "," + fv4 + "," + dv1 + "," +
                dv2 + "," + dv3 + "," + dv4 + "," + timeSecInterval + "," + paramGroup + "," + numLoops +
                "," + nextInterval + "," + aux1 + "," + aux2 + ",," + optionControl;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Interval String Query: The query command asks for the interval string that initializes
        /// the program (INTV0) or for one of the program intervals(INTVn).
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="interval">Interval value</param>
        /// <returns>interval, final value ch.1, final value ch.2, final value ch.3, final value ch.4, 
        /// active channels value</returns>
        public string IntervalString_Query(Socket soc, int interval)
        {
            string param = "INTV";
            param += interval + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Program Data String Command: The function sets up the name and the number
        /// of intervals in the program.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="programName"></param>
        /// <param name="intervals"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ProgDataString_Command(Socket soc, string programName, int intervals)
        {
            string param = "PROG";
            param += "," + programName + "," + intervals;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Program Data String Query: The query command receives which program will be retrieved
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="programName">program name</param>
        /// <returns>name of program and number of intervals</returns>
        public string ProgDataString_Query(Socket soc, string programName)
        {
            string param = "PROG";
            param += programName + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /*------------------- SYSTEM STATUS COMMANDS -----------------------------------------------*/
        /* FUNCTIONS NOT IMPLEMENTED: IDEN, PMEM, SERL, VRSN */

        /// <summary>
        /// Therm-Alarm Analog Input: Send Therm-Alarm analog input units value.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="thermAlarmNum">1-4</param>
        /// <returns>returns 1-3 characters for the value. Common unit codes are % and T (torr).</returns>
        public string ThermAlarmAnalogInput(Socket soc, int thermAlarmNum)
        {
            string param = "AACH";
            param += thermAlarmNum + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Alarm Status: Send alarm status
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channelAlarmNum">0-7</param>
        /// <returns>returns the current alarm status for the selected channel.</returns>
        public int AlarmStatus(Socket soc, int channelAlarmNum)
        {
            /*
             *BIT #    Value Active    Definition
             *0        1               Low Deviation Alarm
             *1        2               High Deviation Alarm
             *2        4               Not Used
             *3        8               Not Used
             *4        16              Low Process Alarm
             *5        32              High Process Alarm
             *6        64              Not Used
             *7        128             Not Used
             *NOTE: Only one bit can be set at a time. 
             */
            string param = "ALRM";
            param += channelAlarmNum + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Process Variable Units: Send Process Variable Units
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="processVarChannel">process var channel (1-8), monitor channel (101-140)</param>
        /// <returns>Celsius, Fahrenheit, percent relative humidity, and torr.
        /// NOTE: If the channel has no units set or is not configured, the chamber will return a blank character.</returns>
        public string ProcessVarUnits(Socket soc, int processVarChannel)
        {
            string param = "CCHR";
            param += processVarChannel + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Channel Configuration Info: Send process channel configuration information
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channelNum">variable channel (1-8), monitor channel (101-140).</param>
        /// <returns>0:Channel not used, 1:Percent relative humidity channel, 2:Temperature channel,
        /// 3:Linear channel, 4:Linear 0 % to 100 % relative humidity channel, 5:Product temperature control channel</returns>
        public int ChannelConfigInfo(Socket soc, int channelNum) 
        {
            /*The 8800 sends a single coded integer describing the channel type.
             *0    Channel not used
             *1    Percent relative humidity channel using a wet bulb / dry bulb thermocouple pair
             *2    Temperature channel using a thermocouple
             *3    Linear channel using a programmable range(for example altitude)
             *4    Linear 0 % to 100 % relative humidity channel using a solid-state sensor
             *5    Product temperature control channel 
             */
            string param = "CCNF";
            param += channelNum + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Channel Run Time Query: Queries the chamber for a channel’s accumulated run time.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channelNum">1-8</param>
        /// <returns>number of hours that the channel has been running.</returns>
        public int ChannelRuntime(Socket soc, int channelNum)
        {
            string param = "CHRT";
            param += channelNum + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Channel On and Configuration Status: Send channel on and configured status
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>two-byte coded integer describing the channel on and configuration status.</returns>
        public int ChannelOnAndConfigSts(Socket soc)
        {
            /*
             *Byte 1 = channel on status: Bits 0 through 7 indicate the on status of channels 1 through 8 respectively.
             *Byte 2 = channel configured status: Bits 8 through 15 indicate the configured status of channels 1 through 8 respectively.
             *The 8800 sets the bit for each channel that is configured.
            */
            string param = "CHST";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Command Status Command: The operation command changes the current comm status.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="ackComm">0: Turns Off Acknowledge. 1: Turns On Acknowledge.</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult CommandStatus_Command(Socket soc, int ackComm)
        {
            if (ackComm < 0 || ackComm > 1)
            {
                return eResult.FAIL;
            }
            string param = "CMST";
            param += ackComm;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Command Status Query: This query command asks the 8800 for the current comm status.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0: Send Acknowledge is Off. 1: Send Acknowledge is On.</returns>
        public int CommandStatus_Query(Socket soc)
        {
            string param = "CMST";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Version Number of Control Module: Query the version of control module.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="cm">1-4</param>
        /// <returns>Control module version and Date</returns>
        public string VersionNumControlModule(Socket soc, int cm)
        {
            string param = "CMVR";
            param += cm + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Name of Channel: This command allows you read the assigned name of any process variable
        /// or monitor channel.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-12: Process variable channels. 13-52, 101-140: Monitor Channels.</param>
        /// <returns>Name of Channel.</returns>
        public string NameOfChannel(Socket soc, int channel)
        {
            string param = "CNAM";
            param += channel + "?";
            string strResult = bufferCommand(soc, param);
            return strResult;
        }

        /// <summary>
        /// Configured Options: Query the system options selected at the factory.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>system options status</returns>
        public int ConfiguredOptions(Socket soc)
        {
            ///Byte 1 (0 through 7)
            ///0    Product temperature control
            ///1    Humidity system
            ///2    Low humidity system
            ///3    -10ºC dew point
            ///4    Purge
            ///5    Cascade Refrigeration system
            ///6    Power save mode
            ///7    Not used

            ///Byte 2 (8 through 15)
            ///8    Transducers Installed
            ///9    Check water level
            ///10   Go to stop on therm-alarm trip
            ///11   Disable TB8/9/11 on refrigeration trip
            ///12~15 not used

            ///Byte 3 (16 through 23)
            ///16   Custom chamber control
            ///17   SPD SE chamber control
            ///18   System monitor functions
            ///19   Go to stop mode on System monitor trips
            ///20~23 not used
            string param = "CONF";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Active Channel Status: Query the active channels
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0-15. 1:ch1, 2:ch2, 4:ch3, 8:ch4.</returns>
        public int ActiveChannelStatus(Socket soc)
        {
            string param = "DCHN";
            param += "?";
            string strReturn = bufferCommand(soc,param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Digital Input Status: Query Digital Input Status
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>DI 1-6 of Control Modules 1-4 active values.</returns>
        public int DigitalInputStatus(Socket soc)
        {
            ///0 through 5 CM0 digital inputs 1 through 6
            ///6 through 11 CM1 digital inputs 1 through 6
            ///12 through 17 CM2 digital inputs 1 through 6
            ///18 through 23 CM3 digital inputs 1 through 6

            ///DI 6     DI 5    DI 4    DI 3    DI 2    DI 1    CM
            ///bit 5    bit 4   bit 3   bit 2   bit 1   bit 0   0
            ///bit 11   bit 10  bit 9   bit 8   bit 7   bit 6   1
            ///bit 17   bit 16  bit 15  bit 14  bit 13  bit 12  2
            ///bit 23   bit 22  bit 21  bit 20  bit 19  bit 18  3
            
            string param = "DIGI";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Digital Output Status: Query Digital Output Status
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="cm">Control Module (0-3)</param>
        /// <param name="extended">extended: Use 'e' to return the status of DO 25-40.</param>
        /// <returns>Returns a integer value that indicates which of DO 1-24 are active,
        /// or a value that indicates which of DO 25-40 are active.</returns>
        public int DigitalOutputStatus(Socket soc, int cm, char extended)
        {
            ///Standard digital outputs:
            ///DO 6     DO 5    DO 4    DO 3    DO 2    DO 1
            ///bit 5    bit 4   bit 3   bit 2   bit 1   bit 0

            ///DO 12    DO 11   DO 10   DO 9    DO 8    DO 7
            ///bit 11   bit 10  bit 9   bit 8   bit 7   bit 6

            ///DO 18    DO 17   DO 16   DO 15   DO 14   DO 13
            ///bit 17   bit 16  bit 15  bit 14  bit 13  bit 12

            ///DO 24    DO 23   DO 22   DO 21   DO 20   DO 19
            ///bit 23   bit 22  bit 21  bit 20  bit 19  bit 18

            ///Extended digital outputs:
            ///DO 28    DO 27   DO 26   DO 25
            ///bit 27   bit 26  bit 25  bit 24

            ///DO 32    DO 31   DO 30   DO 29
            ///bit 31   bit 30  bit 29  bit 28

            ///DO 36    DO 35   DO 34   DO 33
            ///bit 35   bit 34  bit 33  bit 32

            ///DO 40    DO 39   DO 38   DO 37
            ///bit 39   bit 38  bit 37  bit 36

            string param = "DIGO";
            if (extended == 'e')
            {
                param += cm + "," + extended + "?";
            }
            else
            {
                param += cm + "?";
            }
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Door Status
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>1 if is currently closed. 0 if the door is open or there is no digital
        /// input defined for it.</returns>
        public int DoorStatus(Socket soc)
        {
            string param = "DOOR";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Reference Channel Data Type
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-8: Process variable channels, 13-52: Monitor channels (1-40)</param>
        /// <returns>0-9</returns>
        public int ReferenceChannelDatatype(Socket soc, int channel)
        {
            //0 Unused
            //1 Temperature
            //2 RH wet bulb/dry bulb
            //3 RH linear
            //4 RH temperature compensated
            //5 Linear
            //6 Altitude (kilofeet)
            //7 Vibration
            //8 Scaled linear
            //9 Altitude linear (torr)
            string param = "DREF";
            param += channel + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Channel Data Type
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-8: Process variable channels, 13-52 or
        /// 101-140: Monitor Channels(1-40)</param>
        /// <returns>0-15</returns>
        public int ChannelDatatype(Socket soc, int channel)
        {
            //0     Unused
            //1     Temperature
            //2     RH wet bulb/dry bulb
            //3     RH linear
            //4     RH temperature compensated
            //5     Linear
            //6     Altitude (kilofeet)
            //7     Vibration
            //8     Scaled linear
            //9     Altitude linear (torr)
            //10    Temperature Average
            //11    Humidity ratio
            //12    Dew point
            //13    Transducer
            //14    Temperature Minimum
            //15    Temperature Maximum
            string param = "DTYP";
            param += channel + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Last Error Code: The error code buffer holds the last eight errors. 
        /// You can use this command repeatedly to read the entire buffer in a first in–last out format.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>The error code buffer holds the last eight errors. When returns an error code of 0,
        /// the error buffer is empty</returns>
        public int LastErrorCode(Socket soc)
        {
            string param = "IERR";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Initial Value Parameter: Queries the 8800 for the current interval’s initial value
        /// parameter for channel n(1-4)
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-4</param>
        /// <returns>The chamber sends a decimal value for the selected channel.</returns>
        public float InitValueParameter(Socket soc, int channel)
        {
            string param = "IVAL";
            param += channel + "?";
            string strReturn = bufferCommand(soc, param);
            return float.Parse(strReturn);
        }

        /// <summary>
        /// Language mode: This command queries the chamber for the current language mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0: English, 1: French</returns>
        public int Language(Socket soc)
        {
            string param = "LANG";
            param += "?";
            string strReturn = bufferCommand(soc,param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Chamber Light Command: The command allows you to remotely turn the chamber 
        /// light on or off.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="light">0: Turn Off, 1: Turn On</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ChamberLight_Command(Socket soc, int light)
        {
            string param = "LGHT";
            param += light;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Chamber Light Query: The query command asks the chamber for the status of the
        /// light (on or off).
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0: Light is off, 1: Light is on</returns>
        public int ChamberLight_Query(Socket soc)
        {
            string param = "LGHT";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Access Level Command: Send the access level desired to the control chamber.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="level">0:Locked, 1:Lvl.1, 2:Lvl.2, 3:Programmer, 4:Lab Manager, 5:Calibration</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult AccessLevel_Command(Socket soc, int level)
        {
            string param = "LOCK";
            param += level;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Access Level Query: Read the access level of control chamber.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0:Locked, 1:Lvl.1, 2:Lvl.2, 3:Programmer, 4:Lab Manager, 5:Calibration</returns>
        public int AccessLevel_Query(Socket soc)
        {
            string param = "LOCK";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Match Interval Command: Is used to trigger the interval match interrupt event for a service
        /// request.The interval match interrupt event occurs at the beginning of the previously
        /// loaded match interval.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="interval">0-300. 0:Event will occur in every interval</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult MatchInterval_Command(Socket soc, int interval)
        {
            string param = "MINT";
            param += interval;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }
        
        /// <summary>
        /// Match Interval Query: Is used to trigger the interval match interrupt event for a service
        /// request.The interval match interrupt event occurs at the beginning of the previously
        /// loaded match interval.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>0-300. 0:Event will occur in every interval</returns>
        public int MatchInterval_Query(Socket soc)
        {
            string param = "MINT";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Operating Mode: The query command asks the chamber controller for its current operating mode.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>coded integer byte</returns>
        public int OperatingMode(Socket soc)
        {
            /*
             * Bit  Mode
             * 0    Program Mode
             * 1    Edit Mode (with controller in stop mode)
             * 2    View Program Mode
             * 3    Edit Mode (with controller in hold mode)
             * 4    Manual Mode
             * 5    Delayed Start Mode
             * 6    Unused
             * 7    Calibration Mode
             */
            string param = "MODE";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse (strReturn);
        }

        /// <summary>
        /// Program Number: LEGACY SUPPORT! Queries the 8800 for the number of the currently
        /// loaded program.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>integer referred to slot. 0:None.</returns>
        public int ProgramNumber(Socket soc)
        {
            string param = "PRGN";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }
        
        /// <summary>
        /// Refrigeration System Status: This function shows the erfrigeration data.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="sysNum">Refrigeration System Number 1-4</param>
        /// <returns>status for the refrigeration data</returns>
        public int RefrigerationSystemSts(Socket soc, int sysNum)
        {
            /*
             * Bits     Meaning
             * 0&1      Transducer #4 Status
             * 2&3      Transducer #3 Status
             * 4&5      Transducer #2 Status
             * 6&7      Transducer #1 Status
             * 8-11     High-Stage Compressor Status
             * 12-15    Low-Stage Compressor Status
             * 16&17    Refrig. Temp. Status
             */
            string param = "RDAT";
            param += sysNum + "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Refrigeration System Pressures
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="sysNum">1-4. If 0 or null, returns system 1 by default.</param>
        /// <returns>high stage succion pressure, high stage discharge pressure, low stage succion pressure,
        /// low stage discharge pressure, high stage suction temperature, high stage discharge temperature, 
        /// low stage suction temperature, low stage discharge temperature, refrigeration mode</returns>
        public string RefrigerationSysPressure(Socket soc, int sysNum)
        {
            /*
             * String breaks down as follows:
             * - Four pressures for high-stage suction, high-stage discharge, low-stage suction,
             * and low-stage discharge.
             * - Four temperatures for high-stage suction, high-stage discharge, low-stage suction,
             * and low-stage discharge.
             * - One coded integer that indicates the refrigeration mode. Each mode has its own
             * weighting value:
             * 1    Humidity Cooling Mode
             * 2    Temperature Cooling Mode
             * 4    Cascading Mode
             * 8    Pump-Down Mode
             * 16   High-Stage Compressor Trip
             * 32   Low-Stage Compressor Trip
             */
            string param = "REFG";
            if (sysNum > 0 && sysNum < 5)
            {
                param += sysNum + "?";
            }
            else
            {
                param += "?";
            }
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Real Time Clock Command: The operation command loads new values into the real time
        /// clock and resets seconds to 00.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="month">Month: 1-12</param>
        /// <param name="day">Day: 1-31</param>
        /// <param name="hour">Hour: 0-24</param>
        /// <param name="minutes">Minutes: 0-60</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult RTC_Command(Socket soc, int month, int day, int hour, int minutes)
        {
            string param = "RLTM";
            if (month < 1 || month > 12) return eResult.FAIL;
            if (day < 1 || day > 31) return eResult.FAIL;
            if (hour < 0 || hour > 24) return eResult.FAIL;
            if (minutes < 0 || minutes > 60) return eResult.FAIL;
            
            param += month + "," + day + "," + hour + "," + minutes;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Real Time Clock: The query command tells the controller to return the date and time
        /// reading from its real time clock.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>Date and Time</returns>
        public string RTC_Query(Socket soc)
        {
            string param = "RLTM";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Stop Code: The stop code identifies the cause of the most recent transition to the stop state.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>int value according to stop code</returns>
        public int StopCode(Socket soc)
        {
            /*
             * 0    Cold boot power up. The 8800 memory has been initialized.
             * 1    Currently running. Not in stop.
             * 2    Stop key pressed.
             * 3    End of test.
             * 4    External input. An input defined as stop has been activated.
             * 5    Computer interface. The 8800 received the stop command.
             * 6    Open input. A thermocouple or analog input is open.
             * 7    Process alarm. A process alarm setting has been exceeded.
             * 8    System Monitor trip.
             * 9    Power fail recovery. Selected power fail recover mode was set to stop the 8800.
             * 10   Therm-Alarm trip. A Therm-Alarm limit was exceeded.
             * 12   Compressor Fault. Contact Thermotron Product Support.
             * 18   Invalid Program. A program failed to run because it contains invalid data.
             * 24   AST Module Lost. The 8800 is no longer communicating with the AST Module.
             * 26   Open Door. The 8800 has detected an open door.
             * 27   Humidity Sensors Diverging. The dual humidity sensors have drifted too far apart.
             * 28   Humidity Sensor Failure. Both humidity sensors have failed.
             * 30   Deviation Alarm. A deviation alarm limit has been exceeded.
             * 31   Remote Setpoint. The Remote Setpoint input has changed from active to inactive.
             * 32   Humidity Sensor Open. One of two humidity sensors are indicating an open input.
             * 35   Controlled Stop. The controller has completed the Controlled Stop sequence.
             * 36   Air pressure exceeded. The portable Shaker air pressure has exceeded its limit.
             */
            string param = "SCOD";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Service Request status Byte: The 8800 returns the same data that a GPIB serial poll
        /// would return. The events, which set the associated bits in the response data, must be
        /// enabled in the SRQ mask and are loaded using the SRQM command.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>coded int</returns>
        public int ServiceRequestSts(Socket soc)
        {
            /* Bit  Definition
             * 0    Change in state
             * 1    Change in alarm status
             * 2    End of interval
             * 3    Match interval
             * 4    End of program
             * 5    Error
             * 6    Reserved by GPIB (RSV)
             * 7    Power on reset
             */
            string param = "SRQB";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Service Request Event Mask Byte Query: This byte enables the various events for requesting
        /// service via the GPIB SRQ line. The coded integer data represents the enabled events using
        /// the definitions given under the SRQB command.NOTE: Setting the SRQ mask to zero disables all
        /// SRQ interrupts.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="SRQValue">Coded int. 0-255</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ServiceRequestEventMaskByte_Command(Socket soc, int SRQValue)
        {
            string param = "SRQM";
            param += SRQValue;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Service Request Event Mask Byte Query: This byte enables the various events for requesting
        /// service via the GPIB SRQ line. The coded integer data represents the enabled events using
        /// the definitions given under the SRQB command.NOTE: Setting the SRQ mask to zero disables all
        /// SRQ interrupts.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>Coded int. 0-255</returns>
        public int ServiceRequestEventMaskByte_Query(Socket soc)
        {
            string param = "SRQM";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Status Word: System Status of controller.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>1:Run program, 2:Hold program, 4:Suspend program, 8:Undefined, 16:Run manual,
        /// 32:Hold manual, 64 & 128:Undefined</returns>
        public int StatusWord(Socket soc)
        {
            string param = "STAT";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Therm Alarm Status Flags Byte: The command allows you to retrieve the Therm Alarm
        /// status flags byte. The flags byte, a coded integer containing status about the Therm Alarm
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>1:High alarm, 2:Low alarm, 4:High warning, 8:Low warning, 16:Mute, 32:Over range,
        /// 128:Open Thermocouple</returns>
        public int ThermAlarmStatusFlags(Socket soc)
        {
            string param = "TALF";
            param += "?";
            string strReturn = bufferCommand(soc, param);
            return int.Parse(strReturn);
        }

        /*------------------- VARIABLE COMMANDS ----------------------------------------------------*/

        /// <summary>
        /// Analog Therm Alarm Settings Command: The operation command allows you to set any of the 
        /// var settings for any analog Therm-Alarm hooked up to the chamber.
        /// NOTE: You must have an original temperature Therm-Alarm and a secondary analog Therm-Alarm installed.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alrmNum">alarm number 1-8</param>
        /// <param name="type">0:linear, 1:RH temp comp, 2:altitude kft, 3:vibration</param>
        /// <param name="lowLimit">0-100 Celsius</param>
        /// <param name="highLimit">0-100 Celsius</param>
        /// <param name="units"></param>
        /// <param name="maxEx">Max Excursion</param>
        /// <param name="mute">mute time 0-99 minutes</param>
        /// <param name="warning">warning band 0-99.999 units</param>
        /// <param name="delay">delay time 0-30 seconds</param>
        /// <param name="reset">reset status. 0: auto reset, 1: manual reset</param>
        /// <param name="maxFreq">Max frequency. 0:1000, 1:2000, 2:5000, 3:10000, 4: 20000</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult AnalogThermAlarmSettings_Command(Socket soc, int alrmNum, int type, float lowLimit, 
                float highLimit, string units, float maxEx, int mute, float warning, int delay,
                int reset, int maxFreq)
        {
            if (alrmNum < 1 || alrmNum > 8) return eResult.FAIL;
            if (type < 0 || type > 3) return eResult.FAIL;
            if (lowLimit < 0F || lowLimit > 100F) return eResult.FAIL;
            if (highLimit < 0F || highLimit > 100F) return eResult.FAIL;
            if (units.Length > 4) return eResult.FAIL;
            if (mute < 0 || mute > 99) return eResult.FAIL;
            if (warning < 0F || warning > 99.999) return eResult.FAIL;
            if (delay < 0 || delay > 30) return eResult.FAIL;
            if (maxFreq < 0 || maxFreq > 4) return eResult.FAIL;
            
            string param = "AALM";
            param += alrmNum + "," + type + "," + lowLimit + "," + highLimit + "," + units + ","
                + maxEx + "," + mute + "," + warning + "," + delay + "," + reset + ",," + maxFreq;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Analog Therm Alarm Settings Query: The query command allows you to view the analog information
        /// for the Therm-Alarms. NOTE: You must have an original temperature Therm-Alarm and a secondary 
        /// analog Therm-Alarm installed.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alrmNum">alarm number 1-8</param>
        /// <returns>Therm Alarm settings info</returns>
        public string AnalogThermAlarmSettings_Query(Socket soc, int alrmNum)
        {
            if (alrmNum < 1 || alrmNum > 8) return "Error";
            string param = "AALM";
            param += alrmNum + "?";
            string strReturn = bufferCommand(soc, param);
            return strReturn;
        }

        /// <summary>
        /// Therm-Alarm Analog Input Channel Availability: The query command asks the controller if
        /// the analog input channel for Therm-Alarm is available.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alrmNum">Alarm number 1-4</param>
        /// <returns>0:not available, 1: available</returns>
        public int ThermAlarmAnalogInputChanAvailab(Socket soc, int alrmNum)
        {
            string param = "ISAA";
            param += alrmNum + "?";
            string strReturn = bufferCommand(soc,param);
            return int.Parse(strReturn);
        }

        /// <summary>
        /// Live Auxiliaries Settings Command: Unlike standard auxiliary relays, live auxiliaries can
        /// be enabled without the controller being in run mode. 
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="aux">1-8</param>
        /// <param name="state">0: off, 1: on</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult LiveAuxSettings_Command(Socket soc, int aux, int state)
        {
            string param = "LAUX";
            param += aux + "," + state;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Live Auxiliaries Settings Query: Unlike standard auxiliary relays, live auxiliaries can
        /// be enabled without the controller being in run mode. 
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="aux">1-8</param>
        /// <returns>0: off, 1: on</returns>
        public bool LiveAuxSettings_Query(Socket soc, int aux)
        {
            string param = "LAUX";
            param += aux + "?";
            string strReturn = bufferCommand(soc, param);
            return bool.Parse(strReturn);
        }

        /// <summary>
        /// Monitor Channel Value: Queries the controller for the current value of the selected monitor channel.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="monitorNum">monitor channel number 1-24</param>
        /// <returns>current monitor channel value</returns>
        public float MonitorChannelValue(Socket soc, int monitorNum)
        {
            string param = "MNTR";
            param += monitorNum + "?";
            string strReturn = bufferCommand(soc, param);
            return float.Parse(strReturn);
        }

        /// <summary>
        /// Manual Ramp Setting Command:This is a manual mode command. The units are in the scale selected at the
        /// 8800 (such as °C, °F, torr, %RH, etc.).
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="control">(1-4) Channel</param>
        /// <param name="rampRate">Ramp Rate in Units</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ManualRampSetting_Command(Socket soc, int control, int rampRate)
        {
            string param = "MRMP";
            param += control + "," + rampRate;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Manual Ramp Setting Query: The query command reads the manual ramp setting for the selected channel 
        /// in units per minute.The units are in the scale selected at the 8800 (such as °C, °F, torr, %RH, etc.).
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="control">(1-4) Channel</param>
        /// <returns>integer ramp value</returns>
        public int ManualRampSetting_Query(Socket soc, int control)
        {
            string param = "MRMP";
            param += control + "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Controller Options Command: If the 8800 is in manual mode, the operation command temporarily
        /// changes the 8800 options register to the new set of options.NOTE: If the selected options are
        /// not available on your chamber, the 8800 will return an error code.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="options">sum of selected options. 1:Product Temperature Control, 2:Humidity System,
        /// 4:Low Humidity System, 8:GSoak, 16:Purge, 32:Cascade Refrigeration, 64:Power Save Mode,
        /// 128:Single Stage Refrig., 512:Altitude, 1024:Mechanical Boost, 32768:Product Dewpoint Control</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ControllerOptions_Command(Socket soc, int options)
        {
            string param = "OPTN";
            param += options;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Controller Options Query: The query command reads the options register of the 8800.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>sum of active options. 1:Product Temperature Control, 2:Humidity System,
        /// 4:Low Humidity System, 8:GSoak, 16:Purge, 32:Cascade Refrigeration, 64:Power Save Mode,
        /// 128:Single Stage Refrig., 512:Altitude, 1024:Mechanical Boost, 32768:Product Dewpoint Control</returns>
        public int ControllerOptions_Query(Socket soc)
        {
            string param = "OPTN";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Parameter Values Command: The operation command sends new parameter values for a selected channel
        /// of a selected parameter group.The 8800 loads the parameter values into the parameter group 
        /// registers in any mode. NOTE: When using the operation command, you must specify values for every
        /// field and not leave any field blank.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">(1-4)</param>
        /// <param name="group">(1-4)</param>
        /// <param name="HnCPropBand">Heat and Cool Proportion Bands (0.0 - 9999.0)</param>
        /// <param name="HnCIntTime">Heat and Cool Integral Time (0 - 1000 seconds)</param>
        /// <param name="HeatOffset">Heat Offset (0.0 - 100.0)</param>
        /// <param name="CoolOffset">Cool Offset (-100.0 - 0.0)</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ParameterValues_Command(Socket soc, int channel, int group, float HnCPropBand,
                int HnCIntTime, float HeatOffset, float CoolOffset)
        {
            string param = "PARM";
            param += channel + "," + group + "," + HnCPropBand + "," + HnCIntTime + "," + HeatOffset + "," +
                    CoolOffset;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Parameter Values Query: The query command causes the 8800 to send the values of the tuning
        /// parameters for the selected channel in the selected parameter group.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">(1-4)</param>
        /// <param name="group">(1-4)</param>
        /// <returns>string containing: heat and cool proportion, heat and cool integral time, heat offset, 
        /// cool offset</returns>
        public string ParameterValues_Query(Socket soc, int channel, int group)
        {
            string param = "PARM";
            param += channel + "," + group + "?";
            string strReturn = bufferCommand(soc, param);
            
            return strReturn;
        }

        /// <summary>
        /// Parameter Group Command: If the 8800 is in manual mode, the operation command selects the
        /// parameter group(1 through 4) that the 8800 will use to control the channels.Edit from hold
        /// operations temporarily change the parameter group for the program.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="group">(1-4)</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ParameterGroup_Command(Socket soc, int group)
        {
            string param = "PRMG";
            param += group;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Parameter Group Query: Queries the 8800 for the number of the parameter group that it is 
        /// currently using.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>Parameter Group Number</returns>
        public int ParameterGroup_Query(Socket soc)
        {
            string param = "PRMG";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Process Variable Channel: Queries the 8800 for the current process variable value of the selected channel.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">(1-4):External process, (5-8):Internal process, (13-28):Monitor channels 1-16,
        /// (33-36):System Monitor temperature chann. for refrigeration system 1, (37-48):System Monitor chann. 
        /// for refrigeration systems 2, 3, and 4, (101-140):Monitor channels 1-40 </param>
        /// <returns>Channel process variable value.</returns>
        public float ProcessVariableChannel(Socket soc, int channel)
        {
            string param = "PVAR";
            param += channel + "?";
            string strReturn = bufferCommand(soc, param);

            return float.Parse(strReturn);
        }

        /// <summary>
        /// Power Fail Recovery Command: This command is for setting the power failure recovery options. The operation 
        /// command allows you to modify the settings.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="enable">0: off, 1: on</param>
        /// <param name="minutesTime">time in minutes. 0-300</param>
        /// <param name="mode">0:stop, 1:hold, 2:run, 3:restart, 4:run PF_Program.NOTE: It is up to the user to create 
        /// the desired program and name it PF_Program.</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult PwrFailRecovery_Command(Socket soc, int enable, int minutesTime, int mode)
        {
            string param = "PWRF";
            param += enable + "," + minutesTime + "," + mode;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Power Fail Recovery Query: The query command asks the 8800 for the current power fail recovery settings.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>power fail recovery enabled, maximum off time, recovery mode</returns>
        public string PwrFailRecovery_Query(Socket soc)
        {
            string param = "PWRF";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return strReturn;
        }

        /// <summary>
        /// Set Point Command: In manual mode, the operation command loads a new set point into the 8800 for the current operation
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-8</param>
        /// <param name="setpoint"></param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult Setpoint_Command(Socket soc, int channel, float setpoint)
        {
            string param = "SETP";
            param += channel + "," + setpoint;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Set Point Query: The query command asks the 8800 for the current set point reading from channel “n”.
        /// The 8800 sends the set point value in the channel’s selected units.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="channel">1-8</param>
        /// <returns>Set point value</returns>
        public float Setpoint_Query(Socket soc, int channel)
        {
            string param = "SETP";
            param += channel + "?";
            string strReturn = bufferCommand(soc, param);

            return float.Parse(strReturn);
        }

        /// <summary>
        /// Safety Stop Command: The operation command sends new controlled stop parameter values.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="stopMode">0: disabled, 1: enabled</param>
        /// <param name="setpoint">0-50 degrees C</param>
        /// <param name="purgeTime">0-3600 seconds</param>
        /// <param name="deviation">0-999.9 degrees C</param>
        /// <param name="controlMode">0: off, 1: on</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult SafetyStop_Command(Socket soc, int stopMode, float setpoint, int purgeTime, float deviation, int controlMode)
        {
            string param = "SSTP";
            param += stopMode + "," + setpoint + "," + purgeTime + "," + deviation + "," + controlMode;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Safety Stop Query: The query command causes the 8800 to return the values of the controlled stop parameters.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>safety stop mode, set point, purge time, safety stop deviation, stop control mode</returns>
        public string SafetyStop_Query(Socket soc)
        {
            string param = "SSTP";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return strReturn;
        }

        /// <summary>
        /// Therm Alam Settings Command: The operation command allows you to set the Therm-Alarm settings.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alarmNumber">1-4</param>
        /// <param name="alarmLowLimit">0-100 C</param>
        /// <param name="alarmHighLimit">0-100 C</param>
        /// <param name="muteTime">0-99 minutes</param>
        /// <param name="warningBand">0-15 degrees C</param>
        /// <param name="delayTime">0-30 seconds</param>
        /// <param name="reset">0: auto-reset, 1: manual reset</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult ThermAlarmSettings_Command(Socket soc, int alarmNumber, int alarmLowLimit, int alarmHighLimit, int muteTime, float warningBand, int  delayTime, int reset)
        {
            string param = "TALM";
            param += alarmNumber + "," + alarmLowLimit + "," + alarmHighLimit + "," + muteTime + "," + warningBand + "," + delayTime + "," + reset;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Therm Alam Settings Query: The query command allows you to retrieve the Therm-Alarm settings.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alarmNumber">1-4</param>
        /// <returns>therm alarm temperature, low limit, high limit, maximum excursion, mute time minutes,
        /// warning band, alarm delay, reset status</returns>
        public string ThermAlarmSettings_Query(Socket soc, int alarmNumber)
        {
            string param = "TALM";
            param += alarmNumber + "?";
            string strReturn = bufferCommand(soc, param);

            return strReturn;
        }

        /// <summary>
        /// Therm-Alarm analog input value: The query command asks the 8800 for the current Therm-Alarm
        /// “n” analog input value.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alarmNumber">1-4</param>
        /// <returns>Therm-Alarm value</returns>
        public int ThermAlrmAnalogInputVal_Query(Socket soc, int alarmNumber)
        {
            string param = "THAA";
            param += alarmNumber + "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Therm-Alarm Temperature: The query command asks the 8800 for the current Therm-Alarm “n” 
        /// temperature reading.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="alarmNumber">1-4</param>
        /// <returns>Therm-Alarm Temperature</returns>
        public int ThermAlrmTemperature_Query(Socket soc, int alarmNumber)
        {
            string param = "THAT";
            param += alarmNumber + "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Throttle Reading Query: The query command asks the 8800 for the current channel “n” throttle reading.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="process">1-8</param>
        /// <returns>throttle percentage</returns>
        public int ThrottleReading_Query(Socket soc, int process)
        {
            string param = "THTL";
            param += process + "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

        /// <summary>
        /// Temperature Scale Command: Allows to change the temperature scale used on the 8800 display.
        /// </summary>
        /// <param name="soc"></param>
        /// <param name="scale">0: Celsius, 1: Fahrenheit</param>
        /// <returns>PASS When the function has completed successfuly. FAIL When a failure has occured.</returns>
        public eResult TemperatureScale_Command(Socket soc, int scale)
        {
            string param = "TMPS";
            param += scale;
            string strReturn = bufferCommand(soc, param);

            return "0\r" == strReturn ? eResult.PASS : eResult.FAIL;
        }

        /// <summary>
        /// Temperature Scale Query: Allows to read the temperature scale used on the 8800 display.
        /// </summary>
        /// <param name="soc"></param>
        /// <returns>temperature scale</returns>
        public int TemperatureScale_Query(Socket soc)
        {
            string param = "TMPS";
            param += "?";
            string strReturn = bufferCommand(soc, param);

            return int.Parse(strReturn);
        }

    }

}