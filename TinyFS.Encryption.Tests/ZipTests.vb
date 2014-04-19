Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SevenZip

<TestClass()> Public Class ZipTests

    Private _tinyFile As String = "./tiny.fs"
    Private _zip As String = "./tiny.zip"
    <TestInitialize>
    Public Sub SET_UP()
        If IO.File.Exists(_tinyFile) Then IO.File.Delete(_tinyFile)
        If IO.File.Exists(_zip) Then IO.File.Delete(_zip)

        Using tiny As New TinyFS.EmbeddedStorage(_tinyFile)
            Dim fileInfo = tiny.CreateFile("TEST")
            Dim value = "This is a test"
            Dim buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(value.ToCharArray)
            tiny.Write(fileInfo, buffer, 0, buffer.Length)
        End Using
    End Sub
    <TestMethod()> Public Sub CREATE_IONIC_ZIP_WITH_TINY_FS_FILE()
        Using archive As New Ionic.Zip.ZipFile(fileName:=_zip)
            archive.AddFile(_tinyFile, String.Empty)
            archive.Save()
        End Using

    End Sub

    <TestMethod> Public Sub CREATE_SHARP_ZIP_FILE_WITH_TINY_FS_FILE()
        Using archive As New IO.FileStream(_zip, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.None)
            Dim storage As New ICSharpCode.SharpZipLib.Zip.ZipOutputStream(archive)
            Dim entry As New ICSharpCode.SharpZipLib.Zip.ZipEntry("test.tiny")
            'entry.CompressionMethod = ICSharpCode.SharpZipLib.Zip.CompressionMethod.
            storage.PutNextEntry(entry)
            Using fileSource As New IO.FileStream(_tinyFile, IO.FileMode.Open)
                Dim buffer(fileSource.Length - 1) As Byte
                fileSource.Read(buffer, 0, buffer.Length)
                storage.Write(buffer, 0, buffer.Length)
            End Using
            storage.CloseEntry()
            storage.IsStreamOwner = True
            storage.Close()
            'archive.Flush()
        End Using

    End Sub
End Class