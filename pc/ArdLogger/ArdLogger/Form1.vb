Imports System.IO.Ports
Imports System.Threading
Imports System.IO



Public Class Form1


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For i = 1 To 99
            ComboBox1.Items.Add("COM" & i)
        Next

        'Button2.Enabled = False
        disableSerialUi()

        loadConfig(Application.StartupPath & "\default.cfg")


        'config = New AnyConfig()
        'set the document path\name
        'config.cfgFile = "app.config"
        'retrieve and output document values
        'textBox1.Text = "Sample output from "+ config.cfgFile+":\r\n";
        'textBox1.Text+= config.GetValue("//appSettings//add[@key='one']");
        'retrieve and output another document values
        'config.cfgFile = "..\\..\\my.config";
        'textBox1.Text += "\r\n\r\nNow output from "+ 

        serialPort = New SerialPort()

        System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False

        SaveFileDialog1.Filter = "csv | *.csv"

        TextBox2.Text = Application.StartupPath & "\logger.csv"

        Label7.Text = TextBox5.Text

        readThread.Start()
    End Sub



    Dim serialPort As SerialPort
    Dim readThreadExit As Boolean
    Dim readThreadEnable As Boolean
    Dim readThreadToCsv As Boolean
    Dim readThread As New Thread(AddressOf Read)
    Dim message As String
    Dim sw As StreamWriter
    Dim csvTime As Double
    Dim csvTimePrev As Double
    Dim status As Integer
    Dim value As Double
    'Dim bConv As Boolean



    Sub Read()
        While Not readThreadExit
            If readThreadEnable Then
                Try
                    Dim message As String = serialPort.ReadLine()
                    'Console.WriteLine(message)

                    If message.StartsWith("#K=") Then
                        value = message.Split("=")(1)
                        NumericUpDown1.Value = value
                    End If

                    If message.StartsWith("#L=") Then
                        value = message.Split("=")(1)
                        NumericUpDown2.Value = value
                    End If

                    If message.StartsWith("#INPUT=") Then
                        value = message.Split("=")(1)
                        value = Math.Round(TextBox3.Text + (TextBox4.Text - TextBox3.Text) * (value / 1023), 2)
                        TextBox6.Text = value

                        value = message.Split("=")(1)
                        value = Math.Round(value * 5 / 1023, 2)
                        TextBox7.Text = value
                    End If

                    If message.StartsWith("#S") Then
                        status = 0
                        ToolStripStatusLabel3.Text = "PRIPRAVENO"
                        'ToolStripStatusLabel1.Text = "-"


                        Button5.Enabled = True
                        Button7.Enabled = True
                        Button11.Enabled = True
                        Button12.Enabled = True
                    End If

                    If readThreadToCsv Then

                        If InStr(message, "#S") > 0 Then
                            readThreadToCsv = False
                            sw.Close()
                        ElseIf InStr(message, "#") Then
                            RichTextBox1.AppendText(message + vbNewLine + vbNewLine)
                            RichTextBox1.ScrollToCaret()
                        Else

                            Integer.TryParse(message, value)
                            value = TextBox3.Text + (TextBox4.Text - TextBox3.Text) * (value / 1023)

                            sw.WriteLine(csvTime & "; " & value)
                            csvTime = csvTime + 0.05
                            'Console.WriteLine(csvTime)
                            If csvTime >= 25.0 + csvTimePrev Then
                                ToolStripStatusLabel1.Text = Math.Round(csvTime) & " ms"
                                csvTimePrev = csvTime
                            End If
                        End If
                    End If

                    'If status <> 2 Or CheckBox1.Checked Then
                    If status <> 2 Then
                        RichTextBox1.AppendText(message + vbNewLine + vbNewLine)
                        RichTextBox1.ScrollToCaret()

                    End If

                    If message.StartsWith("#R") Then
                        If status = 0 Then
                            ToolStripStatusLabel3.Text = "ZAZNAM"
                            ToolStripStatusLabel1.Text = "-"
                            csvTime = 0

                            Button5.Enabled = False
                            Button7.Enabled = False
                            Button12.Enabled = False
                        End If
                        status = 1
                    End If
                    If message.StartsWith("#D") Then


                        readThreadToCsv = True
                        If status = 0 Then
                            csvTime = 0

                            csvTimePrev = 0
                            ToolStripStatusLabel1.Text = csvTime & " ms"

                            ToolStripStatusLabel3.Text = "PRENOS"

                            Button5.Enabled = False
                            Button7.Enabled = False
                            Button11.Enabled = False
                            Button12.Enabled = False
                        End If

                        status = 2

                        'serialPort.ReadLine()
                        'serialPort.ReadLine()
                        'message = serialPort.ReadLine()
                    End If

                    If message.StartsWith("#C") Then
                        status = 3

                        'readThreadToCsv = True
                        csvTime = 0
                        csvTimePrev = 0
                        ToolStripStatusLabel1.Text = csvTime & " ms"

                        ToolStripStatusLabel3.Text = "KONVERZE"

                        Button5.Enabled = False
                        Button7.Enabled = False
                        Button11.Enabled = False
                        Button12.Enabled = False

                        'serialPort.ReadLine()
                        'serialPort.ReadLine()
                        'message = serialPort.ReadLine()
                    End If

                    If message.StartsWith("#F") Then
                        status = 3
                        ToolStripStatusLabel3.Text = "FORMATOVANI"
                        ToolStripStatusLabel1.Text = "-"
                    End If

                    If message.StartsWith("#E") Then
                        status = 15
                        ToolStripStatusLabel3.Text = "CHYBA"
                        ToolStripStatusLabel1.Text = "-"


                        Button5.Enabled = False
                        Button7.Enabled = False
                        Button11.Enabled = False
                        Button12.Enabled = False
                        'Else
                        '   ToolStripStatusLabel3.Text = "PRIPRAVENO"
                    End If

                    If message.StartsWith("#W") Then
                        status = 15
                        ToolStripStatusLabel3.Text = "VAROVANI"
                        ToolStripStatusLabel1.Text = "-"


                        Button5.Enabled = False
                        Button7.Enabled = False
                        Button11.Enabled = False
                        Button12.Enabled = False
                    End If


                Catch generatedExceptionName As TimeoutException
                Catch ex As Exception
                    MsgBox("READ: " & ex.Message)
                    readThreadEnable = False
                    disableSerialUi()
                    status = 0
                    serialPort.Close()
                End Try
            End If
        End While
    End Sub

    Sub disableSerialUi()
        Button1.Enabled = True
        Button2.Enabled = False
        Button3.Enabled = False
        Button4.Enabled = False
        Button5.Enabled = False
        Button6.Enabled = False
        Button7.Enabled = False
        Button8.Enabled = False
        Button11.Enabled = False
        Button12.Enabled = False
        Button14.Enabled = False
        Button15.Enabled = False
        Button16.Enabled = False
        ToolStripStatusLabel1.Text = "-"
        ToolStripStatusLabel3.Text = "-"
    End Sub

    Sub enableSerialUi()
        Button1.Enabled = False
        Button2.Enabled = True
        Button3.Enabled = True
        Button4.Enabled = True
        Button5.Enabled = True
        Button6.Enabled = True
        Button7.Enabled = True
        Button8.Enabled = True
        Button11.Enabled = True
        Button12.Enabled = True
        Button14.Enabled = True
        Button15.Enabled = True
        Button16.Enabled = True
        ToolStripStatusLabel1.Text = "-"
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Create a new SerialPort object with default settings.
        RichTextBox1.Clear()

        ' Allow the user to set the appropriate properties.
        serialPort.PortName = ComboBox1.Text '"COM2" 'SetPortName(_serialPort.PortName)
        serialPort.BaudRate = 115200 'SetPortBaudRate(_serialPort.BaudRate)
        'serialPort.BaudRate = 9600
        serialPort.Parity = Parity.None 'SetPortParity(_serialPort.Parity)
        serialPort.DataBits = 8 'SetPortDataBits(_serialPort.DataBits)
        serialPort.StopBits = 1 'SetPortStopBits(_serialPort.StopBits)
        serialPort.Handshake = Handshake.None 'SetPortHandshake(_serialPort.Handshake)
        serialPort.DtrEnable = False


        ' Set the read/write timeouts
        serialPort.ReadTimeout = 1000
        serialPort.WriteTimeout = 1000

        '_serialPort.Open()

        Try
            serialPort.Open()
            enableSerialUi()
            readThreadEnable = True
            ToolStripStatusLabel2.Text = "PRIPOJENO"

            System.Threading.Thread.Sleep(1000)
            serialPort.WriteLine("?")

        Catch ex As Exception
            MsgBox(ex.Message) 'Error: Serial Port read timed out."
        Finally
            'If _serialPort IsNot Nothing Then _serialPort.Close()
        End Try

        'Button1.Enabled = False
        'Button2.Enabled = True



        'serialPort.WriteLine("?")

        'readThread.Start()

        'Console.Write("Name: ")
        'Name = Console.ReadLine()

        'Console.WriteLine("Type QUIT to exit")

        'While _continue
        'message = Console.ReadLine()
        '
        'If stringComparer__1.Equals("quit", message) Then
        '_continue = False
        'Else
        '_serialPort.WriteLine([String].Format("<{0}>: {1}", Name, message))
        'End If
        'End While
        'Dim message As String = _serialPort.ReadLine()
        ' Console.WriteLine(message)

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        readThreadEnable = False
        System.Threading.Thread.Sleep(1000)
        serialPort.Close()

        'Button1.Enabled = True
        'Button2.Enabled = False
        disableSerialUi()

        ToolStripStatusLabel2.Text = "ODPOJENO"
        ToolStripStatusLabel3.Text = "-"
    End Sub


    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        readThreadExit = True
        readThread.Join()
        If serialPort.IsOpen Then
            serialPort.Close()
            System.Threading.Thread.Sleep(1000)
        End If

        Try
            sw.Close()
        Catch ex As Exception
        End Try

        'readThread.Abort()

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("f")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("s")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("r")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("e")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Dim message As String ' = _serialPort.ReadLine()
        Try

            'sw = New StreamWriter("C:\logger.csv")
            Try
                sw.Close()
            Catch ex As Exception
            End Try


            sw = New StreamWriter(TextBox2.Text)

            RichTextBox1.Clear()

            message = "CAS [ms]; " & TextBox5.Text 'VALUE [" & TextBox5.Text & "]"
            sw.WriteLine(message)

            'readThreadToCsv = True
            'csvTime = 0
            'csvTimePrev = 0
            'ToolStripStatusLabel1.Text = csvTime & " ms"
            serialPort.WriteLine("d")




            'While True
            'message = serialPort.ReadLine()
            'If InStr(message, "Done") > 0 Then
            'Console.WriteLine("EXIT")
            'Exit While
            'End If
            'sw.Write(message)
            'Console.WriteLine("CSV: " & message)
            'End While

            'sw.Close()

            'readThreadToCsv = False

        Catch ex As Exception
            readThreadToCsv = False
            MessageBox.Show("CSV: " & ex.Message)
        End Try
    End Sub

    Private Sub ToolStripStatusLabel1_Click(sender As Object, e As EventArgs) Handles ToolStripStatusLabel1.Click

    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Sub sendSerial()
        If CheckBox1.Checked Then
            RichTextBox1.Clear()
        End If
        Try
            serialPort.WriteLine(TextBox1.Text)
            RichTextBox1.AppendText(">" & TextBox1.Text & vbNewLine)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        sendSerial()
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        RichTextBox1.Clear()

    End Sub

    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs)

    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        SaveFileDialog1.FileName = TextBox2.Text
        SaveFileDialog1.Filter = "csv | *.csv"
        If SaveFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            TextBox2.Text = SaveFileDialog1.FileName
        End If

    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Try
            System.Diagnostics.Process.Start(TextBox2.Text)

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If status = 1 Or status = 3 Or status = 4 Then
            ToolStripStatusLabel1.Text = csvTime & " ms"
            csvTime = csvTime + Timer1.Interval
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged

    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click
        SaveFileDialog2.Filter = "cfg | *.cfg"
        If SaveFileDialog2.FileName = "" Then
            SaveFileDialog2.FileName = Application.StartupPath & "\default.cfg"
        End If
        If SaveFileDialog2.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                Dim swConfig = New StreamWriter(SaveFileDialog2.FileName)
                swConfig.WriteLine("port=" & ComboBox1.Text)
                swConfig.WriteLine("file=" & TextBox2.Text)
                swConfig.WriteLine("x0=" & TextBox3.Text)
                swConfig.WriteLine("x100=" & TextBox4.Text)
                swConfig.WriteLine("label=" & TextBox5.Text)
                swConfig.Close()
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Sub loadConfig(fileName As String)
        Try
            Dim swConfig = New StreamReader(fileName)
            Dim line As String
            Dim items As String()

            While swConfig.Peek() >= 0
                line = swConfig.ReadLine()
                items = line.Split("=")

                If items(0) = "port" Then
                    ComboBox1.Text = items(1)
                End If
                If items(0) = "file" Then
                    TextBox2.Text = items(1)
                End If
                If items(0) = "x0" Then
                    TextBox3.Text = items(1)
                End If
                If items(0) = "x100" Then
                    TextBox4.Text = items(1)
                End If
                If items(0) = "label" Then
                    TextBox5.Text = items(1)
                End If
            End While


            swConfig.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        OpenFileDialog2.Filter = "cfg | *.cfg"
        OpenFileDialog2.FileName = Application.StartupPath & "\default.cfg"
        If OpenFileDialog2.FileName = "" Then
            OpenFileDialog2.FileName = Application.StartupPath & "\default.cfg"
        End If
        If OpenFileDialog2.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            loadConfig(OpenFileDialog2.FileName)
        End If

    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("c")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        If CheckBox1.Checked Then
            RichTextBox1.Clear()
        End If
        Try
            serialPort.WriteLine("?")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("?")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub TextBox5_Leave(sender As Object, e As EventArgs) Handles TextBox5.Leave
        Label7.Text = TextBox5.Text
    End Sub

    Private Sub ConsoleToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConsoleToolStripMenuItem.Click
        Panel1.Visible = Not Panel1.Visible

    End Sub


    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        Try
            serialPort.WriteLine("k" & Chr(NumericUpDown1.Value))
            Threading.Thread.Sleep(50)
            serialPort.WriteLine("l" & Chr(NumericUpDown2.Value))
            Threading.Thread.Sleep(50)
            serialPort.WriteLine("?")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        OpenFileDialog2.FileName = TextBox10.Text
        OpenFileDialog2.Filter = "bin | *.bin"
        If OpenFileDialog2.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            TextBox10.Text = OpenFileDialog2.FileName
            TextBox11.Text = Application.StartupPath & "\" + Path.GetFileNameWithoutExtension(TextBox10.Text) & ".csv"

            File.Delete(TextBox11.Text & ".tmp")
            Process.Start("bintocsv.exe", """" & TextBox10.Text & """ """ & TextBox11.Text & ".tmp""")
            Threading.Thread.Sleep(1000)

            'Dim time As Double
            csvTime = 0

            'status = 4
            'ToolStripStatusLabel3.Text = "SD CONVERTING"
            'ToolStripStatusLabel1.Text = "-"

            'Dim tempfile = Path.GetTempFileName()
            Try
                Using sw = New StreamWriter(TextBox11.Text)
                    Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(TextBox11.Text & ".tmp")
                        MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
                        MyReader.Delimiters = New String() {","}
                        Dim readRow As String()
                        Dim writeRow(2) As String

                        'head
                        readRow = MyReader.ReadFields()
                        readRow = MyReader.ReadFields()
                        writeRow(0) = "CAS [ms]"
                        writeRow(1) = TextBox5.Text
                        sw.WriteLine(String.Join(";", writeRow))

                        While Not MyReader.EndOfData
                            Try
                                readRow = MyReader.ReadFields()
                                If readRow.Count >= 1 Then
                                    If readRow(0) <> "Overruns" Then
                                        writeRow(1) = TextBox3.Text + (TextBox4.Text - TextBox3.Text) * (Integer.Parse(readRow(0)) / 1023)
                                        'Else
                                        'writeRow(1) = 0
                                    End If
                                    writeRow(0) = csvTime
                                    csvTime = Math.Round(csvTime + 0.05, 2)
                                    sw.WriteLine(String.Join(";", writeRow))
                                End If

                            Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                                MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                            Catch ex As Exception
                                MsgBox(ex.Message)
                                Exit While
                            End Try

                        End While
                    End Using
                End Using
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
            'File.Delete(inputFile)
            'File.Move(tempfile, inputFile)
            'status = 0
            'ToolStripStatusLabel3.Text = "SD CONVERTED"
            'ToolStripStatusLabel1.Text = "-"

        End If
    End Sub


    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        Try
            System.Diagnostics.Process.Start(TextBox11.Text)

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        RichTextBox1.Clear()
        Try
            serialPort.WriteLine("?")
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
End Class


