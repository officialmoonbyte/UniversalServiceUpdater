using Moonbyte.UniversalServer.PluginFramework;
using System;
using System.Collections.Generic;
using System.IO;

namespace UniversalServiceUpdater
{
    public class UniversalServiceUpdater : UniversalPluginFramework
    {
        #region Vars

        string DynDirectory;
        List<Project> Projects = new List<Project>();

        #endregion

        public string Name
        {
            get { return "dyn"; }
        }

        string _PluginSettingsDirectory;

        public void OnLoad(string PluginSettingsDirectory)
        {
            //Set all vars
            DynDirectory = PluginSettingsDirectory + @"\Dyn";
            _PluginSettingsDirectory = PluginSettingsDirectory;

            //Create the Dyn directory if it does not exist
            Directory.CreateDirectory(DynDirectory);

            //Load's all of the projects
            DirectoryInfo projectDirectory = new DirectoryInfo(DynDirectory);
            foreach (DirectoryInfo dirInfo in projectDirectory.GetDirectories())
            {
                Project LoadedProject = new Project(dirInfo.Name, PluginSettingsDirectory);
                Projects.Add(LoadedProject);
            }
        }

        public void Invoke(ClientContext Client, string[] RawCommand)
        {
            try
            {
                // AddProject[Args1] [ProjectName][Args2] [Version][Args3] [AuthPacket][Args4] [Email][Args5]
                if (RawCommand[1].ToUpper() == "ADDPROJECT")
                {
                    try
                    {
                        //Get the version
                        string _Name = RawCommand[2];
                        string _Version = RawCommand[3];
                        string _Auth = RawCommand[4];
                        string _Email = RawCommand[5];

                        //Initialize new project
                        Project NewProject = new Project(_Name, _PluginSettingsDirectory);
                        NewProject.InitializeNewProject(_Version, _Auth, _Email);

                        //Add the new project to the current list
                        Projects.Add(NewProject);

                        Client.SendMessage("ADDPROJECT_TRUE");
                    }
                    catch
                    {
                        Client.SendMessage("ADDPROJECT_FALSE");
                    }
                }
                // CheckProjectName[Args1] ProjectName[Args2]
                else if (RawCommand[1].ToUpper() == "CHECKPROJECTNAME")
                {
                    string _Name = RawCommand[2];

                    for (int i = 0; i < Projects.Count; i++)
                    {
                        if (Projects[i].ProjectName == _Name)
                        {
                            Client.SendMessage("CHECKPROJECT_TRUE");
                            return;
                        }
                    }

                    Client.SendMessage("CHECKPROJECT_FALSE");
                }
                // GetVersion[Args1] ProjectName[Args2]
                else if (RawCommand[1].ToUpper() == "GETVERSION")
                {
                    try
                    {
                        //Gets the name from the args list
                        string _Name = RawCommand[2];

                        //For each project listed
                        for (int i = 0; i < Projects.Count; i++)
                        {
                            //Gets a temp project
                            Project tmpProject = Projects[i];

                            //Check if the temp project name is equal to the requested name
                            if (tmpProject.ProjectName == _Name)
                            {
                                //Sends a message to the client of the temp project version.
                                Client.SendMessage(tmpProject.Version);
                                return;
                            }
                        }

                        //Sends a error message to client, project does not exist's in the project list.
                        Client.SendMessage("GETVERSION_PROJECTNOTEXIST");
                    }
                    catch
                    {
                        //Sends a error message to client, unknown error.
                        Client.SendMessage("GETVERSION_FALSE");
                    }
                }
                // ChangeVersion[args1] ProjectName[args2] AuthPacket[Args3] NewVersion[Args4]
                else if (RawCommand[1].ToUpper() == "CHANGEVERSION")
                {
                    //Get all values from the args list
                    string _ProjectName = RawCommand[2];
                    string _AuthPacket = RawCommand[3];
                    string _NewVersion = RawCommand[4];

                    for (int i = 0; i < Projects.Count; i++)
                    {
                        //Sets a temp project
                        Project TmpProject = Projects[i];

                        //Check if the temp project name is the requested name
                        if (TmpProject.ProjectName == _ProjectName)
                        {
                            //Get a request code to change the version
                            string Requestcode = TmpProject.ChangeVersion(_AuthPacket, _NewVersion);

                            //Sends error message and / or sucessful message
                            if (Requestcode == "UNKNOWN") Client.SendMessage("CHANGEVERSION_UNKNOWN");
                            if (Requestcode == "AUTHE") Client.SendMessage("CHANGEVERSION_AUTH");
                            if (Requestcode == "TRUE") Client.SendMessage("CHANGEVERSION_TRUE");
                            return;
                        }
                    }

                    //Sends a message. project does not exists
                    Client.SendMessage("CHANGEVERSION_PROJECTNOTEXIST");
                }
                // ChangeAuth[args1] ProjectName[Args2] OldAuth[Args3] NewAuth[Args4]
                else if (RawCommand[1].ToUpper() == "CHANGEAUTH")
                {
                    //Get all values from the args list
                    string _ProjectName = RawCommand[2];
                    string _OldAuth = RawCommand[3];
                    string _NewAuth = RawCommand[4];

                    //Tries to find the project
                    for (int i = 0; i < Projects.Count; i++)
                    {
                        //Set the temp project
                        Project tmpProject = Projects[i];

                        //Check if tmpproject name is equal to the requested project
                        if (tmpProject.ProjectName == _ProjectName)
                        {
                            //Get a request code to change the auth
                            string Requestcode = tmpProject.ChangeAuth(_OldAuth, _NewAuth);

                            //Sends error message and / or sucessful message
                            if (Requestcode == "UNKNOWN") Client.SendMessage("CHANGEAUTH_UNKNOWN");
                            if (Requestcode == "AUTHE") Client.SendMessage("CHANGEAUTH_AUTH");
                            if (Requestcode == "TRUE") Client.SendMessage("CHANGEAUTH_TRUE");
                            return;
                        }
                    }

                    //Sends a message. project does not exist
                    Client.SendMessage("CHANGEAUTH_PROJECTNOTEXIST");

                }
                else if (RawCommand[1].ToUpper() == "FORGOTAUTH")
                {
                    //FORGOTAUTH[ARGS1] ACTIVATE[ARGS2] ProjectName[Args3]
                    if (RawCommand[2].ToUpper() == "ACTIVATE")
                    {

                        string _ProjectName = RawCommand[3];
                        //Tries to find the project
                        for (int i = 0; i < Projects.Count; i++)
                        {
                            Project tmpProject = Projects[i];

                            //Check if the tmp project name is equal to requested project
                            if (tmpProject.ProjectName == _ProjectName)
                            {
                                //Generates the auth code
                                int AuthCode = tmpProject.GenerateAuthCode();

                                //Sends email
                                Console.WriteLine(tmpProject.Email);
                                Client.SendMessage("FORGOTAUTH_ACTIVATE_TRUE");
                            }
                        }

                        Client.SendMessage("FORGOTAUTH_ACTIVATE_FALSE");
                    }
                    //ForgotAuth[args1] Confirm[Args2] ProjectName[Args3] AuthCode[Args4]
                    if (RawCommand[2].ToUpper() == "CONFIRM")
                    {
                        //Sets all vars
                        string _ProjectName = RawCommand[3];
                        string AuthCode = RawCommand[4];

                        //Tries to find the project
                        for (int i = 0; i < Projects.Count; i++)
                        {
                            Project tmpProject = Projects[i];

                            //Check if the tmp project name is equal to the requested project
                            if (tmpProject.ProjectName == _ProjectName)
                            {
                                string ProjectAuthCode = tmpProject.AuthCode;

                                if (ProjectAuthCode == "0")
                                {
                                    Client.SendMessage("FORGOTAUTH_CONFIRM_TIME");
                                    return;
                                }

                                //Verify auth code
                                if (AuthCode == tmpProject.AuthCode)
                                {
                                    Client.SendMessage("FORGOTAUTH_CONFIRM_TRUE");
                                    return;
                                }
                                else
                                {
                                    Client.SendMessage("FORGOTAUTH_CONFIRM_FALSE");
                                    return;
                                }
                            }
                        }
                    }
                    //ForgotAuth[Args1] Change[Args2] ProjectName[Args3] NewAuth[Args4] AuthCode[Args5]
                    else if (RawCommand[2].ToUpper() == "CHANGE")
                    {
                        //Sets all vars
                        string _ProjectName = RawCommand[3];
                        string NewAuth = RawCommand[4];
                        string AuthCode = RawCommand[5];

                        //Tries to find the project
                        for (int i = 0; i < Projects.Count; i++)
                        {
                            Project tmpProject = Projects[i];

                            //Check if the tmp project name is equal to the requested project
                            if (tmpProject.ProjectName == _ProjectName)
                            {
                                string ProjectAuthCode = tmpProject.AuthCode;

                                if (ProjectAuthCode == "0")
                                {
                                    Client.SendMessage("FORGOTAUTH_CONFIRM_TIME");
                                    return;
                                }

                                //Verify auth code
                                if (AuthCode == tmpProject.AuthCode)
                                {
                                    tmpProject.Auth = NewAuth;
                                    return;
                                }
                                else
                                {
                                    Client.SendMessage("FORGOTAUTH_CONFIRM_FALSE");
                                    return;
                                }
                            }
                        }
                    }
                }

            }
            catch
            {

            }
        }
    }
}

