﻿Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI
Imports PSMoveServiceExCAPI.PSMoveServiceExCAPI.Constants

Module Tests
    Dim mControllers As Controllers() = Nothing
    Dim mTrackers As Trackers() = Nothing

    Dim sHost As String = "127.0.0.1"

    Sub Main()
        Try
            While True
                Dim sLine As String

                Console.WriteLine("Listen to [T]racker or [C]ontroller:")
                sLine = Console.ReadLine()

                Select Case (sLine.ToLowerInvariant)
                    Case "t"
                        Console.WriteLine("Tracker to listen [0-7]:")
                        sLine = Console.ReadLine()

                        If (String.IsNullOrEmpty(sLine)) Then
                            Return
                        End If

                        Dim iNum As Integer
                        If (Not Integer.TryParse(sLine, iNum)) Then
                            Return
                        End If

                        Console.WriteLine("Enter remote IP (leave blank for localhost): ")
                        sLine = Console.ReadLine()

                        If (Not String.IsNullOrEmpty(sLine)) Then
                            sHost = sLine
                        End If

                        DoTrackers(iNum)
                    Case "c"
                        Console.WriteLine("Controller to listen [0-9]:")
                        sLine = Console.ReadLine()

                        If (String.IsNullOrEmpty(sLine)) Then
                            Return
                        End If

                        Dim iNum As Integer
                        If (Not Integer.TryParse(sLine, iNum)) Then
                            Return
                        End If

                        Console.WriteLine("Enter remote IP (leave blank for localhost): ")
                        sLine = Console.ReadLine()

                        If (Not String.IsNullOrEmpty(sLine)) Then
                            sHost = sLine
                        End If

                        DoControllers(iNum)
                End Select

                Console.ReadLine()
            End While

        Catch ex As Exception
            Console.WriteLine("ERROR: " & ex.Message)
            Threading.Thread.Sleep(5000)
        End Try
    End Sub

    Private Sub DoControllers(iListenController As Integer)
        Using mService As New Service(sHost)
            mService.Connect()

            ' Get new list of controllers
            mControllers = Controllers.GetControllerList()

            If (iListenController < 0 OrElse iListenController > mControllers.Length - 1) Then
                Throw New ArgumentException("Controller id out of range")
            End If

            If (mControllers IsNot Nothing) Then
                Dim mController As Controllers = mControllers(iListenController)

                ' Setup streaming flags we need.
                mController.m_DataStreamFlags =
                        PSMStreamFlags.PSMStreamFlags_includeCalibratedSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includePhysicsData Or
                        PSMStreamFlags.PSMStreamFlags_includePositionData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawSensorData Or
                        PSMStreamFlags.PSMStreamFlags_includeRawTrackerData

                ' Enable and start listening to the stream
                mController.m_Listening = True
                mController.m_DataStreamEnabled = True

                ' Start tracker data stream for this controller
                ' This is never needed unless you want to get the projection from the camera
                ' Only one tracker stream per controller is supported
                mController.SetTrackerStream(0)

                While True
                    ' Poll changes and refresh controller data
                    ' Use 'RefreshFlags' to optimize what you need to reduce API calls
                    mService.Update()

                    If (mService.HasControllerListChanged) Then
                        Throw New ArgumentException("Controller list has changed")
                    End If

                    mController.Refresh(Controllers.Info.RefreshFlags.RefreshType_All)

                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("Controller ID: " & mController.m_Info.m_ControllerId)
                    Console.WriteLine("m_ControllerType: " & mController.m_Info.m_ControllerType.ToString)
                    Console.WriteLine("m_ControllerHand: " & mController.m_Info.m_ControllerHand.ToString)

                    Console.WriteLine("m_ControllerSerial: " & mController.m_Info.m_ControllerSerial)
                    Console.WriteLine("m_ParentControllerSerial: " & mController.m_Info.m_ParentControllerSerial)
                    Console.WriteLine("IsControllerStable: " & mController.IsControllerStable())

                    If (mController.m_Info.IsStateValid()) Then
                        ' Get PSMove stuff
                        If (mController.m_Info.m_ControllerType = PSMControllerType.PSMController_Move) Then
                            Dim mPSMoveState = mController.m_Info.GetPSState(Of Controllers.Info.PSMoveState)

                            Console.WriteLine("m_TrackingColorType: " & mPSMoveState.m_TrackingColorType.ToString)

                            Console.WriteLine("m_TriangleButton: " & mPSMoveState.m_TriangleButton.ToString)
                            Console.WriteLine("m_CircleButton: " & mPSMoveState.m_CircleButton.ToString)
                            Console.WriteLine("m_CrossButton: " & mPSMoveState.m_CrossButton.ToString)
                            Console.WriteLine("m_SquareButton: " & mPSMoveState.m_SquareButton.ToString)
                            Console.WriteLine("m_SelectButton: " & mPSMoveState.m_SelectButton.ToString)
                            Console.WriteLine("m_StartButton: " & mPSMoveState.m_StartButton.ToString)
                            Console.WriteLine("m_PSButton: " & mPSMoveState.m_PSButton.ToString)
                            Console.WriteLine("m_MoveButton: " & mPSMoveState.m_MoveButton.ToString)
                            Console.WriteLine("m_TriggerButton: " & mPSMoveState.m_TriggerButton.ToString)

                            Console.WriteLine("m_BatteryValue: " & mPSMoveState.m_BatteryValue.ToString)
                            Console.WriteLine("m_TriggerValue: " & mPSMoveState.m_TriggerValue)

                            Console.WriteLine("m_bIsCurrentlyTracking: " & mPSMoveState.m_bIsCurrentlyTracking)
                            Console.WriteLine("m_IsPositionValid: " & mPSMoveState.m_IsPositionValid)
                            Console.WriteLine("m_IsOrientationValid: " & mPSMoveState.m_IsOrientationValid)
                        End If
                    End If

                    If (mController.m_Info.IsPoseValid()) Then
                        Console.WriteLine("m_Position.x: " & mController.m_Info.m_Pose.m_Position.x)
                        Console.WriteLine("m_Position.y: " & mController.m_Info.m_Pose.m_Position.y)
                        Console.WriteLine("m_Position.z: " & mController.m_Info.m_Pose.m_Position.z)

                        Console.WriteLine("m_Orientation.x: " & mController.m_Info.m_Pose.m_Orientation.x)
                        Console.WriteLine("m_Orientation.y: " & mController.m_Info.m_Pose.m_Orientation.y)
                        Console.WriteLine("m_Orientation.z: " & mController.m_Info.m_Pose.m_Orientation.z)
                        Console.WriteLine("m_Orientation.z: " & mController.m_Info.m_Pose.m_Orientation.w)
                    End If

                    If (mController.m_Info.IsSensorValid()) Then
                        Console.WriteLine("m_Gyroscope.x: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.x)
                        Console.WriteLine("m_Gyroscope.y: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.y)
                        Console.WriteLine("m_Gyroscope.z: " & mController.m_Info.m_PSCalibratedSensor.m_Gyroscope.z)

                        Console.WriteLine("m_Magnetometer.x: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.x)
                        Console.WriteLine("m_Magnetometer.y: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.y)
                        Console.WriteLine("m_Magnetometer.z: " & mController.m_Info.m_PSCalibratedSensor.m_Magnetometer.z)

                        Console.WriteLine("m_Accelerometer.x: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.x)
                        Console.WriteLine("m_Accelerometer.y: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.y)
                        Console.WriteLine("m_Accelerometer.z: " & mController.m_Info.m_PSCalibratedSensor.m_Accelerometer.z)
                    End If

                    If (mController.m_Info.IsTrackingValid()) Then
                        If (mController.m_Info.m_PSTracking.m_Shape = PSMShape.PSMShape_Ellipse) Then
                            Dim mProjection = mController.m_Info.m_PSTracking.GetTrackingProjection(Of Controllers.Info.PSTracking.PSMTrackingProjectionEllipse)

                            Console.WriteLine("mCenter.x: " & mProjection.mCenter.x)
                            Console.WriteLine("mCenter.y: " & mProjection.mCenter.y)
                        End If
                    End If

                    Threading.Thread.Sleep(100)

                    'GC.Collect()
                    'GC.WaitForPendingFinalizers()
                End While
            End If
        End Using
    End Sub

    Private Sub DoTrackers(iListenTracker As Integer)
        Using mService As New Service(sHost)
            mService.Connect()

            ' Get new list of controllers
            mTrackers = Trackers.GetTrackerList

            If (iListenTracker < 0 OrElse iListenTracker > mTrackers.Length - 1) Then
                Throw New ArgumentException("Controller id out of range")
            End If

            If (mTrackers IsNot Nothing) Then
                Dim mTracker As Trackers = mTrackers(iListenTracker)

                mTracker.Refresh(Trackers.Info.RefreshFlags.RefreshType_Init)

                While True
                    ' Poll changes and refresh tracker data
                    ' Use 'RefreshFlags' to optimize what you need to reduce API calls
                    mService.Update()

                    If (mService.HasTrackerListChanged) Then
                        Throw New ArgumentException("Tracker list has changed")
                    End If

                    ' Tracker pose does not update with stream,
                    If (mService.HasPlayspaceOffsetChanged) Then
                        mTracker.Refresh(Trackers.Info.RefreshFlags.RefreshType_Init)
                        Console.WriteLine("Playsapce offsets have changed")
                    End If

                    Console.WriteLine(" --------------------------------- ")
                    Console.WriteLine("Tracker ID: " & mTracker.m_Info.m_TrackerId)
                    Console.WriteLine("m_TrackerType: " & mTracker.m_Info.m_TrackerType.ToString)
                    Console.WriteLine("m_TrackerDrvier: " & mTracker.m_Info.m_TrackerDrvier.ToString)
                    Console.WriteLine("m_DevicePath: " & mTracker.m_Info.m_DevicePath)
                    Console.WriteLine("m_SharedMemoryName: " & mTracker.m_Info.m_SharedMemoryName)

                    If (mTracker.m_Info.IsPoseValid()) Then
                        Console.WriteLine("m_Position.x: " & mTracker.m_Info.m_Pose.m_Position.x)
                        Console.WriteLine("m_Position.y: " & mTracker.m_Info.m_Pose.m_Position.y)
                        Console.WriteLine("m_Position.z: " & mTracker.m_Info.m_Pose.m_Position.z)

                        Console.WriteLine("m_Orientation.x: " & mTracker.m_Info.m_Pose.m_Orientation.x)
                        Console.WriteLine("m_Orientation.y: " & mTracker.m_Info.m_Pose.m_Orientation.y)
                        Console.WriteLine("m_Orientation.z: " & mTracker.m_Info.m_Pose.m_Orientation.z)
                        Console.WriteLine("m_Orientation.w: " & mTracker.m_Info.m_Pose.m_Orientation.w)
                    End If

                    If (mTracker.m_Info.IsViewValid()) Then
                        Console.WriteLine("tracker_screen_dimensions.x: " & mTracker.m_Info.m_View.tracker_screen_dimensions.x)
                        Console.WriteLine("tracker_screen_dimensions.y: " & mTracker.m_Info.m_View.tracker_screen_dimensions.y)
                    End If

                    Threading.Thread.Sleep(1000)

                    'GC.Collect()
                    'GC.WaitForPendingFinalizers()
                End While
            End If
        End Using
    End Sub
End Module
