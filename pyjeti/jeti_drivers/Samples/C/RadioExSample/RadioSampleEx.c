/*----------------------------------------------------------------------*/
/*																		*/
/* FILE:    RadioSampleEx.c												*/
/*																		*/
/* PURPOSE: This sample shows how to perform basic radiometric			*/
/*			measurements using the DLL provided by the JETI SDK			*/
/*          Note that you have to include the file jeti_radio_ex.lib	*/
/*          in your project to access all the functions.				*/
/*                                                                      */
/*----------------------------------------------------------------------*/

#include "windows.h"
#include "stdio.h"
#include "conio.h"
#include "jeti_radio_ex.h"


void main(void)
{
	DWORD		dwNumDevices;
	DWORD		dwError;
	DWORD_PTR	dwDevice;
	char		ch;
	int			i;
	BOOL		boStatus;
	FLOAT		fRadio;
	FLOAT		fPhoto;
	FLOAT		fChromx, fChromy;

	system ("cls");

	// determines the number of connected JETI devices
	printf ("Searching device...\n");
	dwError = JETI_GetNumRadioEx (&dwNumDevices);
	if ((dwError != 0) || (dwNumDevices == 0))
	{
		printf ("No matching device found!\nError code 0x%08X\nPress any key to continue...", dwError);
		_getch();
		return;
	}

	// open the first found device (zero-based index)
	dwError = JETI_OpenRadioEx (0, &dwDevice);
	if (dwError != 0)
	{
		printf ("Could not open device!\nError code 0x%08X\nPress any key to continue...", dwError);
		_getch();
		return;
	}
	

	do
	{
		system ("cls");
		printf ("Please select:\n");
		printf ("--------------\n\n");
		printf ("1) perform radiometric measurement...\n");
		printf ("0) exit\n\n");
		ch = _getch();
		switch (ch)
		{
			case '1':		// Perform a complete radiometric measurement
					// Start a radiometric measurement with an average of 1 and a step-width of 1nm.
					// The integration time will be determined automatically (0).
					printf ("Performing measurement. Please wait...\n\n");
					dwError = JETI_MeasureEx (dwDevice, 0, 1, 1);
					if (dwError != 0)
					{
						printf ("Could not start measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
						break;
					}

					// Checks the measurment status until boStatus becomes TRUE (measurement has finished)
					do
					{
						dwError = JETI_MeasureStatusEx (dwDevice, &boStatus);
						if (dwError != 0)
						{
							printf ("Could not determine measurement status!\nError code 0x%08X\nPress any key to continue...", dwError);
							break;
						}
					} while (boStatus);

					// Returns the radiometric value
					dwError = JETI_RadioEx (dwDevice, 380, 780, &fRadio);
					if (dwError != 0)
						printf ("Could not get radiometric value!\nError code 0x%08X\n", dwError);
					else
						printf ("radiometric value: %.3E\n", fRadio);
					// Returns the photometric value determined by the last measuement
					dwError = JETI_PhotoEx (dwDevice, &fPhoto);
					if (dwError != 0)
						printf ("Could not get photometric value!\nError code 0x%08X\n", dwError);
					else
						printf ("photometric value: %.3E\n", fPhoto);
					// Returns the CIE-1931 chromaticity coordinates xy determined by the last measurement
					dwError = JETI_ChromxyEx (dwDevice, &fChromx, &fChromy);
					if (dwError != 0)
						printf ("Could not get chromaticity coordinates x and y!\nError code 0x%08X\nPress any key to continue...", dwError);
					else
						printf ("chromaticity coordinates:\nx:  %.4f\ny:  %.4f\nPress any key to continue...", fChromx, fChromy);
					_getch();
				break;
			default:
					system ("cls");
				break;
		}
	} while (ch != '0');

	// close the connection to the device
	JETI_CloseRadioEx (dwDevice);

	printf ("Press any key to continue...");
	_getch();
	return;
}

