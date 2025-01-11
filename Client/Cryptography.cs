using System.Security.Cryptography;
namespace Security{
class RSAEncryption{
    RSA rsa; 
    public RSAEncryption(){
        rsa = RSA.Create();
    }
    public byte[] Encrypt(byte[] data, byte[] publicKey){
        int bytesRead;
        rsa.ImportRSAPublicKey(publicKey,out bytesRead);
        byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        return encrypted;
    }
    public byte[] GetPublicKey(){
        return rsa.ExportRSAPublicKey();
    }
    public byte[] Decrypt(byte[] data){
        byte[] encrypted = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        return encrypted;
    }
}

class AESEncryption{
    Aes aes; 
    public AESEncryption(){
        aes = Aes.Create();
    }
    public void GetKeys(out byte[] Key, out byte[] IV){
        Key = aes.Key;
        IV = aes.IV;
    }
    public byte[] Encrypt(byte[] data, byte[] Key, byte[] IV){
        aes.Key = Key;
        aes.IV = IV;
        using (MemoryStream msEncrypt = new MemoryStream()){
            CryptoStream cryptoStream = new CryptoStream(msEncrypt,aes.CreateEncryptor(aes.Key,aes.IV),CryptoStreamMode.Write);
            cryptoStream.Write(data);
            byte[] encrypted = msEncrypt.ToArray();
            return encrypted;
        }
    }
    public byte[] Decrypt(byte[] data, byte[] Key, byte[] IV){
        aes.Key = Key;
        aes.IV = IV;
        using (MemoryStream msEncrypt = new MemoryStream()){
            CryptoStream cryptoStream = new CryptoStream(msEncrypt,aes.CreateDecryptor(aes.Key,aes.IV),CryptoStreamMode.Read);
            cryptoStream.Write(data);
            byte[] decrypted = msEncrypt.ToArray();
            return decrypted;
        }
    }



}
}
