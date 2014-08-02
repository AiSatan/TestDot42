using System;
using System.Threading;
using Android.App;
using Android.Os;
using Android.Util;
using Android.Widget;
using Dot42;
using Dot42.Manifest;
using Java.Io;
using Java.Net;
using Java.Util;
using Org.Apache.Http.Conn.Util;
using AiSatanDevice;
using Android.Content;
using Android.View;

[assembly: Application("TestDot42")]

// these replace similar entries in AndroidManifest.xml
[assembly: UsesPermission(Android.Manifest.Permission.INTERNET)]
[assembly: UsesPermission(Android.Manifest.Permission.ACCESS_NETWORK_STATE)]
[assembly: UsesPermission(Android.Manifest.Permission.BATTERY_STATS)]
[assembly: UsesPermission(Android.Manifest.Permission.MODIFY_AUDIO_SETTINGS)]

namespace AiSatanDevice
{
	[Activity]
	public class MainActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(R.Layouts.MainLayout);

			var btnStart = (Button)FindViewById(R.Ids.Start);
			btnStart.Click += OnClickStart;

			var btnStop = (Button)FindViewById(R.Ids.Stop);
			btnStop.Click += OnClickStop;

			player = new AiPlayer();
			player.Start();
		}

		private void OnClickStop(object sender, EventArgs e)
		{
			StopService(new Intent(this, typeof(HttpService)));
		}

		private void OnClickStart(object sender, EventArgs e)
		{
			StartService(new Intent(this, typeof(HttpService)));
		}

		internal AiPlayer player { get; set; }
	}
}