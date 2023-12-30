using System;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Node.Certificate;
using Node.Certificate.Models;
using Node.Certificate.Template;
using static System.Environment;

namespace TestNode
{

    [TestClass]
    public class FullIdentCATest
	{

        [TestMethod]
        public async Task FullCACreationTest()
        {
            CertificateSettings certificateSettings = new CertificateSettings(CertificateType.CA);
            CertificateAuthorityConfig certificateAuthorityConfig = new CertificateAuthorityConfig();
            certificateAuthorityConfig.Difficulty = 14;
            ConcreteSystemSecurityBuilder builder = new ConcreteSystemSecurityBuilder(certificateAuthorityConfig, certificateSettings);

            var template = builder.CreateCertificateTemplate();
            await builder.GenerateKeys(generateTypes: GenerateTypes.Threads);
            var cert = builder.CreateSelfSignedCertificate(builder.GetPrivate(), builder.GetPublicKey(), template);

            //TODO: Why we get different leght ?
            Regex regex = new(@$"^.?.?{BitConverter.ToString(template.GetSerialNumber()).Replace("-", "")}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Assert.IsTrue(regex.IsMatch(cert.SerialNumber));
            Assert.AreEqual(cert.SubjectName.Name, "O=LittleMozzarella, CN=localhost");
            Assert.AreEqual(cert.IssuerName.Name, "O=LittleMozzarella, CN=localhost");
            Assert.IsTrue(cert.NotAfter == new DateTime(0001, 01, 01, 12, 50, 00));
            Assert.IsTrue(cert.NotBefore == new DateTime(0001, 01, 01, 12, 50, 00));
            Assert.IsTrue(cert.SignatureAlgorithm.FriendlyName == "sha256ECDSA");
            Assert.IsTrue(cert.Extensions.Count == 5);
            Assert.IsTrue(cert.Extensions.FirstOrDefault(m => m.Oid.Value== "2.5.29.15").Critical == true);
            Assert.IsTrue(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.19").Critical == true);
            Assert.IsTrue(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.14").Critical == false);
            Assert.IsTrue(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.999.2.1").Critical == false);
            Assert.IsTrue(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.17").Critical == false);
            Assert.AreEqual(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.999.2.1").RawData.Length, 1);
            Assert.AreEqual(cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.999.2.1").RawData.FirstOrDefault(), 0);
            string ExportedPublicKey = Convert.ToBase64String(cert.PublicKey.GetECDsaPublicKey().ExportSubjectPublicKeyInfo());
            string PublicKey = Convert.ToBase64String(builder.GetPublicKey());
            Assert.AreEqual(ExportedPublicKey, PublicKey);
        }

        [TestMethod]
        public async Task FullIdentityCreationTest()
        {
            
            CertificateSettings certificateSettings = new CertificateSettings(CertificateType.CA);
            CertificateAuthorityConfig certificateAuthorityConfig = new CertificateAuthorityConfig();
            certificateAuthorityConfig.Difficulty = 14;
            ConcreteSystemSecurityBuilder builder = new ConcreteSystemSecurityBuilder(certificateAuthorityConfig, certificateSettings);
            CertificateDirector director = new OSXCertificateDirector();
            var certificate = await director.GenerateFullCertificateAuthority(builder);

            certificateSettings = new CertificateSettings(CertificateType.Identity);
            builder = new ConcreteSystemSecurityBuilder(certificateSettings);
            var template = builder.CreateLeafTemplate();
            byte[]? identityKey = null;
            var identity = builder.NewIdentity(certificate.Cert, certificate.PrivateKey, template, out identityKey);
            builder.SaveIdentity(identity);
            if (identityKey != null)
            {
                builder.SaveKey(identityKey);
            }
            CertificateAuthorizer authorizer = new CertificateAuthorizer(SpecialFolder.UserProfile);
            var ca = authorizer.LoadIdentConfig();

            Regex regex = new(@$"^.?.?{BitConverter.ToString(template.GetSerialNumber()).Replace("-", "")}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Assert.IsTrue(regex.IsMatch(ca.Cert.SerialNumber));
            Assert.AreEqual(ca.Cert.SubjectName.Name, "O=LittleMozzarella, CN=localhost");
            Assert.AreEqual(ca.Cert.IssuerName.Name, "O=LittleMozzarella, CN=localhost");
            Assert.IsTrue(ca.Cert.NotAfter == new DateTime(0001, 01, 01, 12, 50, 00));
            Assert.IsTrue(ca.Cert.NotBefore == new DateTime(0001, 01, 01, 12, 50, 00));
            Assert.IsTrue(ca.Cert.SignatureAlgorithm.FriendlyName == "sha256ECDSA");
            Assert.IsTrue(ca.Cert.Extensions.Count == 4);
            Assert.IsTrue(ca.Cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.15").Critical == true);
            Assert.IsTrue(ca.Cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.19").Critical == true);
            Assert.IsTrue(ca.Cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.17").Critical == false);
            Assert.IsTrue(ca.Cert.Extensions.FirstOrDefault(m => m.Oid.Value == "2.5.29.37").Critical == false);
            string ExportedPublicKey = Convert.ToBase64String(ca.Leaf.PublicKey.ExportSubjectPublicKeyInfo());
            string PublicKey = Convert.ToBase64String(certificate.Cert.PublicKey.ExportSubjectPublicKeyInfo());
            Assert.AreEqual(ExportedPublicKey, PublicKey);
        }

        [TestMethod]
        public async Task IdentityLoad()
        {
            CertificateSettings certificateSettings = new CertificateSettings(CertificateType.CA);
            CertificateAuthorityConfig certificateAuthorityConfig = new CertificateAuthorityConfig();
            certificateAuthorityConfig.Difficulty = 14;
            ConcreteSystemSecurityBuilder builder = new ConcreteSystemSecurityBuilder(certificateAuthorityConfig, certificateSettings);
            CertificateDirector director = new OSXCertificateDirector();
            var certificate = await director.GenerateFullCertificateAuthority(builder);

            certificateSettings = new CertificateSettings(CertificateType.Identity);
            builder = new ConcreteSystemSecurityBuilder(certificateSettings);
            director.CreateIdentity(builder, certificate);
            CertificateAuthorizer authorizer = new CertificateAuthorizer(SpecialFolder.UserProfile);
            var identity = authorizer.LoadIdentConfig();
            var ca = authorizer.LoadFullCAConfig();
            Assert.IsTrue(identity.Cert != null);
            Assert.IsTrue(identity.Leaf != null);
            Assert.IsTrue(identity.NodeID.Length == 32);
            Assert.IsTrue(identity.RestChain.Length >= 0);
            Assert.IsTrue(identity.NodeID[identity.NodeID.Length - 1] >= 0);
            
            Assert.IsTrue(ca.Cert != null);
            Assert.IsTrue(ca.algorithm.SignatureAlgorithm == "ECDsa");
            Assert.IsTrue(ca.NodeID.Length == 32);
            Assert.IsTrue(ca.RestChain.Length >= 1);
            Assert.IsTrue(ca.NodeID[ca.NodeID.Length-1] >= 0);
            Assert.IsTrue(ca.PrivateKey != null);

        }



    }
}

