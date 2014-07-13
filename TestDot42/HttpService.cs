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

namespace TestDot42
{
	[Service]
	public class HttpService : Service
	{
		private const int PORT = 8088;
		private bool stop = false;

		public override void OnStart(Intent intent, int startId)
		{
			base.OnStart(intent, startId);
		}

		/// <summary>
		/// Stop the server.
		/// </summary>
		public override void OnDestroy()
		{
			ShowMessage("STOP: " + GetIPAddress() + ":" + PORT);
			Toast.MakeText(this, "Server STOP width: " + GetIPAddress() + ":" + PORT, Toast.LENGTH_LONG).Show();
			stop = true;
			base.OnDestroy();
		}

		public override int OnStartCommand(Intent intent, int flags, int startId)
		{
			notificationManager = (NotificationManager)GetSystemService(NOTIFICATION_SERVICE);
			var thread = new Thread(new ThreadStart(RunServer));
			thread.Start();
			ShowMessage("RUN: " + GetIPAddress() + ":" + PORT, true);
			Toast.MakeText(this, "RUN: " + GetIPAddress() + ":" + PORT, Toast.LENGTH_LONG).Show();
			return START_STICKY;
		}

		private void ShowMessage(string message, bool isNow = false)
		{
			var notif = new Notification(R.Drawables.common_signin_btn_icon_pressed_dark, message, Java.Lang.System.CurrentTimeMillis());

			Intent notificationIntent = new Intent(this, typeof(MainActivity));

			// 3-я часть
			Intent intent = new Intent(this, typeof(MainActivity));
			intent.PutExtra("HttpServer", "somefile");
			PendingIntent pIntent = PendingIntent.GetActivity(this, 0, intent, 0);

			// 2-я часть
			notif.SetLatestEventInfo(this, "HttpServer", message, pIntent);

			if (!isNow)
			{
				// ставим флаг, чтобы уведомление пропало после нажатия
				notif.Flags = Notification.FLAG_AUTO_CANCEL;
			}
			else
			{
				notif.Flags = Notification.FLAG_FOREGROUND_SERVICE;
			}

			// отправляем
			notificationManager.Notify(1, notif);
		}

		public class BRRec : BroadcastReceiver
		{
			public string bStatus = "0";
			public override void OnReceive(Context context, Intent intent)
			{
				bStatus = intent.GetIntExtra(BatteryManager.EXTRA_LEVEL, -1).ToString();
			}
		}

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		/// <summary>
		/// Run a very simple server.
		/// </summary>
		private void RunServer()
		{
			var ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);

			var receiver = new BRRec();

			RegisterReceiver(receiver, ifilter);

			var t = receiver.bStatus;

			try
			{
				//ShowMessage("SimpleHttpServer: Creating server socket");
				var serverSocket = new ServerSocket(PORT);
				var intent = new Intent(Intent.ACTION_MEDIA_BUTTON);
				var tScript = "";
				var imgs = "";
				var rand = new Random();
				var tmp = rand.Next(00000, 99999);
				try
				{
					while (!stop)
					{
						//ShowMessage("SimpleHttpServer: Waiting for connection");
						var socket = serverSocket.Accept();

						var input = new BufferedReader(new InputStreamReader(socket.GetInputStream()));
						var output = new BufferedWriter(new OutputStreamWriter(socket.GetOutputStream()));

						string line;
						string header = "";
						//status.Text += "SimpleHttpServer: Reading request" + "\r\n";

						while ((line = input.ReadLine()) != null)
						{
							header += line + "<br/>";
							var tl = tmp.ToString();
							//status.Text += "SimpleHttpServer: Received: " + line + "\r\n";
							if (line.IndexOf(tl) > 0)
							{
								if (line.IndexOf("GET /?n") == 0)
								{
									imgs = ResTrack(intent, KeyEvent.KEYCODE_MEDIA_NEXT);
								}
								else if (line.IndexOf("GET /?p") == 0)
								{
									imgs = ResTrack(intent, KeyEvent.KEYCODE_MEDIA_PREVIOUS);
								}
								else if (line.IndexOf("GET /?s") == 0)
								{
									imgs = ResTrack(intent, KeyEvent.KEYCODE_MEDIA_PLAY_PAUSE);
								}
							}
							if (line.IndexOf("favicon") > 0)
							{
								break;
							}
							tmp = rand.Next(00000, 99999);
							if (line.Length == 0)
								break;
						}

						var localUri = GetIPAddress() + ":" + PORT;
						//ShowMessage(localUri);
						//status.Text += "SimpleHttpServer: Sending response" + "\r\n";
						output.Write(
								@"HTTP/1.1 200 OK

									<!DOCTYPE HTML>
									<html>
										<head>
											<meta charset='utf-8'>
											<title>Переключатель треков для AiSatanDevice</title>
										</head>
									" + style + @"
										<body>
											<div class='container'>
													<h1>AiSatanDevice Admin Panel: </h1>
													<p>BatteryStatus: " + receiver.bStatus + @"</p>
													<a href='/?p" + tmp + @"' class='button button-gray'>Previous</a>
													<a href='/?s" + tmp + @"' class='button button-red'>Play/Pause</a>
													<a href='/?n" + tmp + @"' class='button button-green'>Next</a>
													<p>Input: </p>
													<br/>
													<p> " + header + @"</p>
											</div>
<div class='container'>
										<img src='" + imgs + @"'>
</div>
										</body>
									" + tScript + @"
									</html>
									");
						output.Flush();
						tScript = "";
						socket.Close();
					}
				}
				finally
				{
					serverSocket.Close();
				}
			}
			catch (Exception ex)
			{
				ShowMessage(ex.Message, true);
			}
		}

		private string ResTrack(Intent intent, int KEYCODE)
		{
			intent.PutExtra(Intent.EXTRA_KEY_EVENT, new KeyEvent(KeyEvent.ACTION_DOWN, KEYCODE));
			SendOrderedBroadcast(intent, null);

			intent.PutExtra(Intent.EXTRA_KEY_EVENT, new KeyEvent(KeyEvent.ACTION_UP, KEYCODE));
			SendOrderedBroadcast(intent, null);
			var status = "";
			switch (KEYCODE)
			{
				case KeyEvent.KEYCODE_MEDIA_PLAY_PAUSE:
					status = "http://www.holyblasphemy.net/wp-content/uploads/2012/01/satan.jpg";
					break;
				case KeyEvent.KEYCODE_MEDIA_NEXT:
					status = "http://cdn1-www.craveonline.com/assets/uploads/2013/06/Youre-Next-Vinson.jpg";
					break;
				case KeyEvent.KEYCODE_MEDIA_PREVIOUS:
					status = "http://atkritka.com/upload/iblock/820/atkritka_1386179601_43.jpg";
					break;
				default:
					return "";
			}
			return status;
		}

		private string GetIPAddress()
		{
			var list = Collections.List(NetworkInterface.GetNetworkInterfaces());
			foreach (var intf in list.AsEnumerable())
			{
				if (intf.IsLoopback())
					continue;
				var addresses = Collections.List(intf.GetInetAddresses());
				foreach (var addr in addresses)
				{
					if (InetAddressUtils.IsIPv4Address(addr.GetHostAddress()))
						return addr.GetHostAddress();
				}
			}
			return "?";
		}

		private string style = @"<style>

		@import url(http://cdnjs.cloudflare.com/ajax/libs/meyer-reset/2.0/reset.css);

		body {
		  font: 13px/20px 'Lucida Grande', Verdana, sans-serif;
		  color: #404040;
		  background: white;
		}

		.button {
		  position: relative;
		  display: inline-block;
		  vertical-align: top;
		  height: 36px;
		  line-height: 35px;
		  padding: 0 20px;
		  font-size: 13px;
		  color: white;
		  text-align: center;
		  text-decoration: none;
		  text-shadow: 0 -1px rgba(0, 0, 0, 0.4);
		  background-clip: padding-box;
		  border: 1px solid;
		  border-radius: 2px;
		  cursor: pointer;
		  -webkit-box-shadow: inset 0 1px rgba(255, 255, 255, 0.1), inset 0 0 0 1px rgba(255, 255, 255, 0.08), 0 1px 2px rgba(0, 0, 0, 0.25);
		  box-shadow: inset 0 1px rgba(255, 255, 255, 0.1), inset 0 0 0 1px rgba(255, 255, 255, 0.08), 0 1px 2px rgba(0, 0, 0, 0.25);
		}

		.button:before {
		  content: '';
		  position: absolute;
		  top: 0;
		  bottom: 0;
		  left: 0;
		  right: 0;
		  pointer-events: none;
		  background-image: -webkit-radial-gradient(center top, farthest-corner, rgba(255, 255, 255, 0.08), rgba(255, 255, 255, 0));
		  background-image: -moz-radial-gradient(center top, farthest-corner, rgba(255, 255, 255, 0.08), rgba(255, 255, 255, 0));
		  background-image: -o-radial-gradient(center top, farthest-corner, rgba(255, 255, 255, 0.08), rgba(255, 255, 255, 0));
		  background-image: radial-gradient(center top, farthest-corner, rgba(255, 255, 255, 0.08), rgba(255, 255, 255, 0));
		}

		.button:hover:before {
		  background-image: -webkit-radial-gradient(farthest-corner, rgba(255, 255, 255, 0.18), rgba(255, 255, 255, 0.03));
		  background-image: -moz-radial-gradient(farthest-corner, rgba(255, 255, 255, 0.18), rgba(255, 255, 255, 0.03));
		  background-image: -o-radial-gradient(farthest-corner, rgba(255, 255, 255, 0.18), rgba(255, 255, 255, 0.03));
		  background-image: radial-gradient(farthest-corner, rgba(255, 255, 255, 0.18), rgba(255, 255, 255, 0.03));
		}

		.button:active {
		  -webkit-box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.2);
		  box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.2);
		}

		.button:active:before {
		  content: none;
		}

		.button-green {
		  background: #5ca934;
		  border-color: #478228 #478228 #3c6f22;
		  background-image: -webkit-linear-gradient(top, #69c03b, #5ca934 66%, #54992f);
		  background-image: -moz-linear-gradient(top, #69c03b, #5ca934 66%, #54992f);
		  background-image: -o-linear-gradient(top, #69c03b, #5ca934 66%, #54992f);
		  background-image: linear-gradient(to bottom, #69c03b, #5ca934 66%, #54992f);
		}

		.button-green:active {
		  background: #5ca934;
		  border-color: #3c6f22 #478228 #478228;
		}

		.button-red {
		  background: #d5452f;
		  border-color: #ae3623 #ae3623 #992f1f;
		  background-image: -webkit-linear-gradient(top, #da5c48, #d5452f 66%, #c73d28);
		  background-image: -moz-linear-gradient(top, #da5c48, #d5452f 66%, #c73d28);
		  background-image: -o-linear-gradient(top, #da5c48, #d5452f 66%, #c73d28);
		  background-image: linear-gradient(to bottom, #da5c48, #d5452f 66%, #c73d28);
		}

		.button-red:active {
		  background: #d5452f;
		  border-color: #992f1f #ae3623 #ae3623;
		}

		.button-gray {
		  background: #47494f;
		  border-color: #2f3034 #2f3034 #232427;
		  background-image: -webkit-linear-gradient(top, #55585f, #47494f 66%, #3d3f44);
		  background-image: -moz-linear-gradient(top, #55585f, #47494f 66%, #3d3f44);
		  background-image: -o-linear-gradient(top, #55585f, #47494f 66%, #3d3f44);
		  background-image: linear-gradient(to bottom, #55585f, #47494f 66%, #3d3f44);
		}

		.button-gray:active {
		  background: #47494f;
		  border-color: #232427 #2f3034 #2f3034;
		}

		.container {
		  margin: 30px auto;
		  width: 580px;
		  text-align: center;
		}

		.container > .button { margin: 12px; }

	</style>";

		public NotificationManager notificationManager { get; set; }
	}
}
