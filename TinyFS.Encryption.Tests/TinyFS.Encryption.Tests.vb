Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class TinyTests
    Private sourceFile = "./test.txt"
    Private tinyFile = "./test.tiny"
    Private keyValues As KeyValues

    <TestInitialize>
    Public Sub SETUP_TEST()
        If IO.File.Exists(sourceFile) Then IO.File.Delete(sourceFile)
        If IO.File.Exists(tinyFile) Then IO.File.Delete(tinyFile)

        Using writer = IO.File.CreateText(sourceFile)
            writer.WriteLine("This is a test")
        End Using

        Dim keyGenerator As New KeyGenerator()
        keyValues = keyGenerator.CreateKeyValues("password", "username")
    End Sub
    <TestCleanup>
    Public Sub CLEAN_UP()
        If IO.File.Exists(sourceFile) Then IO.File.Delete(sourceFile)
        If IO.File.Exists(tinyFile) Then IO.File.Delete(tinyFile)
    End Sub
    <TestMethod()>
    Public Sub ENCRYPT_AND_STORE_TEST_FILE_IN_A_TINY_FS_FILE()
        Dim encryptor As New AESEncryption(keyValues.IV, keyValues.KEY)

        Using soureStream = IO.File.OpenRead(sourceFile)
            Dim buffer(soureStream.Length - 1) As Byte
            soureStream.Read(buffer, 0, buffer.Length)

            Dim encyptedData = encryptor.Encrypt(buffer)
            Using tiny As New TinyFS.EmbeddedStorage(tinyFile)
                Dim info = tiny.CreateFile("Test")
                tiny.Write(info, encyptedData, 0, encyptedData.Length)
            End Using
        End Using
    End Sub
    <TestMethod>
    Public Sub RETRIEVE_STORED_FILE_AND_DECRYPT_FROM_TINY_FS_FILE()
        ENCRYPT_AND_STORE_TEST_FILE_IN_A_TINY_FS_FILE()

        Dim encryptor As New AESEncryption(keyValues.IV, keyValues.KEY)
        Dim results As String = String.Empty

        Using tiny As New TinyFS.EmbeddedStorage(tinyFile)
            Dim sourceData = tiny.Read("Test")
            Dim decryptedData = encryptor.Decrypt(sourceData)
            results = System.Text.ASCIIEncoding.ASCII.GetString(decryptedData)
        End Using

        Assert.IsTrue(results.StartsWith("This is a test"))
    End Sub
End Class