using IndieGoat.UniversalServer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace UniversalServiceUpdater
{
    public class UniversalServiceUpdater : IServerPlugin
    {
        #region Vars

        string DynDirectory;
        List<Project> Projects = new List<Project>();

        #endregion

        string IServerPlugin.Name
        {
            get
            {
                return "Dyn";
            }
        }

        event EventHandler<SendMessageEventArgs> IServerPlugin.SendMessage
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void onLoad(string ServerDirectory)
        {
            //Set all vars
            DynDirectory = ServerDirectory + @"\Dyn";

            //Create the Dyn directory if it does not exist
            Directory.CreateDirectory(DynDirectory);

            //Load's all of the projects
            DirectoryInfo projectDirectory = new DirectoryInfo(DynDirectory);
            foreach (DirectoryInfo dirInfo in projectDirectory.GetDirectories())
            {
                Project LoadedProject = new Project(dirInfo.Name, ServerDirectory);
                Projects.Add(LoadedProject);
            }
        }

        public void Invoke(ClientSocketWorkload workload, ClientContext context, int port, List<string> Args, string ServerDirectory)
        {
            try
            {
                // AddProject[Args1] [ProjectName][Args2] [Version][Args3] [AuthPacket][Args4] [Email][Args5]
                if (Args[1].ToUpper() == "ADDPROJECT")
                {
                    try
                    {
                        //Get the version
                        string _Name = Args[2];
                        string _Version = Args[3];
                        string _Auth = Args[4];
                        string _Email = Args[5];

                        //Initialize new project
                        Project NewProject = new Project(_Name, ServerDirectory);
                        NewProject.InitializeNewProject(_Version, _Auth, _Email);

                        //Add the new project to the current list
                        Projects.Add(NewProject);

                        workload.SendMessage(context, "ADDPROJECT_TRUE");
                    }
                    catch
                    {
                        workload.SendMessage(context, "ADDPROJECT_FALSE");
                    }
                }
                // CheckProjectName[Args1] ProjectName[Args2]
                else if (Args[1].ToUpper() == "CHECKPROJECTNAME")
                {
                    string _Name = Args[2];

                    for (int i = 0; i < Projects.Count; i++)
                    {
                        if (Projects[i].ProjectName == _Name)
                        {
                            workload.SendMessage(context, "CHECKPROJECT_TRUE");
                            return;
                        }
                    }

                    workload.SendMessage(context, "CHECKPROJECT_FALSE");
                }
                // GetVersion[Args1] ProjectName[Args2]
                else if (Args[1].ToUpper() == "GETVERSION")
                {
                    try
                    {
                        //Gets the name from the args list
                        string _Name = Args[2];

                        //For each project listed
                        for (int i = 0; i < Projects.Count; i++)
                        {
                            //Gets a temp project
                            Project tmpProject = Projects[i];

                            //Check if the temp project name is equal to the requested name
                            if (tmpProject.ProjectName == _Name)
                            {
                                //Sends a message to the client of the temp project version.
                                workload.SendMessage(context, tmpProject.Version);
                                return;
                            }
                        }

                        //Sends a error message to client, project does not exist's in the project list.
                        workload.SendMessage(context, "GETVERSION_PROJECTNOTEXIST");
                    }
                    catch
                    {
                        //Sends a error message to client, unknown error.
                        workload.SendMessage(context, "GETVERSION_FALSE");
                    }
                }
                // ChangeVersion[args1] ProjectName[args2] AuthPacket[Args3] NewVersion[Args4]
                else if (Args[1].ToUpper() == "CHANGEVERSION")
                {
                    //Get all values from the args list
                    string _ProjectName = Args[2];
                    string _AuthPacket = Args[3];
                    string _NewVersion = Args[4];

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
                            if (Requestcode == "UNKNOWN") workload.SendMessage(context, "CHANGEVERSION_UNKNOWN");
                            if (Requestcode == "AUTHE") workload.SendMessage(context, "CHANGEVERSION_AUTH");
                            if (Requestcode == "TRUE") workload.SendMessage(context, "CHANGEVERSION_TRUE");
                            return;
                        }
                    }

                    //Sends a message. project does not exists
                    workload.SendMessage(context, "CHANGEVERSION_PROJECTNOTEXIST");
                }
                // ChangeAuth[args1] ProjectName[Args2] OldAuth[Args3] NewAuth[Args4]
                else if (Args[1].ToUpper() == "CHANGEAUTH")
                {
                    //Get all values from the args list
                    string _ProjectName = Args[2];
                    string _OldAuth = Args[3];
                    string _NewAuth = Args[4];

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
                            if (Requestcode == "UNKNOWN") workload.SendMessage(context, "CHANGEAUTH_UNKNOWN");
                            if (Requestcode == "AUTHE") workload.SendMessage(context, "CHANGEAUTH_AUTH");
                            if (Requestcode == "TRUE") workload.SendMessage(context, "CHANGEAUTH_TRUE");
                            return;
                        }
                    }

                    //Sends a message. project does not exist
                    workload.SendMessage(context, "CHANGEAUTH_PROJECTNOTEXIST");

                }
                else if (Args[1].ToUpper() == "FORGOTAUTH")
                {
                    //FORGOTAUTH[ARGS1] ACTIVATE[ARGS2] ProjectName[Args3]
                    if (Args[2].ToUpper() == "ACTIVATE")
                    {

                        string _ProjectName = Args[3];
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
                                workload.SendMessage(context, "FORGOTAUTH_ACTIVATE_TRUE");
                            }
                        }

                        workload.SendMessage(context, "FORGOTAUTH_ACTIVATE_FALSE");
                    }
                    //ForgotAuth[args1] Confirm[Args2] ProjectName[Args3] AuthCode[Args4]
                    if (Args[2].ToUpper() == "CONFIRM")
                    {
                        //Sets all vars
                        string _ProjectName = Args[3];
                        string AuthCode = Args[4];

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
                                    workload.SendMessage(context, "FORGOTAUTH_CONFIRM_TIME");
                                    return;
                                }

                                //Verify auth code
                                if (AuthCode == tmpProject.AuthCode)
                                {
                                    workload.SendMessage(context, "FORGOTAUTH_CONFIRM_TRUE");
                                    return;
                                }
                                else
                                {
                                    workload.SendMessage(context, "FORGOTAUTH_CONFIRM_FALSE");
                                    return;
                                }
                            }
                        }
                    }
                    //ForgotAuth[Args1] Change[Args2] ProjectName[Args3] NewAuth[Args4] AuthCode[Args5]
                    else if (Args[2].ToUpper() == "CHANGE")
                    {
                        //Sets all vars
                        string _ProjectName = Args[3];
                        string NewAuth = Args[4];
                        string AuthCode = Args[5];

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
                                    workload.SendMessage(context, "FORGOTAUTH_CONFIRM_TIME");
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
                                    workload.SendMessage(context, "FORGOTAUTH_CONFIRM_FALSE");
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

        public void Unload()
        {

        }
    }
}

