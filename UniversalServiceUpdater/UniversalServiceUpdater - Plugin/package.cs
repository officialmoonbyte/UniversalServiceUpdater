using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniversalServiceUpdater
{
    public class Project
    {
        #region Vars

        string ServerDirectory;
        public string ProjectName;
        string ProjectDirectory;

        //File Directories
        string EmailFile;
        string ProjectVersionFile;
        string AuthFile;
        string ConfirmAuthFile;
        string SaltFile;

        #endregion

        #region Main

        /// <summary>
        /// Initialize the project
        /// </summary>
        /// <param name="_ProjectName">The name of the project</param>
        /// <param name="_ServerDirectory">Server directory</param>
        public Project(string _ProjectName, string _ServerDirectory)
        {
            //Set all local vars
            ServerDirectory = _ServerDirectory;
            ProjectName = _ProjectName;
            ProjectDirectory = ServerDirectory + @"\Dyn\Projects\" + _ProjectName + @"\";

            //Create the project directory if it does not exist
            if (!Directory.Exists(ProjectDirectory)) Directory.CreateDirectory(ProjectDirectory);

            //Set all of the file directories for the project
            EmailFile = ProjectDirectory + "email.eml";
            ProjectVersionFile = ProjectDirectory + "version.ver";
            AuthFile = ProjectDirectory + "Auth.psd";
            ConfirmAuthFile = ProjectDirectory + "renewAuth.ren";
            SaltFile = ProjectDirectory + "Salt.sat";

            //Generates and writes information in all of the files, if needed.
            if (!File.Exists(EmailFile)) File.Create(EmailFile).Close();
            if (!File.Exists(ProjectVersionFile)) File.Create(ProjectVersionFile).Close();
            if (!File.Exists(AuthFile)) File.Create(AuthFile).Close();
            if (!File.Exists(ConfirmAuthFile)) File.Create(ConfirmAuthFile).Close();
            if (!File.Exists(SaltFile)) File.Create(SaltFile).Close();

            //Edit the confirm auth file 
            string fileContent = DateTime.Now.ToString();
            File.WriteAllText(ConfirmAuthFile, fileContent);
        }

        #endregion

        #region Create new project

        /// <summary>
        /// Set all vars of a project
        /// </summary>
        public void InitializeNewProject(string _ProjectVersion, string _AuthPacket, string _Email)
        {
            //Generate salt and write salt
            byte[] bytes = new byte[128];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);

            Salt = Convert.ToBase64String(bytes);

            //Set all vars
            Version = _ProjectVersion;
            Email = _Email;
            Auth = sha512Encryption.Encrypt(Salt + _AuthPacket);
        }

        #endregion

        #region Change version

        /// <summary>
        /// Check the auth and then returns a string
        /// </summary>
        public string ChangeVersion(string _AuthPacket, string _NewVersion)
        {
            try
            {
                if (sha512Encryption.Encrypt(Salt + _AuthPacket) != Auth) return "AUTHE";

                Version = _NewVersion;
                return "TRUE";
            }
            catch
            { return "UNKNOWN"; }
        }

        #endregion

        #region Change Auth

        public string ChangeAuth(string _OldAuth, string _NewAuth)
        {
            try
            {
                if (sha512Encryption.Encrypt(Salt + _OldAuth) != Auth) return "AUTHE";

                Auth = sha512Encryption.Encrypt(Salt + _NewAuth);
                return "TRUE";
            }
            catch
            { return "UNKNOWN"; }
        }

        #endregion

        #region Create auth code

        public int GenerateAuthCode()
        {
            this.AuthCode = null;
            return int.Parse(this.AuthCode);
        }

        #endregion

        #region Values

        public string Email
        {
            get { return File.ReadAllText(EmailFile); }
            set
            { File.WriteAllText(EmailFile, value); }
        }
        public string Version
        {
            get { return File.ReadAllText(ProjectVersionFile); }
            set { File.WriteAllText(ProjectVersionFile, value); }
        }
        public string Auth
        {
            get { return File.ReadAllText(AuthFile); }
            set { File.WriteAllText(AuthFile, value); }
        }
        public string Salt
        {
            get { return File.ReadAllText(SaltFile); }
            set { File.WriteAllText(SaltFile, value); }
        }
        public string AuthCode
        {
            get
            {
                string[] AuthContent = File.ReadAllText(ConfirmAuthFile).Split(new string[] { "%20%" }, StringSplitOptions.None);
                DateTime nowDate = DateTime.Now;
                DateTime AuthDate = DateTime.Parse(AuthContent[1]);

                if (nowDate.AddMinutes(30) > AuthDate) { return 0.ToString(); }
                else { return AuthContent[0]; }
            }
            set
            {
                //Generates a random number
                Random rnd = new Random();
                int authCode = rnd.Next(100000, 999999);

                //Checks if the string is null
                if (value == null)
                {
                    //Writes to the auth file
                    string AuthPacket = authCode + "%20%" + DateTime.Now.ToString();
                    File.WriteAllText(ConfirmAuthFile, AuthPacket);
                }
            }
        }

        #endregion

        #region Encrypting

        class sha512Encryption
        {

            public static string Encrypt(string value)
            {
                try
                {
                    SHA512 sha512 = SHA512Managed.Create();
                    byte[] bytes = Encoding.UTF8.GetBytes(value);
                    byte[] hash = sha512.ComputeHash(bytes);
                    return GetStringFromHash(hash);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error] UserDatabase - Failed to encrypt a value : " + e.Message);
                    return null;
                }
            }

            private static string GetStringFromHash(byte[] hash)
            {
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }
                return result.ToString();
            }

        }

        #endregion
    }
}
