'copy at least jeti_radio_ex64.dll and jeti_core64.dll to C:\Windows\system32 directory before running this program
Private Declare PtrSafe Function JETI_GetNumRadioEx& Lib "jeti_radio_ex64.dll" (ByRef NumDevices As Long)
Private Declare PtrSafe Function JETI_OpenRadioEx& Lib "jeti_radio_ex64.dll" (ByVal dwDeviceNum As Long, ByRef dwDevice As LongPtr)
Private Declare PtrSafe Function JETI_CloseRadioEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr)
Private Declare PtrSafe Function JETI_MeasureEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr, ByVal fTint As Single, ByVal wAver As Integer, ByVal dwStep As Long)
Private Declare PtrSafe Function JETI_MeasureStatusEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr, ByRef bolsBusy As Long)
Private Declare PtrSafe Function JETI_SpecRadEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr, ByVal dwBeg As Long, ByVal dwEnd As Long, ByRef fSprad As Single)
Private Declare PtrSafe Function JETI_PhotoEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr, ByRef fPhoto As Single)
Private Declare PtrSafe Function JETI_ChromxyEx& Lib "jeti_radio_ex64.dll" (ByVal dwDevice As LongPtr, ByRef fChromx As Single, ByRef fChromy As Single)

Dim dwNumDevices As Long
Dim LibError As Long
Dim dwDevice As LongPtr
Dim boStatus As Long
Dim fSprad(0 To 400) As Single          '380 - 780nm, 1nm step => 401 values
Dim fPhoto As Single
Dim Chromx As Single
Dim Chromy As Single

Sub Measure_OnClick()

    '--------------------------------------------------------------------------------------------------------------
    'Get the count of connected devices
    LibError = JETI_GetNumRadioEx(dwNumDevices)
    If dwNumDevices < 1 Or LibError <> 0 Then
        Result = MsgBox("No device found!", vbCritical + vbOKOnly, "ERROR")
        GoTo EndSub
    End If

    '--------------------------------------------------------------------------------------------------------------
    'Open the first found device
    LibError = JETI_OpenRadioEx(0, dwDevice)
    If LibError <> 0 Then
        Result = MsgBox("Error opening the device!", vbCritical + vbOKOnly, "ERROR")
        GoTo EndSub
    End If
        
    '--------------------------------------------------------------------------------------------------------------
    'Start radiometric measurement with automatic adapted integration time, average 1 and 1nm step-width
        LibError = JETI_MeasureEx(dwDevice, 0, 1, 1)
        
    ' Wait if if the measurement has finished.
    Do
        LibError = JETI_MeasureStatusEx(dwDevice, boStatus)
        If LibError <> 0 Then
            MsgBox Err.Description
            GoTo EndSub
        End If
    Loop While boStatus
        
        'Read measurement values
        ' spectral data
        LibError = JETI_SpecRadEx(dwDevice, 380, 780, fSprad(0))
        
        'photometric value Y
        LibError = JETI_PhotoEx(dwDevice, fPhoto)
        
        'Chromaticity coordinates xy
        LibError = JETI_ChromxyEx(dwDevice, fChromx, fChromy)
        
    '--------------------------------------------------------------------------------------------------------------
    'Close the device
        LibError = JETI_CloseRadioEx(dwDevice)


EndSub:
End Sub
