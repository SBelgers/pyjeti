/*--------------------------------------------------------------------------*/
/*                                                                          */
/* FILE:    SyncSample.c													*/
/*                                                                          */
/* PURPOSE: This sample shows how to make synchronized measurements with	*/
/*          the use of an optical trigger and the cycle mode				*/
/*          Note that you have to include the files jeti_core.lib			*/
/*			and jeti_radio.lib in your project to access all the functions.	*/
/*																			*/
/* Written by JETI Technische Instrumente GmbH using MS Visual C++ 2013		*/
/*																			*/
/*                                                                          */
/*--------------------------------------------------------------------------*/

#include "windows.h"
#include "stdio.h"
#include "conio.h"
#include "jeti_core.h"
#include "jeti_radio.h"

DWORD_PTR	dwDevice;
DWORD		dwError;

int PerformMeasurement (void);

void main(void)
{
	int		ch;
	int		i;
	DWORD	dwNumDevices;
	DWORD	dwDeviceNum;
	char	cBoardSerialNr[16];
	char	cSpecSerialNr[16];
	char	cDeviceSerialNr[16];

	system ("cls");
	JETI_GetNumRadio (&dwNumDevices);
	if (dwNumDevices == 0)
	{
		printf ("No matching device found!\nError code 0x%08X\nPress any key to continue...", dwError);
		_getch();
		return;
	}
	else
	{
		if (dwNumDevices > 1)			// More than one device found
		{
			printf ("More than one device found:\n");
			for (i=0; i<dwNumDevices; i++)
			{
				JETI_GetSerialRadio (i, cBoardSerialNr, cSpecSerialNr, cDeviceSerialNr);
				printf ("device number: %d  board serial number: %s  spectrometer serial number: %s  device serial number: %s\n", i, cBoardSerialNr, cSpecSerialNr, cDeviceSerialNr);
			}
			printf ("Please enter the device number to open: ");
			ch = _getch();
			dwDeviceNum = ch-48;
			if (dwDeviceNum >= dwNumDevices)
			{
				printf ("\n\nInvalid device number!\nPress any key to continue...");
				_getch();
				return;
			}
			dwError = JETI_OpenRadio (dwDeviceNum, &dwDevice);
			if (dwError != 0)
			{
				printf ("Could not open device!\nError code 0x%08X\nPress any key to continue...", dwError);
				_getch();
				return;
			}
		}
		else							// One device found
		{
			dwError = JETI_OpenRadio (0, &dwDevice);
			if (dwError != 0)
			{
				printf ("Could not open device!\nError code 0x%08X\nPress any key to continue...", dwError);
				_getch();
				return;
			}
		}
	}

	
	do
	{
		system ("cls");
		printf ("Please select:\n\n");
		printf ("m) perform radiometric measurement...\n\n");
		printf ("0) exit\n\n");
		ch = _getch();
		switch (ch)
		{
			case 'm':
					system ("cls");
					// start measurement
					PerformMeasurement ();
					_getch();
				break;
			default:
					system ("cls");
				break;
		}
	} while (ch != '0');
	JETI_CloseRadio (dwDevice);

	return;
}


int PerformMeasurement (void)
{
	DWORD	dwResult;
	FLOAT	fFlickerFreq;
	DWORD	dwWarning;
	FLOAT	fSyncFreq;
	BOOL	boStatus;
	FLOAT	fRadio;
	FLOAT	fPrevTint;
	FLOAT	fConfTint;

	
	// try to determine flicker frequency automatically
	printf ("Try to determine flicker frequency...\n");
	dwResult = JETI_GetFlickerFreq(dwDevice, &fFlickerFreq, &dwWarning);

	if ((fFlickerFreq == 0.0) || (dwResult != 0))
	{
		printf("Could not determine flicker frequency!\nPlease enter a sync frequency in Hz: ");
		scanf_s("%f", &fSyncFreq);
	}
	else
		fSyncFreq = fFlickerFreq;
	
	// set sync mode
	JETI_SetSyncMode(dwDevice, TRUE);

	// set sync frequency in Hz
	JETI_SetSyncFreq(dwDevice, fSyncFreq);
	
	// start measurement
	dwError = JETI_Measure (dwDevice);
	if (dwError != 0)
	{
		printf ("Could not start measurement!\nError code 0x%08X\nPress any key to continue...", dwError);
		return -1;
	}
	printf ("Measurement started. Please wait...\n");

	// wait until measurement is finished
	do
	{
		dwError = JETI_MeasureStatus (dwDevice, &boStatus);
		if (dwError != 0)
		{
			printf ("Could not determine measurement status!\nError code 0x%08X\nPress any key to continue...", dwError);
			return -1;
		}
		printf (".");
		Sleep (100);

	} while (boStatus);
	printf ("\n\n");
	if (dwError)
		return -1;

	// read radiometric value
	JETI_Radio (dwDevice, &fRadio);
	printf ("radiometric value:\t%.3E\n\n", fRadio);

	// show info about the sync frequency
	printf ("sync frequency [Hz]:\t%.2f\n\n", fSyncFreq);
	
	printf ("Press any key to continue...");
	
	// set sync mode back to use integration time in ms
	JETI_SetSyncMode(dwDevice, FALSE);
	
	return 0;
}