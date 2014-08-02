using System;
using Android.App;
using Android.Os;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using System.Threading;
using Android.Content;
using Java.Net;
using Java.Io;
using Android.View;
using Java.Util;
using Org.Apache.Http.Conn.Util;
using Android.Media;

namespace AiSatanDevice
{
	class AiPlayer : MainActivity
	{
		public string MusicFolder { get; set; }

		public MediaPlayer mediaPlayer;
		public AudioManager audioManager;

		public AiPlayer()
		{
			MusicFolder = Android.Os.Environment.DIRECTORY_MUSIC + "/audio";
			audioManager = (AudioManager) GetSystemService(AUDIO_SERVICE);
		}

		public void Start()
		{
			var files = new File(MusicFolder);
			mediaPlayer = new MediaPlayer();
			foreach (var file in files.List())
			{
				mediaPlayer.SetDataSource(file);
				mediaPlayer.SetAudioStreamType(AudioManager.STREAM_MUSIC);
				mediaPlayer.Prepare();
				mediaPlayer.Start();
				var changer = new OnChangeListener();
				mediaPlayer.SetOnCompletionListener(changer);
				break;
			}
		}

		class OnChangeListener : MediaPlayer.IOnCompletionListener
		{
			public string MusicFolder { get; set; }

			public void OnCompletion(MediaPlayer mp)
			{
				mp.SetDataSource(MusicFolder);
				mp.SetAudioStreamType(AudioManager.STREAM_MUSIC);
				mp.Prepare();
				mp.Start();
			}
		}
	}
}
