using System;
using System.Security.Cryptography;
using System.Text;
using ClubGamerZone.TowerDefense.Application.Authentication;
using UnityEngine;

namespace ClubGamerZone.TowerDefense.Infrastructure.Firebase
{
    public sealed class ProtectedAuthenticationSessionStore : IAuthenticationSessionStore
    {
        private const string RefreshTokenKey = "citadel_fall.auth.refresh_token.v1";
        private const string KeySalt = "CitadelFall.PersistentAuth.v1";

        public string LoadRefreshToken()
        {
            var protectedValue = PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
            if (string.IsNullOrWhiteSpace(protectedValue))
            {
                return string.Empty;
            }

            try
            {
                var payload = Convert.FromBase64String(protectedValue);
                if (payload.Length <= 16)
                {
                    return string.Empty;
                }

                var iv = new byte[16];
                var encrypted = new byte[payload.Length - iv.Length];
                Buffer.BlockCopy(payload, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(payload, iv.Length, encrypted, 0, encrypted.Length);
                using (var aes = CreateAes(iv))
                using (var decryptor = aes.CreateDecryptor())
                {
                    var plain = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    return Encoding.UTF8.GetString(plain);
                }
            }
            catch (Exception)
            {
                Clear();
                return string.Empty;
            }
        }

        public void SaveRefreshToken(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Clear();
                return;
            }

            using (var aes = CreateAes(null))
            using (var encryptor = aes.CreateEncryptor())
            {
                var plain = Encoding.UTF8.GetBytes(refreshToken);
                var encrypted = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                var payload = new byte[aes.IV.Length + encrypted.Length];
                Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
                Buffer.BlockCopy(encrypted, 0, payload, aes.IV.Length, encrypted.Length);
                PlayerPrefs.SetString(RefreshTokenKey, Convert.ToBase64String(payload));
                PlayerPrefs.Save();
            }
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(RefreshTokenKey);
            PlayerPrefs.Save();
        }

        private static Aes CreateAes(byte[] iv)
        {
            var aes = Aes.Create();
            aes.Key = BuildDeviceKey();
            if (iv != null)
            {
                aes.IV = iv;
            }

            return aes;
        }

        private static byte[] BuildDeviceKey()
        {
            var identity = $"{UnityEngine.Application.identifier}|{SystemInfo.deviceUniqueIdentifier}|{KeySalt}";
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(identity));
            }
        }
    }
}
