Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class ConfigTests

    <TestMethod()> Public Sub CREATE_CONFIGURATION()
        Dim config = StorageConfigBuilder.
            CreateDefault().
            SetFileName("./test.tiny").
            SetEncryptorType(Of AESEncryption)().
            SetKeyGenerator(New KeyGenerator()).
            SetUserName("userName").
            SetPassword("password").
            CreateConfig()
    End Sub

End Class
Public Interface IConfig
    ReadOnly Property Encryptor As IEncryptor
    ReadOnly Property FileName As String
End Interface
Public Class StorageConfiguration
    Implements IConfig
    Private _encryptor As IEncryptor
    Private _fileName As String
    Public Sub New(encryptor As IEncryptor, fileName As String)
        Me._encryptor = encryptor
        Me._fileName = fileName
    End Sub
    Public ReadOnly Property Encryptor As IEncryptor Implements IConfig.Encryptor
        Get
            Return _encryptor
        End Get
    End Property

    Public ReadOnly Property FileName As String Implements IConfig.FileName
        Get
            Return _fileName
        End Get
    End Property
End Class
Public Class StorageConfigBuilder
    Private keyGenerator As IKeyGenerator
    Private encryptor As Type
    Private fileName As String
    Private keyValues As KeyValues
    Private password As String
    Private userName As String
    Private Sub New()

    End Sub
    Public Shared Function CreateDefault() As StorageConfigBuilder
        Return New StorageConfigBuilder()
    End Function
    Public Function SetFileName(fileName As String) As StorageConfigBuilder
        Me.fileName = fileName
        Return Me
    End Function
    Public Function SetKeyGenerator(generator As IKeyGenerator) As StorageConfigBuilder
        Me.keyGenerator = generator
        Return Me
    End Function
    Public Function SetEncryptorType(Of TEncryptor As IEncryptor)() As StorageConfigBuilder
        Me.encryptor = GetType(TEncryptor)
        Return Me
    End Function
    Public Function SetUserName(username As String) As StorageConfigBuilder
        Me.userName = username
        Return Me
    End Function
    Public Function SetPassword(password As String) As StorageConfigBuilder
        Me.password = password
        Return Me
    End Function
    Public Function CreateConfig() As IConfig

    End Function
End Class