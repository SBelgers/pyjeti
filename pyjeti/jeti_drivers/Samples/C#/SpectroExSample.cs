using System;
using System.Runtime.InteropServices;

/// <summary>
/// PURPOSE: 
/// This sample shows how to perform basic spectrometric			
/// measurements using the DLL provided by the JETI SDK.
/// 
/// Written by JETI Technische Instrumente GmbH using MS Visual C# 2019
/// </summary>
namespace SpectroSampleCSharp
{
    internal class JETI_SpectroExDll
    {
#if WIN64
		private const string JetiSpectroExDll = "jeti_spectro_ex64.dll";
#else
        private const string JetiSpectroExDll = "jeti_spectro_ex.dll";
#endif

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_GetNumSpectroEx(out UInt32 uiNumDevices);

        [DllImport(JetiSpectroExDll, CharSet = CharSet.Ansi)]
        public static extern UInt32 JETI_GetSerialSpectroEx(UInt32 uiDeviceNum, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbBoardSerialNr,
            [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbSpecSerialNr, [MarshalAs(UnmanagedType.LPStr), Out] System.Text.StringBuilder sbDeviceSerialNr);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_OpenSpectroEx(UInt32 uiDeviceNum, out UIntPtr puiDevice);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_CloseSpectroEx(UIntPtr puiDevice);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_StartLightEx(UIntPtr puiDevice, float fTint, ushort ushAver);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_SpectroStatusEx(UIntPtr puiDevice, [MarshalAs(UnmanagedType.Bool)] out bool bStatus);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_SpectroBreakEx(UIntPtr puiDevice);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_LightPixEx(UIntPtr puiDevice, [Out] Int32[] iLight);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_LightWaveEx(UIntPtr puiDevice, UInt32 uiBeg, UInt32 uiEnd, ,float fStep, [Out] float[] fLight);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_SpectroTintEx(UIntPtr puiDevice, out float fTint);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_PixelCountEx(UIntPtr puiDevice, out UInt32 uiPixel);

        [DllImport(JetiSpectroExDll)]
        public static extern UInt32 JETI_GetRadioExDLLVersion(out ushort ushMajorVersion, out ushort ushMinorVersion, out ushort ushBuildNumber);

    }


    class SpectroExSample
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
			float fLight[];
			Int32 iLight[];
			UInt32 uiPixel;
            System.Text.StringBuilder sbBoardSerialNr = new System.Text.StringBuilder(16);
            System.Text.StringBuilder sbSpecSerialNr = new System.Text.StringBuilder(16);
            System.Text.StringBuilder sbDeviceSerialNr = new System.Text.StringBuilder(16);

            string sTmp;

            Console.Clear();
            try
            {
                // determines the number of connected JETI devices
                Console.WriteLine("Searching device...");
                uiError = JETI_SpectroExDll.JETI_GetNumSpectroEx(out uiNumDevices);
                if ((uiError != 0) || (uiNumDevices == 0))
                {
                    Console.WriteLine($"No matching device found!\nError code {uiError:X08}\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }

                // open the first found device (zero-based index)
                uiError = JETI_SpectroExDll.JETI_OpenSpectroEx(0, out puiDevice);
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
                    Console.WriteLine("1) perform light measurement...");
                    Console.WriteLine("2) get serial numbers...\n\n");
                    Console.WriteLine("*********************");
                    Console.WriteLine("* Single Operations *");
                    Console.WriteLine("*********************\n");
                    Console.WriteLine("a) start light measurement...");
                    Console.WriteLine("b) break measurement...");
                    Console.WriteLine("c) get measurement status...");
                    Console.WriteLine("d) get the light spectrum (wavelength based)...");
                    Console.WriteLine("e) get the light spectrum (pixel based)...");
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
                    }
                    switch (ch)
                    {
                        case '1':       // Perform a complete light measurement
                            // Start a light measurement.
                            Console.WriteLine("Performing measurement. Please wait...\n");
                            uiError = JETI_SpectroExDll.JETI_StartLightEx(puiDevice, fTint, ushAverage);
                            if (uiError != 0)
                            {
                                Console.WriteLine($"Could not start measurement!\nError code {uiError:X08}\nPress any key to continue...");
                                break;
                            }

                            // Checks the measurment status until boStatus becomes TRUE (measurement has finished)
                            do
                            {
                                uiError = JETI_SpectroExDll.JETI_SpectroStatusEx(puiDevice, out bStatus);
                                if (uiError != 0)
                                {
                                    Console.WriteLine($"Could not determine measurement status!\nError code {uiError:X08}\nPress any key to continue...");
                                    break;
                                }
                            } while (bStatus);

                            // read the light spectrum (380-780nm, 1nm step-width)
							fLight = new float[401];
                            uiError = JETI_SpectroExDll.JETI_LightWaveEx(puiDevice, 380, 780, 1, out fLight);
                            if (uiError != 0)
                                Console.WriteLine($"Could not read light spectrum!\nError code {uiError:X08}\nPress any key to continue...");
                            else
							{
                                for (i = 0; i < 401; i++)
									Console.Write($"wl[nm]: {i+380}    cts: {fLight[i]}");
							}
                            Console.ReadKey(true);
                            break;
                        case '2':
                            // get serial numbers from the first found device (zero-based index)
                            uiError = JETI_SpectroExDll.JETI_GetSerialSpectroEx(0, sbBoardSerialNr, sbSpecSerialNr, sbDeviceSerialNr);
                            if (uiError != 0)
                                Console.WriteLine($"Could not get serial numbers!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"electronics serial number: {sbBoardSerialNr}\nspectrometer serial number: {sbSpecSerialNr}\n" +
                                    $"device serial number: {sbDeviceSerialNr}\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'a':       //	Start a light measurement
                            uiError = JETI_SpectroExDll.JETI_StartLightEx(puiDevice, fTint, ushAverage);
                            if (uiError != 0)
                                Console.WriteLine($"Could not start measurement!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine($"Measurement successfully started.\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'b':       // Cancels an initiated measurement
                            uiError = JETI_SpectroExDll.JETI_SpectroBreakEx(puiDevice);
                            if (uiError != 0)
                                Console.WriteLine($"Could not break measurement!\nError code {uiError:X08}\nPress any key to continue...");
                            else
                                Console.WriteLine("Measurement cancelled.\nPress any key to continue...");
                            Console.ReadKey(true);
                            break;
                        case 'c':       // Returns the status of the initiated measurement
                            uiError = JETI_SpectroExDll.JETI_SpectroStatusEx(puiDevice, out bStatus);
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
                        case 'd':		// read the light spectrum (380-780nm, 1nm step-width)
							fLight = new float[401];
                            uiError = JETI_SpectroExDll.JETI_LightWaveEx(puiDevice, 380, 780, 1, out fLight);
                            if (uiError != 0)
                                Console.WriteLine($"Could not read light spectrum!\nError code {uiError:X08}\nPress any key to continue...");
                            else
							{
                                for (i = 0; i < 401; i++)
									Console.Write($"wl[nm]: {i+380}    cts: {fLight[i]}");
							}
                            Console.ReadKey(true);
                            break;
                        case 'e':       // read the light spectrum (pixel based)
							uiError = JETI_SpectroExDll.JETI_PixelCountEx(puiDevice, out uiPixel);
							iLight = new Int32[uiPixel];
                            uiError = JETI_SpectroExDll.JETI_LightPixEx(puiDevice, out iLight);
                            if (uiError != 0)
                                Console.WriteLine($"Could not read light spectrum!\nError code {uiError:X08}\nPress any key to continue...");
                            else
							{
                                for (i = 0; i < uiPixel; i++)
									Console.Write($"pix: {i}    cts: {iLight[i]}");
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
                if (puiDevice != UIntPtr.Zero) JETI_SpectroExDll.JETI_CloseSpectroEx(puiDevice);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            return;
        }
    }
}
