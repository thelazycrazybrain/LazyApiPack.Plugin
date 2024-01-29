﻿using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

using (RSA parent = RSA.Create(4096))
using (RSA rsa = RSA.Create(2048))
{
    CertificateRequest parentReq = new CertificateRequest(
        "CN=Experimental Issuing Authority",
        parent,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);

    parentReq.CertificateExtensions.Add(
        new X509BasicConstraintsExtension(true, false, 0, true));

    parentReq.CertificateExtensions.Add(
        new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

    using (X509Certificate2 parentCert = parentReq.CreateSelfSigned(
        DateTimeOffset.UtcNow.AddDays(-45),
        DateTimeOffset.UtcNow.AddYears(365)))
    {
        CertificateRequest req = new CertificateRequest(
            "CN=Valid-Looking Timestamp Authority",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.CrlSign ,
                false));

        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                    new Oid("1.3.6.1.5.5.7.3.8")
                },
                true));

        req.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        using (X509Certificate2 cert = req.Create(
            parentCert,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(9999),
            new byte[] { 1, 2, 3, 4 }))
        {
            //File.WriteAllBytes("C:\\Temp\\MyCert.pfx", cert.Export(X509ContentType.Pfx, "12345"));


            // Do something with these certs, like export them to PFX,
            // or add them to an X509Store, or whatever.
        }

        File.WriteAllBytes("C:\\Temp\\FullWithPrivate.pfx", parentCert.Export(X509ContentType.Pfx, "12345"));

    }
    // signtool sign /f MyCert.pfx /p 12345 /fd SHA256 LazyApiPack.Plugin.Demo.Plugin1.dll"
    // signtool sign /f MyCert.pfx /p 12345 /fd SHA256 LazyApiPack.Plugin.Demo.Plugin2.dll"
}