﻿Imports System.IO
Imports Pss.Cisco.Models

Public Class FrmSetup

    Dim validConfig As Boolean = False
    Private Sub FrmSetup_Load(sender As Object, e As EventArgs) Handles Me.Load
        'Dim MyPhone As New ClsPhone
        ' saves the user seting son click
        'MyPhoneSettings = MyPhone.DownloadPhoneSettings(TxtphoneIP.Text)
        'MyPhoneSettings.LocalPort = txtListeningIPport.Text
        'MyPhoneSettings.PhoneIP = TxtphoneIP.Text
        'MyPhoneSettings.PhonePort = txtPhonePort.Text
        'If MyPhoneSettings.CTI_Enable = "Yes" Then imgCTIEnabled.Image = Pss.Cisco.My.Resources.Resources.greentick
        'If MyPhoneSettings.DebugLevel = "full" Then imgDebugFull.Image = Pss.Cisco.My.Resources.Resources.greentick
        'If MyPhoneSettings.StationName <> vbLf Then imgStationSet.Image = Pss.Cisco.My.Resources.Resources.greentick
        'If MyPhoneSettings.LinksysKeySystem = "Yes" Then imgLinksysKeySystemEnabled.Image = Pss.Cisco.My.Resources.Resources.greentick

        validConfig = CheckConfig(False)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Btnsave.Click


        MyPhoneSettings.password = txtpassword.Text
        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\PssLinksys\Phone", "password", MyPhoneSettings.password)

        validConfig = CheckConfig(True)

        If validConfig = True Then
            MyPhoneSettings.LocalIP = CmbLocalIP.Text
            MyPhoneSettings.PhoneIP = TxtphoneIP.Text
            SetStoredSettings(MyPhoneSettings)

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Else
            MsgBox("Some settings are not valid.  Please correct before saving settings.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
        End If

    End Sub

    Private Sub btnImportCsv_Click(sender As Object, e As EventArgs) Handles btnImportCsv.Click

        OFDImport.Filter = "CSV files (*.csv)|*.csv"
        OFDImport.Title = "Select File to import phone list"
        If (OFDImport.ShowDialog() = DialogResult.OK) Then
            LoadCSVtoPhoneBook(OFDImport.FileName)
        End If
    End Sub

    Public Sub LoadCSVtoPhoneBook(filename As String)
        'loads the phone book from 'filenname'
        Dim TempPhoneBook As New List(Of PhoneBookEntry)

        ' Reader to read from the file
        Using sr As New StreamReader(filename)
            Dim tmp() As String
            ' Hold the amount of lines already read in a 'counter-variable'
            Do While sr.Peek <> -1 ' Is -1 when no data exists on the next line of the CSV file
                tmp = sr.ReadLine.Split(",".ToCharArray())
                Dim entry = New PhoneBookEntry
                With entry

                    .FirstName = StrConv(tmp(0).Trim, VbStrConv.ProperCase)
                    .Surname = StrConv(tmp(1).Trim, VbStrConv.ProperCase)
                    .Number = StrConv(tmp(2).Trim, VbStrConv.ProperCase)

                End With
            Loop
        End Using

        Try

            For each entry in TempPhoneBook
                MyPhoneBook.Add(entry)
            Next
            
        Catch ex As Exception
            ex.Log()
        End Try

        SavePhoneBook(Path.Combine(DataDir, "CiscoPhone\Phonebook.csv"))
        LoadPhoneBook(Path.Combine(DataDir, "CiscoPhone\Phonebook.csv"))
        Me.Close()

    End Sub

    Public Sub ExportPhoneBooktoCSV(filename As String)
        
        Try
            Using outFile = My.Computer.FileSystem.OpenTextFileWriter(filename, False)

                For Each entry In MyPhoneBook
                    With entry
                        If String.IsNullOrWhiteSpace(.DisplayName) _
                            OrElse String.IsNullOrWhiteSpace(.Number) Then Continue For
                        
                        outFile.WriteLine(.FirstName & "," & .Surname & "," & .Number)

                    End With
                Next
            End Using

        Catch ex As Exception
            ex.Log()
        End Try

        Close()

    End Sub

    Private Sub btnExportCsv_Click(sender As Object, e As EventArgs) Handles btnExportCsv.Click

        SFDExport.Filter = "CSV files (*.csv)|*.csv"
        SFDExport.Title = "Export phone list"
        If (SFDExport.ShowDialog() = DialogResult.OK) Then
            ExportPhoneBooktoCSV(SFDExport.FileName)
        End If
    End Sub


    Private Sub FrmSetup_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        Me.PaintGradient(e.Graphics)

    End Sub

    Private Sub btnRecheck_Click(sender As Object, e As EventArgs) Handles btnRecheck.Click
        validConfig = CheckConfig(True)
    End Sub

    Function CheckConfig(checkIPaddress As Boolean) As Boolean
        Dim ValidConfig = True
        If TxtphoneIP.Text <> "" Then
            If CmbLocalIP.Text <> "" Then
                Try
                    If My.Computer.Network.Ping(TxtphoneIP.Text) Then
                        imgCTIEnabled.Image = My.Resources.Cross
                        imgDebugFull.Image = My.Resources.Cross
                        imgStationSet.Image = My.Resources.Cross
                        lblStationNameSet.Text = "Station Name Set"
                        imgLinksysKeySystemEnabled.Image = My.Resources.Cross

                        
                        ClsPhone.password = LoginPassword
                        MyPhoneSettings = DownloadPhoneSettings(TxtphoneIP.Text)
                        MyPhoneSettings = DownloadPhoneSettings(TxtphoneIP.Text)
                        MyPhoneSettings.LocalPort = 514
                        MyPhoneSettings.PhoneIP = TxtphoneIP.Text
                        If MyPhoneSettings.CTI_Enable = "Yes" Then imgCTIEnabled.Image = My.Resources.Resources.greentick Else ValidConfig = False
                        If MyPhoneSettings.DebugLevel = "full" Then imgDebugFull.Image = My.Resources.Resources.greentick Else ValidConfig = False
                        If MyPhoneSettings.StationName <> vbLf And MyPhoneSettings.StationName <> "invalid" Then
                            imgStationSet.Image = My.Resources.Resources.greentick
                            lblStationNameSet.Text = "Station Name set (" & MyPhoneSettings.StationName & ")"
                        Else
                            ValidConfig = False
                        End If
                        If MyPhoneSettings.LinksysKeySystem = "Yes" Then imgLinksysKeySystemEnabled.Image = My.Resources.Resources.greentick Else ValidConfig = False
                        If MyPhoneSettings.Debug_Server_Address = vbLf Then
                            imgDebugServerSet.Image = My.Resources.Resources.Cross
                            LbldebugAddress.Text = "Debug server set"
                            ValidConfig = False
                        Else
                            If MyPhoneSettings.Debug_Server_Address = CmbLocalIP.Text Then
                                imgDebugServerSet.Image = Cisco.My.Resources.Resources.greentick
                                LbldebugAddress.Text = "Debug server matches PC address: " & MyPhoneSettings.Debug_Server_Address
                            Else
                                imgDebugServerSet.Image = Cisco.My.Resources.Resources.Cross
                                LbldebugAddress.Text = "Debug server setting incorrect, set to: " & MyPhoneSettings.Debug_Server_Address
                                ValidConfig = False
                            End If
                        End If
                    Else
                        MsgBox("No ping response from handset IP (" & TxtphoneIP.Text & ") - Failed to load data from handset", MsgBoxStyle.OkOnly, "SPA Call Control Pro")
                    End If
                Catch ex As Exception
                    MsgBox("Error: " & ex.Message, MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
                End Try
            Else

                MsgBox("Please enter or select which Local PC IP address to use before continuing", MsgBoxStyle.Exclamation, "SPA Call Control Pro")
                ValidConfig = False
                CmbLocalIP.Focus()
            End If



        Else
            If checkIPaddress = True Then
                MsgBox("Please enter a value in the Phone IP address field before continuing", MsgBoxStyle.Exclamation, "SPA Call Control Pro")
            End If
            ValidConfig = False
            TxtphoneIP.Focus()
        End If

        Return ValidConfig
    End Function

    Private Sub btnsharedFolder_Click(sender As Object, e As EventArgs) Handles btnsharedFolder.Click

        If TxtSharedFolder.Text <> "" Then
            SharedFBD.SelectedPath = TxtSharedFolder.Text
        Else
            SharedFBD.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        End If

        Dim result As DialogResult = SharedFBD.ShowDialog()

        If result = DialogResult.OK Then
            TxtSharedFolder.Text = SharedFBD.SelectedPath
            If Not TxtSharedFolder.Text.EndsWith("\") Then TxtSharedFolder.Text = TxtSharedFolder.Text & "\"
        End If

    End Sub

    Private Sub txtpassword_TextChanged(sender As Object, e As EventArgs) Handles txtpassword.TextChanged

        LoginPassword = txtpassword.Text

    End Sub
End Class