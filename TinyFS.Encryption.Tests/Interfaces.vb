
Interface Interfaces
    Sub Decrypt(inputFileName As String, outPutFileName As String)
End Interface
Interface IEncryptFile
    Sub Encrypt(inputFileName As String, outPutFileName As String)
End Interface

Interface IDecrypt
    Function Decrypt(source() As Byte) As Byte()
End Interface
Interface IEncrypt
    Function Encrypt(source() As Byte) As Byte()
End Interface

Interface IKeyGenerator
    Function CreateKeyValues(password As String, userName As String) As KeyValues
End Interface

