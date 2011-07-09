using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using System.Timers;
using Terraria_Server.Plugin;
using Terraria_Server;
using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Misc;

namespace Passport
{
    public class PassportPlugin : Plugin
    {
        public static string PLUGIN_FOLDER = Statics.PluginPath + Path.DirectorySeparatorChar + "Passport";
        public Properties properties;
        public bool isEnabled = false;
        private PassportManager passportManager;
        private Timer passportManagerSaveTimer;
        private int saveTimeMillis = 300000; // 5 mins

        public override void Load()
        {
            Name = "Passport";
            Description = "Server-side single user accounts";
            Author = "Envoy"; 
            Version = "1.1.24";
            TDSMBuild = 24;
         
            Log("Version " + base.Version + " Loading...");
        
            // setup properties
            setupProperties();
         
            // get the singleton PassportManager
            passportManager = PassportManagerFactory.getPassportManager();

            // start the save timer
            Log("Starting save timer with interval [" + saveTimeMillis + "] millis");
            passportManagerSaveTimer = new PassportManagerSaveTimer(passportManager, saveTimeMillis);
            passportManagerSaveTimer.Start();
         
            isEnabled = true;
        }

        // 
        // Plugin
        //
     
        public override void Enable()
        {
            Log("Enabled");
            this.registerHook(Hooks.PLAYER_COMMAND);            
            this.registerHook(Hooks.PLAYER_LOGOUT);            
        }

        public override void Disable()
        {
            Log("Disabled");
            passportManagerSaveTimer.Stop();
            passportManager.save();
            isEnabled = false;
        }
     
        public override void onPlayerLogout(PlayerLogoutEvent Event)
        { 
            // not sure what is going on, but must do this hack
            Event.Player.Name = Event.Socket.oldName;
            passportManager.logout(Event.Player);
            Event.Player.Name = null;
            Event.Cancelled = true;
        }
     
        public override void onPlayerCommand(PlayerCommandEvent Event)
        {
            string[] commands = Event.Message.ToLower().Split(' '); //Split into sections (to lower case to work with it better)

            if (commands.Length > 0) {               
                if (commands[0] != null && commands[0].Trim().Length > 0) { //If it is not nothing, and the string is actually something

                    //
                    // CREATEUSER
                    //
                    if (commands[0].Equals("/createuser")) {
                        // proper usage
                        if (commands.Length != 3) {
                            Event.Player.sendMessage("Error: /createuser <username> <password>.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        string username = commands[1];
                        string password = commands[2];

                        Passport passport = passportManager.getPassport(Event.Player);
                        if (passport != null) {
                            Event.Player.sendMessage("Error: Already logged in.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        try {
                            passport = passportManager.createUser(Event.Player, username, password);
                            Event.Player.sendMessage("Successfully created account.", 255, 0f, 255f, 255f);
                        } catch (UserExistsException e) {
                            Log("User already exists");
                            Event.Player.sendMessage("Error: User already exists.", 255, 255f, 0f, 0f);
                            return;
                        }

                        Event.Cancelled = true;
                    }

                    //
                    // LOGIN
                    //
                    if (commands[0].Equals("/login")) {
                        // proper usage
                        if (commands.Length != 3) {
                            Event.Player.sendMessage("Error: /login <username> <password>.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        string username = commands[1];
                        string password = commands[2];

                        Passport passport = null;

                        passport = passportManager.getPassport(Event.Player);
                        if (passport != null) {
                            Event.Player.sendMessage("Error: Already logged in.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        // purposefully be vague with error messages to deter information mining
                        try {
                            passport = passportManager.loginUser(Event.Player, username, password);
                            Event.Player.sendMessage("Successfully logged in.", 255, 0f, 255f, 255f);
                        } catch (UserNotFoundException e1) {
                            Event.Player.sendMessage("Error: Authentication.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        } catch (AuthenticationException e2) {
                            Event.Player.sendMessage("Error: Authentication.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        } catch (UserAlreadyLoggedInException e3) {
                            Event.Player.sendMessage("Error: Authentication.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        Event.Cancelled = true;
                    }

                    //
                    // LOGOUT
                    //
                    if (commands[0].Equals("/logout")) {        
                        Log("logout");

                        try {
                            passportManager.logout(Event.Player);
                            Event.Player.sendMessage("Successfully logged out.", 255, 0f, 255f, 255f);
                            Event.Cancelled = true;
                            return;
                        } catch (UserNotLoggedInException e) {
                            Event.Player.sendMessage("Error: Not logged in.", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;

                        }

                    }

                }
            }

            Event.Cancelled = true;
        }

        //
        // PRIVATE
        //

        private void setupProperties()
        {
            string pluginFolder = PLUGIN_FOLDER;
            createDirectory(pluginFolder);

            properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "passport.properties");
            properties.Load();
            properties.pushData(); //Creates default values if needed.
            properties.Save();

            //read properties data
            saveTimeMillis = properties.saveTimeMillis();
        }

        private static void createDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

        private void Log(string message)
        {
            Program.tConsole.WriteLine("[" + base.Name + "] " + message);
        }

        /**
         * Timer to automatically save user data.
         */
        private class PassportManagerSaveTimer : Timer
        {
            private PassportManager passportManager;

            public PassportManagerSaveTimer(PassportManager passportManager, int saveTimeMillis)
            {
                this.passportManager = passportManager;
                this.Interval = saveTimeMillis;
                this.Elapsed += new ElapsedEventHandler(this.elapsed);
                this.Enabled = true;
            }

            public void elapsed(object sender, ElapsedEventArgs e)
            {
                passportManager.save();
            }
        }

    }

}