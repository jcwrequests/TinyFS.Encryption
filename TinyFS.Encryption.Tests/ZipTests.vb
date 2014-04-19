Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class ZipTests

    Private _tinyFile = "./tiny.fs"
    Private _zip = "./tiny.zip"
    <TestInitialize>
    Public Sub SET_UP()
        Using tiny As New TinyFS.EmbeddedStorage(_tinyFile)
            Dim fileInfo = tiny.CreateFile("TEST")
            Dim value = "This is a test"
            Dim buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(value.ToCharArray)
            tiny.Write(fileInfo, buffer, 0, buffer.Length)
        End Using
    End Sub
    <TestMethod()> Public Sub CREATE_ZIP_WITH_TINY_FS_FILE()
        Using archive As New Ionic.Zip.ZipFile(fileName:=_zip)
            archive.AddFile(_tinyFile, String.Empty)
            archive.Save()
        End Using

    End Sub

End Class