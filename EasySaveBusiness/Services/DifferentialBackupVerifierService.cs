using System.IO;
using System.Security.Cryptography;
using EasySaveBusiness.Models;
namespace EasySaveBusiness.Services
{
    public class DifferentialBackupVerifierService
    {
        public bool VerifyDifferentialBackupAndShaDifference(BackupConfig config,string file1, string file2)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                byte[] hash1 = hashAlgorithm.ComputeHash(File.ReadAllBytes(file1));
                byte[] hash2 = hashAlgorithm.ComputeHash(File.ReadAllBytes(file2));
                return config.Type == BackupType.Full || !File.Exists(destinationFile) || StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
            }
        }
    }
}
