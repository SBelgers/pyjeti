/*----------------------------------------------------------------------*/
/*																		*/
/* FILE:    SpectroExSample.c											*/
/*																		*/
/* PURPOSE: This sample shows how to perform basic radiometric			*/
/*			measurements using the DLL provided by the JETI SDK			*/
/*          Note that you have to include the file jeti_radio.lib		*/
/*          in your project to access all the functions.				*/
/*																		*/
/* Written by JETI Technische Instrumente GmbH using MS Visual C++ 2005	*/
/*																		*/
/*                                                                      */
/*----------------------------------------------------------------------*/

#include "windows.h"
#include "stdio.h"
#include "conio.h"
#include "jeti_core.h"
#include "jeti_spectro_ex.h"


void main(void)
{
	DWORD	dwComPort;
	DWORD	dwBaudrate;
	DWORD	dwNumDevices;
	DWORD	dwError;
	DWORD	dwDevice;
	char	ch;
	int		i;
	BOOL	boStatus;
	float	*fLight;
	DWORD	*dwLight;
	DWORD	dwTint;
	DWORD	dwAver;
	DWORD	dwBeg, dwEnd;
	FLOAT	fStep;
	DWORD	dwCount;
	DWORD	dwPixel;


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
		dwError = JETI_GetNumSpectroEx (&dwNumDevices);
		if ((dwError != 0) || (dwNumDevices == 0))
		{
			printf ("No matching device found!\nError code 0x%08X\nPress any key to continue...", dwError);
			_getch();
			return;
		}

		// open the first found device (zero-based index)
		dwError = JETI_OpenSpectroEx (0, &dwDevice);
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
		printf ("1) perform light measurement...\n\n\n");
		printf ("*********************\n");
		printf ("* Single Operations *\n");
		printf ("*********************\n\n");
		printf ("a) start light measurement...\n");
		printf ("b) break measurement...\n");
		printf ("c) get measurement status...\n");
		printf ("d) get the light spectrum (wavelength based)...\n");
		printf ("e) get the light spectrum (pixel based)...\n\n");
		printf ("0) exit\n\n");
		ch = _getch();
		switch (ch)
		{
			case '1':		// Perform a complete light measurement
					// Start a light measurement in the range of 380 to 780 nm with a step-width of 5nm.
					// The integration time will be 100ms and average will be 1.
					printf ("Performing measurement. Please wait...\n\n");
					dwError = JETI_StartLightEx (dwDevice, 100, 1);
					if (dwError != 0)
					{
						printf ("Could not start measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
						break;
					}

					// Checks the measurment status until boStatus becomes TRUE (measurement has finished)
					do
					{
						dwError = JETI_SpectroStatusEx (dwDevice, &boStatus);
						if (dwError != 0)
						{
							printf ("Could not determine measurement status!\nError code 0x%08X\nPress any key to continue...", dwError);
							break;
						}
					} while (boStatus);

					// read the light spectrum
					fLight = calloc (81, sizeof(float));		// ((780-380)/5)+1 = 81 values
					dwError = JETI_LightWaveEx (dwDevice, 380, 780, 5, fLight);
					if (dwError != 0)
						printf ("Could not read light spectrum!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
					{
						printf ("light spectrum:\n");
						for (i=0; i<81; i++)
							printf ("wl[nm]: %d     cts: %.2f\n", i*5+380, fLight[i]);
						printf ("Press any key to continue...");
					}
					free (fLight);
					_getch();
				break;
			case 'a':		//	Start a light measurement
					printf ("Please enter an integration time (0 for adaption): ");
					scanf_s ("%d", &dwTint);
					printf ("Please enter the count of measurement for averaging: ");
					scanf_s ("%d", &dwAver);
					dwError = JETI_StartLightEx (dwDevice, dwTint, (WORD) dwAver);
					if (dwError != 0)
						printf ("Could not start light measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("Light measurement successfully started.\nPress any key to continue...");
					_getch();
				break;
			case 'b':		// Cancels an initiated measurement
					dwError = JETI_SpectroBreakEx (dwDevice);
					if (dwError != 0)
						printf ("Could not break measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("Measurement cancelled.\nPress any key to continue...");
					_getch ();
				break;
			case 'c':		// Returns the status of the initiated measurement
					dwError = JETI_SpectroStatusEx (dwDevice, &boStatus);
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
			case 'd':		// Returns the light spectrum (wavelangth based)
					printf ("Please enter the start wavelength [nm]: ");
					scanf_s ("%d", &dwBeg);
					printf ("Please enter the end wavelength [nm]: ");
					scanf_s ("%d", &dwEnd);
					printf ("Please enter the step-width [nm]: ");
					scanf_s ("%f", &fStep);
					dwCount = (DWORD) (((dwEnd-dwBeg) / fStep) + 1);
					fLight = (FLOAT*) calloc (dwCount, sizeof(FLOAT));
					dwError = JETI_LightWaveEx (dwDevice, dwBeg, dwEnd, fStep, fLight);
					if (dwError != 0)
						printf ("Could not get light spectrum!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
					{
						printf ("light spectrum:\n");
						for (i=0; i<dwCount; i++)
							printf ("wl[nm]: %.1f     cts: %.2f\n", i*fStep+dwBeg, fLight[i]);
						printf ("Press any key to continue...");
					}
					free (fLight);
					_getch();
				break;
			case 'e':		// Returns the light spectrum (pixel based)
					JETI_PixelCountEx (dwDevice, &dwPixel);
					dwLight = (DWORD*) calloc (dwPixel, sizeof(DWORD));
					dwError = JETI_LightPixEx (dwDevice, dwLight);
					if (dwError != 0)
						printf ("Could not get light spectrum!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
					{
						printf ("light spectrum:\n");
						for (i=0; i<dwPixel; i++)
							printf ("pix: %3d     cts: %5d\n", i, dwLight[i]);
						printf ("Press any key to continue...");
					}
					free (dwLight);
					_getch();
				break;
			default:
					system ("cls");
				break;
		}
	} while (ch != '0');

	// close the connection to the device
	JETI_CloseSpectroEx (dwDevice);

	printf ("Press any key to continue...");
	_getch();
	return;
}

