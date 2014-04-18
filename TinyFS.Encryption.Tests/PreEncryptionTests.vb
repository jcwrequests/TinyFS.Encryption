Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Security.Cryptography
Imports System.Runtime.InteropServices

<TestClass()> Public Class PreEncryptionTests
    Private outputFile As String = "./test.txt"
    <TestInitialize>
    Public Sub Setup()
        If IO.File.Exists(outputFile) Then IO.File.Delete(outputFile)
    End Sub
    <TestMethod()>
    Public Sub GENERATE_KEY_FROM_PASSWORD()
        Dim hashProvider As New SHA256Managed()
        Dim password As String = "PASSWORD"
        Dim passwordInBytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(password.ToCharArray)
        Dim salt() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes("salt".ToCharArray)

        Dim hashedPassword = hashProvider.ComputeHash(passwordInBytes)

    End Sub

    <TestMethod>
    Public Sub ENCRYPT_AN_IN_MEMORY_BYTE_ARRAY()
        Dim provider As New AesCryptoServiceProvider()
        provider.GenerateIV()
        provider.GenerateKey()
        Dim inputData = System.Text.ASCIIEncoding.ASCII.GetBytes("MY SECRECT".ToArray())


        Dim encryptor As ICryptoTransform = provider.CreateEncryptor(key:=provider.Key, iv:=provider.IV)
        Using memoryStream As New IO.MemoryStream()
            Using cryptoStream As New CryptoStream(stream:=memoryStream, transform:=encryptor, mode:=CryptoStreamMode.Write)
                Using writer As New IO.StreamWriter(cryptoStream)
                    writer.Write(inputData)
                    writer.Flush()
                    cryptoStream.FlushFinalBlock()
                    writer.Flush()

                    Debug.WriteLine(Convert.ToBase64String(memoryStream.ToArray))
                End Using

            End Using

        End Using
    End Sub

    <TestMethod>
    Public Sub ENCRYPTION_AN_IN_MEMEORY_BYTE_ARRAY_USING_PASSWORD()
        Dim hashProvider As New SHA256Managed()
        Dim password As String = "PASSWORD"
        Dim user As String = "username"

        Dim passwordInBytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes((password.ToCharArray.Concat("salt".ToCharArray)).ToArray)
        Dim userInBytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes((user.ToCharArray.Concat("salt".ToCharArray)).ToArray)

        Dim hashedPassword = hashProvider.ComputeHash(passwordInBytes)

        Dim hasedUser = hashProvider.ComputeHash(userInBytes)

        Dim key = hashedPassword
        Dim iv = hasedUser.Take(16).ToArray()

        Dim provider As New AesCryptoServiceProvider()

        Dim inputData = System.Text.ASCIIEncoding.ASCII.GetBytes("MY SECRECT".ToArray())
        Dim outputData() As Byte

        Dim encryptor As ICryptoTransform = provider.CreateEncryptor(key:=key, iv:=iv)
        Using memoryStream As New IO.MemoryStream()
            Using cryptoStream As New CryptoStream(stream:=memoryStream, transform:=encryptor, mode:=CryptoStreamMode.Write)
                Using writer As New IO.StreamWriter(cryptoStream)
                    writer.Write(inputData)
                    writer.Flush()
                    cryptoStream.FlushFinalBlock()
                    writer.Flush()
                    outputData = memoryStream.ToArray
                End Using
            End Using
        End Using

        Debug.WriteLine(Convert.ToBase64String(outputData))

    End Sub

    <TestMethod>
    Public Sub ENCRYPT_AND_DECRYPT()
        Dim keyGenerator As New KeyGenerator()
        Dim values = keyGenerator.CreateKeyValues("password", "username")

        Dim encryptor As New AESEncryption(values.IV, values.KEY)

        Dim source As String = "this is a test"
        Dim sourceInBytes = Text.ASCIIEncoding.ASCII.GetBytes(source.ToCharArray)

        Dim encryptedResult = encryptor.Encrypt(sourceInBytes)
        Dim decryptedResult = encryptor.Decrypt(encryptedResult)
        Dim decryptedString = Text.ASCIIEncoding.ASCII.GetString(decryptedResult)

        Assert.IsTrue(source.Equals(decryptedString))

    End Sub

   
End Class

Public Class KeyGenerator
    Implements IKeyGenerator

    Private hashProvider As SHA256Managed
    Public Sub New()
        hashProvider = New SHA256Managed()
    End Sub
    Public Function CreateKeyValues(password As String, userName As String) As KeyValues Implements IKeyGenerator.CreateKeyValues
        Dim passwordInBytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes((password.ToCharArray.Concat("salt".ToCharArray)).ToArray)
        Dim userInBytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes((userName.ToCharArray.Concat("salt".ToCharArray)).ToArray)

        Dim hashedPassword = hashProvider.ComputeHash(passwordInBytes)

        Dim hasedUser = hashProvider.ComputeHash(userInBytes)

        Dim key = hashedPassword
        Dim iv = hasedUser.Take(16).ToArray()

        Return New KeyValues(iv, key)
    End Function
End Class
Public Class KeyValues
    Public Sub New(iv() As Byte, key() As Byte)
        Me.IV = iv
        Me.KEY = key
    End Sub
    Public Property IV As Byte()
    Public Property KEY As Byte()
End Class
Public Class AESEncryption
    Implements IDecrypt
    Implements IEncrypt
    Implements IEncryptor

    Private iv As New Security.SecureString
    Private key As New Security.SecureString
    Public Sub New(iv() As Byte, key() As Byte)
        iv.ToList.ForEach(Sub(b) Me.iv.AppendChar(Chr(b)))
        Me.iv.MakeReadOnly()
        key.ToList.ForEach(Sub(b) Me.key.AppendChar(Chr(b)))
        Me.key.MakeReadOnly()
    End Sub
    Public Function Encrypt(source() As Byte) As Byte() Implements IEncrypt.Encrypt, IEncryptor.Encrypt
        Dim result() As Byte
        Using aesProvider As New AesCryptoServiceProvider()

            Dim _iv = ExtractString(iv).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()
            Dim _key = ExtractString(key).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()

            Dim encryptor = aesProvider.CreateEncryptor(_key, _iv)
            result = Transform(source, encryptor)
        End Using

        Return result
    End Function
    Public Function Decrypt(source() As Byte) As Byte() Implements IDecrypt.Decrypt, IEncryptor.Decrypt
        Dim result() As Byte
        Using aesProvider As New AesCryptoServiceProvider()

            Dim _iv = ExtractString(iv).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()
            Dim _key = ExtractString(key).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()

            Dim encryptor = aesProvider.CreateDecryptor(_key, _iv)
            result = Transform(source, encryptor)
        End Using

        Return result
    End Function
    Public Function Transform(inputBytes() As Byte, provider As ICryptoTransform) As Byte()
        Dim result() As Byte

        Using inputStream As New IO.MemoryStream(inputBytes)
            Using outputStream As New IO.MemoryStream
                Using cryptStream As New CryptoStream(outputStream, provider, CryptoStreamMode.Write)
                    CType(inputStream, IO.Stream).CopyTo(cryptStream)
                    cryptStream.Close()
                    result = outputStream.ToArray()
                End Using
            End Using
        End Using
        Return result
    End Function
    Private Function ExtractString(source As Security.SecureString) As String
        Dim pointer = Marshal.SecureStringToBSTR(source)
        Dim value = Marshal.PtrToStringBSTR(pointer)
        Marshal.ZeroFreeBSTR(pointer)
        Return value
    End Function
End Class
Public Class AESFileEncryption
    Implements Interfaces
    Implements IEncryptFile

    Private iv As New Security.SecureString
    Private key As New Security.SecureString
    Private bufferLength As Integer

    Public Sub New(iv() As Byte, key() As Byte, Optional bufferLength As Integer = 4096)
        iv.ToList.ForEach(Sub(b) Me.iv.AppendChar(Chr(b)))
        Me.iv.MakeReadOnly()
        key.ToList.ForEach(Sub(b) Me.key.AppendChar(Chr(b)))
        Me.key.MakeReadOnly()
        Me.bufferLength = bufferLength
    End Sub

    Public Sub Encrypt(inputFileName As String, outPutFileName As String) Implements IEncryptFile.Encrypt

        Using aesProvider As New AesCryptoServiceProvider()
            Dim _iv = ExtractString(iv).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()
            Dim _key = ExtractString(key).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()

            Dim encryptor = aesProvider.CreateEncryptor(_key, _iv)
            TransformFile(inputFileName, outPutFileName, encryptor)
        End Using

    End Sub
    Public Sub Decrypt(inputFileName As String, outPutFileName As String) Implements Interfaces.Decrypt

        Using aesProviver As New AesCryptoServiceProvider()
            Dim _iv = ExtractString(iv).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()
            Dim _key = ExtractString(key).ToCharArray().Select(Function(v) CType(Asc(v), Byte)).ToArray()

            Dim descryptor = aesProviver.CreateDecryptor(_key, _iv)
            TransformFile(inputFileName, outPutFileName, descryptor)
        End Using

    End Sub
    Private Function ExtractString(source As Security.SecureString) As String
        Dim pointer = Marshal.SecureStringToBSTR(source)
        Dim value = Marshal.PtrToStringBSTR(pointer)
        Marshal.ZeroFreeBSTR(pointer)
        Return value
    End Function

    Private Sub TransformFile(inputFileName As String, outPutFileName As String, transformer As ICryptoTransform)

        Using inputStream As New IO.FileStream(inputFileName, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read, bufferLength)
            Using outputStream As New IO.FileStream(outPutFileName, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.None, bufferLength)
                Using cryptStream As New CryptoStream(outputStream, transformer, CryptoStreamMode.Write)
                    CType(inputStream, IO.Stream).CopyTo(cryptStream)
                    cryptStream.Close()
                End Using
            End Using
        End Using

    End Sub
End Class
