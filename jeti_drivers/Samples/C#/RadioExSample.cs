using System;
using System.Runtime.InteropServices;

/// <summary>
/// PURPOSE: 
/// This sample shows how to perform basic radiometric			
/// measurements using the DLL provided by the JETI SDK.
/// 
/// Written by JETI Technische Instrumente GmbH using MS Visual C# 2019
/// </summary>
namespace RadioSampleCSharp
{
    internal class JETI_RadioExDll
    {
#if WIN64
		private const string JetiRadioExDll = "jeti_radio_ex64.dll";
#else
        private const string JetiRadioExDll = "jeti_radio_ex.dll";
#endif

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_GetNumRadioEx(out UInt32 uiNumDevices);

        [DllImport(JetiRadioExDll, CharSet = CharSet.Ansi)]
        public static extern UInt32 JETI_GetSerialRadioEx(UInt32 uiDeviceNum, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbBoardSerialNr,
            [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbSpecSerialNr, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbDeviceSerialNr);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_OpenRadioEx(UInt32 uiDeviceNum, out UIntPtr puiDevice);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_CloseRadioEx(UIntPtr puiDevice);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_MeasureEx(UIntPtr puiDevice, float fTint, ushort ushAver, UInt32 uiStep);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_MeasureAdaptEx(UIntPtr puiDevice, ushort ushAver, UInt32 uiStep);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_PrepareMeasureEx(UIntPtr puiDevice, float fTint, ushort ushAver, UInt32 uiStep);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_MeasureStatusEx(UIntPtr puiDevice, [MarshalAs(UnmanagedType.Bool)] out bool bStatus);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_MeasureAdaptStatusEx(UIntPtr puiDevice, out float fTint, out ushort ushAverage, [MarshalAs(UnmanagedType.Bool)] out bool bStatus);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_MeasureBreakEx(UIntPtr puiDevice);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_SpecRadEx(UIntPtr puiDevice, UInt32 uiBeg, UInt32 uiEnd, [Out] float[] faSprad);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_SaveSpecRadSPCEx(UIntPtr puiDevice, UInt32 uiBeg, UInt32 uiEnd, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbPathName,
            [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbOperator, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbMemo);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_SaveSpecRadCSVEx(UIntPtr puiDevice, UInt32 uiBeg, UInt32 uiEnd, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbPathName,
            [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbOperator, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbMemo);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_RadioEx(UIntPtr puiDevice, UInt32 uiBeg, UInt32 uiEnd, out float fRadio);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_PhotoEx(UIntPtr puiDevice, out float fPhoto);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_ChromxyEx(UIntPtr puiDevice, out float fChromx, out float fChromy);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_Chromxy10Ex(UIntPtr puiDevice, out float fChromx10, out float fChromy10);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_ChromuvEx(UIntPtr puiDevice, out float fChromu, out float fChromv);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_ChromXYZEx(UIntPtr puiDevice, out float fX, out float fY, out float fZ);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_DWLPEEx(UIntPtr puiDevice, out float fDWL, out float fPE);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_CCTEx(UIntPtr puiDevice, out float fCCT);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_DuvEx(UIntPtr puiDevice, out float fDuv);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_CRIEx(UIntPtr puiDevice, float fCCT, [Out] float[] fCRI);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_RadioTintEx(UIntPtr puiDevice, out float fTint);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_SetMeasDistEx(UIntPtr puiDevice, UInt32 uiDistance);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_GetMeasDistEx(UIntPtr puiDevice, out ushort ushDistance);

        [DllImport(JetiRadioExDll)]
        public static extern UInt32 JETI_GetRadioExDLLVersion(out ushort ushMajorVersion, out ushort ushMinorVersion, out ushort ushBuildNumber);

    }


    class RadioExSample
    {

        static void Main(string[] args)
        {
            uint uiNumDevices;
            uint uiError;
            UIntPtr puiDevice = UIntPtr.Zero;
            char ch = '0';
            int i;
            float fTint = 0;
            ushort ushAverage = 0;
            uint uiStep = 0;
            bool bStatus;
            float fRadio;
            float fPhoto;
            float fChromx, fChromy;
            float fCCT;
            float[] faCRI;
            System.Text.StringBuilder sbBoardSerialNr = new System.Text.StringBuilder(16);
            System.Text.StringBuilder sbSpecSerialNr = new System.Text.StringBuilder(16);
            System.Text.StringBuilder sbDeviceSerialNr = new System.Text.StringBuilder(16);

            string sTmp;

            Console.Clear();
            try
            {
                // determines the number of connected JETI devices
                Console.WriteLine("Searching device...");
                uiError = JETI_RadioExDll.JETI_GetNumRadioEx(out uiNumDevices);
                if ((uiError != 0) || (uiNumDevices == 0))
                {
                    Console.WriteLine($"No matching device found!\nError code {uiError:X08}\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }

                // open the first found device (zero-based index)
                uiError = JETI_RadioExDll.JETI_OpenRadioEx(0, out puiDevice);
                if (uiError != 0)
                {
                    Console.WriteLine($"Could not open device!\nError code {uiError:X08}\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }

                do
                {
                    Console.Clear();
                    Console.WriteLine("Please select:");
                    Console.WriteLine("--------------\n");
                    Console.WriteLine("1) perform radiometric measurement...");
                    Console.WriteLine("2) get serial numbers...\n\n");
                    Console.WriteLine("*********************");
                    Console.WriteLine("* Single Operations *");
                    Console.WriteLine("*********************\n");
                    Console.WriteLine("a) start radiometric measurement...");
                    Console.WriteLine("b) break measurement...");
                    Console.WriteLine("c) get measurement status...");
                    Console.WriteLine("d) get the radiometric value...");
                    Console.WriteLine("e) get the photometric value...");
                    Console.WriteLine("f) get chromaticity coordinates x and y...");
                    Console.WriteLine("g) get correlated color temperature CCT...");
                    Console.WriteLine("h) get color rendering index CRI...");
                    Console.WriteLine("0) exit\n");
                    sTmp = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(sTmp))
                        continue;
                    ch = sTmp[0];

                    if (ch == '1' || ch == 'a')
                    {
                        Console.Write("Please enter an integration time (0 for adaption): ");
                        sTmp = Console.ReadLine();
                        if (!float.TryParse(sTmp, out fTint))
                            throw new Exception($"'{sTmp}' is an invalid integration time.");

                        Console.Write("Please enter the count of measurement for averaging: ");
                        sTmp = Console.ReadLine();
                        if (!ushort.TryParse(sTmp, out ushAverage))
                            throw new Exception($"'{sTmp}' is an invalid count of measurement for averaging.");

                        Console.Write("Please enter the stepwidth in [nm]: ");
                        sTmp = Console.ReadLine();
                        if (!UInt32.TryParse(sTmp, out uiStep))
                            throw new Exception($"'{sTmp}' is an invalid stepwidth.");
                    }
                    switch (ch)
                    {
                        case '1':       // Perform a complete radiometric measurement
                                        // Start a radiometric measurement in the range of 380 to 780 nm with a step-width of 5nm.
                                        // The integration time will be determined automatically.

                            Console.WriteLine("Performing measurement. Please wait...\n");
                            uiError = JETI_RadioExDll.JETI_MeasureEx(puiDevice, fTint, ushAverage, uiStep);
                            if (uiError != 0)
                            {
                                Console.WriteLine($"Could not start measurement!\nError code {uiError:X08}\nPress any key to continue...");
                                break;
                            }

                            // Checks the measurment status until boStatus becomes TRUE (measurement has finished)
                            do
                            {
                                uiError = JETI_RadioExDll.JETI_MeasureStatusEx(puiDevice, out bStatus);
                                if (uiError != 0)
                                {
                                    Console.WriteLine($"Could not determine measurement status!\nError code {uiError:X08}\nPress any key to continue...");
                                    break;
                                }
                            } while (bStatus);

                            // Returns the radiometric value
                            uiError = JETI_RadioExDll.JETI_RadioEx(puiDevice, 380, 780, out fRadio);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get radiometric value!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"radiometric value: {fRadio:E3}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case '2':
                            // get serial numbers from the first found device (zero-based index)
                            uiError = JETI_RadioExDll.JETI_GetSerialRadioEx(0, sbBoardSerialNr, sbSpecSerialNr, sbDeviceSerialNr);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get serial numbers!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"electronics serial number: {sbBoardSerialNr}\nspectrometer serial number: {sbSpecSerialNr}\n" +
                                    $"device serial number: {sbDeviceSerialNr}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'a':       //	Start a radiometric measurement in the range of 380 to 780 nm with a step-width of 5nm.
                                        // The integration time will be determined automatically.
                            uiError = JETI_RadioExDll.JETI_MeasureEx(puiDevice, fTint, ushAverage, uiStep);
                            if (uiError != 0)
                                Console.WriteLine($"Could not start measurement!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"Measurement successfully started.\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'b':       // Cancels an initiated measurement
                            uiError = JETI_RadioExDll.JETI_MeasureBreakEx(puiDevice);
                            if (uiError != 0)
                                Console.WriteLine($"Could not break measurement!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine("Measurement cancelled.\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'c':       // Returns the status of the initiated measurement
                            uiError = JETI_RadioExDll.JETI_MeasureStatusEx(puiDevice, out bStatus);
                            if (uiError != 0)
                                Console.WriteLine($"Could not determine measurement status!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                            {
                                if (bStatus)
                                    Console.WriteLine("Measurement in progress.\nPress any key to continue...");
                                else
                                    Console.WriteLine("Measurement ready / Device idle.\nPress any key to continue...");
                            }
                            Console.ReadKey(true);
                            break;
                        case 'd':       // Returns the radiometric value determined by the last measurement
                            uiError = JETI_RadioExDll.JETI_RadioEx(puiDevice, 380, 780, out fRadio);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get radiometric value!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"radiometric value: {fRadio:E3}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'e':       // Returns the photometric value determined by the last measuement
                            uiError = JETI_RadioExDll.JETI_PhotoEx(puiDevice, out fPhoto);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get photometric value!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"photometric value: {fPhoto:E3}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'f':       // Returns the CIE-1931 chromaticity coordinates xy determined by the last measurement
                            uiError = JETI_RadioExDll.JETI_ChromxyEx(puiDevice, out fChromx, out fChromy);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get chromaticity coordinates x and y!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"chromaticity coordinates:\nx:  {fChromx:F4} \ny:  {fChromy:F4}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'g':       // Returns the correlated color temperature determined by the last measurement
                            uiError = JETI_RadioExDll.JETI_CCTEx(puiDevice, out fCCT);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get correlated color temperature!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"correlated color temperature CCT: {fCCT:F1}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'h':       // Returns the color rendering indices according to the CIE 13.3-1995 publication
                            faCRI = new float[17];
                            Console.Write("Please enter the correlated color temperature of the reference source: ");
                            sTmp = Console.ReadLine();
                            if (!float.TryParse(sTmp, out fCCT))
                                throw new Exception($"'{sTmp}' is an invalid color temperature.");
                            uiError = JETI_RadioExDll.JETI_CRIEx(puiDevice, fCCT, faCRI);
                            if (uiError != 0)
                                Console.WriteLine("Could not get colour rendering indizes!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                            {
                                Console.Write($"DC:  {faCRI[0]:E1}\n");             // chromaticity difference
                                Console.Write($"Ra:  {faCRI[1]:F2}\n");             // general color rendering index
                                for (i = 0; i < 15; i++)
                                    Console.Write($"R{i + 1}:  {faCRI[i + 2]:F1}\n", i + 1, faCRI[i + 2]); // special color rendering indices
                                Console.WriteLine("Press any key to continue...");
                            }
                            Console.ReadKey(true);
                            break;
                        default:
                            Console.Clear();
                            break;
                    }
                } while (ch != '0');

            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            finally
            {   // close the connection to the device
                if (puiDevice != UIntPtr.Zero) JETI_RadioExDll.JETI_CloseRadioEx(puiDevice);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
    }
}
