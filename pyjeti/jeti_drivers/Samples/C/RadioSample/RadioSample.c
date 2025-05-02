/*----------------------------------------------------------------------*/
/*																		*/
/* FILE:    RadioSample.c												*/
/*																		*/
/* PURPOSE: This sample shows how to perform basic radiometric			*/
/*			measurements using the DLL provided by the JETI SDK			*/
/*          Note that you have to include the file jeti_radio.lib		*/
/*          in your project to access all the functions.				*/
/*																		*/
/* Written by JETI Technische Instrumente GmbH using MS Visual C++ 2013	*/
/*																		*/
/*                                                                      */
/*----------------------------------------------------------------------*/

#include "windows.h"
#include "stdio.h"
#include "conio.h"
#include "jeti_core.h"
#include "jeti_radio.h"


void main(void)
{
	DWORD		dwComPort;
	DWORD		dwBaudrate;
	DWORD		dwNumDevices;
	DWORD		dwError;
	DWORD_PTR	dwDevice;
	char		ch;
	int			i;
	BOOL		boStatus;
	FLOAT		fRadio;
	FLOAT		fPhoto;
	FLOAT		fChromx, fChromy;
	FLOAT		fCCT;
	FLOAT		*fCRI;

	system ("cls");

	// ask for a COM port number
	printf ("Please enter a COM port number (1...255) to open or 0 to search device: ");
	scanf_s ("%d", &dwComPort);

	if (dwComPort > 0)
	{
		// ask for baudrate
		printf ("Please enter a baudrate (921600, 115200, 38400): ");
		scanf_s ("%d", &dwBaudrate);
		// open the device on the specified COM port
		dwError = JETI_OpenCOMDevice (dwComPort, dwBaudrate, &dwDevice);
		if (dwError != 0)
		{
			printf ("Could not open device!\nError code 0x%08X\nPress any key to continue...", dwError);
			_getch();
			return;
		}
	}
	else
	{
		// determines the number of connected JETI devices
		printf ("Searching device...\n");
		dwError = JETI_GetNumRadio (&dwNumDevices);
		if ((dwError != 0) || (dwNumDevices == 0))
		{
			printf ("No matching device found!\nError code 0x%08X\nPress any key to continue...", dwError);
			_getch();
			return;
		}

		// open the first found device (zero-based index)
		dwError = JETI_OpenRadio (0, &dwDevice);
		if (dwError != 0)
		{
			printf ("Could not open device!\nError code 0x%08X\nPress any key to continue...", dwError);
			_getch();
			return;
		}
	}
	

	do
	{
		system ("cls");
		printf ("Please select:\n");
		printf ("--------------\n\n");
		printf ("1) perform radiometric measurement...\n\n\n");
		printf ("*********************\n");
		printf ("* Single Operations *\n");
		printf ("*********************\n\n");
		printf ("a) start radiometric measurement...\n");
		printf ("b) break measurement...\n");
		printf ("c) get measurement status...\n");
		printf ("d) get the radiometric value...\n");
		printf ("e) get the photometric value...\n");
		printf ("f) get chromaticity coordinates x and y...\n");
		printf ("g) get correlated color temperature CCT...\n");
		printf ("h) get color rendering index CRI...\n");
		printf ("0) exit\n\n");
		ch = _getch();
		switch (ch)
		{
			case '1':		// Perform a complete radiometric measurement
					// Start a radiometric measurement in the range of 380 to 780 nm with a step-width of 5nm.
					// The integration time will be determined automatically.
					printf ("Performing measurement. Please wait...\n\n");
					dwError = JETI_Measure (dwDevice);
					if (dwError != 0)
					{
						printf ("Could not start measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
						break;
					}

					// Checks the measurment status until boStatus becomes TRUE (measurement has finished)
					do
					{
						dwError = JETI_MeasureStatus (dwDevice, &boStatus);
						if (dwError != 0)
						{
							printf ("Could not determine measurement status!\nError code 0x%08X\nPress any key to continue...", dwError);
							break;
						}
					} while (boStatus);

					// Returns the radiometric value
					dwError = JETI_Radio (dwDevice, &fRadio);
					if (dwError != 0)
						printf ("Could not get radiometric value!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("radiometric value: %.3E\nPress any key to continue...", fRadio);
					_getch();
				break;
			case 'a':		//	Start a radiometric measurement in the range of 380 to 780 nm with a step-width of 5nm.
							// The integration time will be determined automatically.
					dwError = JETI_Measure (dwDevice);
					if (dwError != 0)
						printf ("Could not start measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("Measurement successfully started.\nPress any key to continue...");
					_getch();
				break;
			case 'b':		// Cancels an initiated measurement
					dwError = JETI_MeasureBreak (dwDevice);
					if (dwError != 0)
						printf ("Could not break measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("Measurement cancelled.\nPress any key to continue...");
					_getch ();
				break;
			case 'c':		// Returns the status of the initiated measurement
					dwError = JETI_MeasureStatus (dwDevice, &boStatus);
					if (dwError != 0)
						printf ("Could not determine measurement status!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
					{
						if (boStatus)
							printf ("Measurement in progress.\nPress any key to continue...");
						else
							printf ("Measurement ready / Device idle.\nPress any key to continue...");
					}
					_getch();
				break;
			case 'd':		// Returns the radiometric value determined by the last measurement
					dwError = JETI_Radio (dwDevice, &fRadio);
					if (dwError != 0)
						printf ("Could not get radiometric value!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("radiometric value: %.3E\nPress any key to continue...", fRadio);
					_getch();
				break;
			case 'e':		// Returns the photometric value determined by the last measuement
					dwError = JETI_Photo (dwDevice, &fPhoto);
					if (dwError != 0)
						printf ("Could not get photometric value!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("photometric value: %.3E\nPress any key to continue...", fPhoto);
					_getch();
				break;
			case 'f':		// Returns the CIE-1931 chromaticity coordinates xy determined by the last measurement
					dwError = JETI_Chromxy (dwDevice, &fChromx, &fChromy);
					if (dwError != 0)
						printf ("Could not get chromaticity coordinates x and y!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("chromaticity coordinates:\nx:  %.4f\ny:  %.4f\nPress any key to continue...", fChromx, fChromy);
					_getch();
				break;
			case 'g':		// Returns the correlated color temperature determined by the last measurement
					dwError = JETI_CCT (dwDevice, &fCCT);
					if (dwError != 0)
						printf ("Could not get correlated color temperature!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("correlated color temperature CCT: %.1f\nPress any key to continue...", fCCT);
					_getch();
				break;
			case 'h':		// Returns the color rendering indices according to the CIE 13.3-1995 publication
					fCRI = (FLOAT*) calloc (17, sizeof(FLOAT));
					dwError = JETI_CRI (dwDevice, fCRI);
					if (dwError != 0)
						printf ("Could not get colour rendering indizes!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
					{
						printf ("DC:  %.1E\n", fCRI[0]);				// chromaticity difference
						printf ("Ra:  %.2f\n", fCRI[1]);				// general color rendering index
						for (i=0; i<15; i++)
							printf ("R%d:  %.1f\n", i+1, fCRI[i+2]);	// special color rendering indices
						printf ("Press any key to continue...");
					}
					free (fCRI);
					_getch();
				break;
			default:
					system ("cls");
				break;
		}
	} while (ch != '0');

	// close the connection to the device
	JETI_CloseRadio (dwDevice);

	printf ("Press any key to continue...");
	_getch();
	return;
}

