﻿using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace MusicBeePlugin
{
	public partial class Plugin
    {
        private MusicBeeApiInterface mbApiInterface;
        private PluginInfo about = new PluginInfo();
		private float completedPercentage = 0;
		private System.Timers.Timer timer;

        public PluginInfo Initialise(IntPtr apiInterfacePtr)
        {
            mbApiInterface = new MusicBeeApiInterface();
            mbApiInterface.Initialise(apiInterfacePtr);
            about.PluginInfoVersion = PluginInfoVersion;
            about.Name = "Taskbar Tidbits";
            about.Description = "Display music progress in taskbar";
            about.Author = "AvikB";
            about.TargetApplication = "";   // current only applies to artwork, lyrics or instant messenger name that appears in the provider drop down selector or target Instant Messenger
            about.Type = PluginType.General;
            about.VersionMajor = 1;  // your plugin version
            about.VersionMinor = 0;
            about.Revision = 0;
            about.MinInterfaceVersion = MinInterfaceVersion;
            about.MinApiRevision = MinApiRevision;
            about.ReceiveNotifications = (ReceiveNotificationFlags.PlayerEvents | ReceiveNotificationFlags.TagEvents);
			about.ConfigurationPanelHeight = 0;   // height in pixels that musicbee should reserve in a panel for config settings. When set, a handle to an empty panel will be passed to the Configure function
			initTimer();
			return about;
        }

        public bool Configure(IntPtr panelHandle)
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
            // panelHandle will only be set if you set about.ConfigurationPanelHeight to a non-zero value
            // keep in mind the panel width is scaled according to the font the user has selected
            // if about.ConfigurationPanelHeight is set to 0, you can display your own popup window
            if (panelHandle != IntPtr.Zero)
            {
                Panel configPanel = (Panel)Panel.FromHandle(panelHandle);
                Label prompt = new Label();
                prompt.AutoSize = true;
                prompt.Location = new Point(0, 0);
                prompt.Text = "prompt:";
                TextBox textBox = new TextBox();
                textBox.Bounds = new Rectangle(60, 0, 100, textBox.Height);
                configPanel.Controls.AddRange(new Control[] { prompt, textBox });
            }
            return false;
        }
       
        // called by MusicBee when the user clicks Apply or Save in the MusicBee Preferences screen.
        // its up to you to figure out whether anything has changed and needs updating
        public void SaveSettings()
        {
            // save any persistent settings in a sub-folder of this path
            string dataPath = mbApiInterface.Setting_GetPersistentStoragePath();
        }

        // MusicBee is closing the plugin (plugin is being disabled by user or MusicBee is shutting down)
        public void Close(PluginCloseReason reason)
        {
        }

        // uninstall this plugin - clean up any persisted files
        public void Uninstall()
        {
        }

        // receive event notifications from MusicBee
        // you need to set about.ReceiveNotificationFlags = PlayerEvents to receive all notifications, and not just the startup event
        public void ReceiveNotification(string sourceFileUrl, NotificationType type)
        {
            // perform some action depending on the notification type
            switch (type)
            {
                case NotificationType.PluginStartup:
					OnMusicBeeStarted();
					// perform startup initialisation
					switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
							break;
                        case PlayState.Paused:
                            break;
                    }
                    break;
                case NotificationType.TrackChanged:
                    string artist = mbApiInterface.NowPlaying_GetFileTag(MetaDataType.Artist);

                    break;
                case NotificationType.PlayStateChanged:
                    switch (mbApiInterface.Player_GetPlayState())
                    {
                        case PlayState.Playing:
                            TaskbarProgress.SetState(mbApiInterface.MB_GetWindowHandle(), TaskbarProgress.TaskbarStates.Normal);
                            break;
                        case PlayState.Paused:
                            TaskbarProgress.SetState(mbApiInterface.MB_GetWindowHandle(), TaskbarProgress.TaskbarStates.Paused);
                            break;
                        default:
                            TaskbarProgress.SetState(mbApiInterface.MB_GetWindowHandle(), TaskbarProgress.TaskbarStates.NoProgress);
                            break;
                    }
                    break;

            }
        }

		private void initTimer()
		{
            timer = new System.Timers.Timer();
			timer.Interval = 100;
			timer.Elapsed += new ElapsedEventHandler(onTime);
			timer.Enabled = true;
		}

		private void onTime(object sender, ElapsedEventArgs e)
		{
			updateTaskbarProgress();
		}

		private void OnMusicBeeStarted()
		{
			updateTaskbarProgress();
		}

		private void updateTaskbarProgress()
		{
			float currentPos = mbApiInterface.Player_GetPosition();
			float totalTime = mbApiInterface.NowPlaying_GetDuration();
			completedPercentage = currentPos / totalTime * 100;


			TaskbarProgress.SetValue(mbApiInterface.MB_GetWindowHandle(), completedPercentage, 100);
		}

		// return an array of lyric or artwork provider names this plugin supports
		// the providers will be iterated through one by one and passed to the RetrieveLyrics/ RetrieveArtwork function in order set by the user in the MusicBee Tags(2) preferences screen until a match is found
		public string[] GetProviders()
        {
            return null;
        }

        // return lyrics for the requested artist/title from the requested provider
        // only required if PluginType = LyricsRetrieval
        // return null if no lyrics are found
        public string RetrieveLyrics(string sourceFileUrl, string artist, string trackTitle, string album, bool synchronisedPreferred, string provider)
        {
            return null;
        }

        // return Base64 string representation of the artwork binary data from the requested provider
        // only required if PluginType = ArtworkRetrieval
        // return null if no artwork is found
        public string RetrieveArtwork(string sourceFileUrl, string albumArtist, string album, string provider)
        {
            //Return Convert.ToBase64String(artworkBinaryData)
            return null;
        }

	}
}