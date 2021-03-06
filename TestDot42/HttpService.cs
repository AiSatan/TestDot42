﻿using System;
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

namespace AiSatanDevice
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

			if (isNow)
			{
				notif.Flags = Notification.FLAG_NO_CLEAR;
			}
			else
			{
				notificationManager.Cancel(1);
				return;
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
								else if (line.IndexOf("GET /?r") == 0)
								{
									imgs = ResTrack(intent, KeyEvent.KEYCODE_MEDIA_PLAY_PAUSE);
								}
								else if (line.IndexOf("GET /?s") == 0)
								{
									imgs = ResTrack(intent, KeyEvent.KEYCODE_MEDIA_STOP);
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
						var bColor = "#00FF00";
						if(int.Parse(receiver.bStatus) < 50)
						{
							bColor = "#FFD600";
						}
						else if (int.Parse(receiver.bStatus) < 20)
						{
							bColor = "#FF0000";
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
											<title>AiSatanDevice Admin Panel</title>
										</head>
										<body>
											<script src='//code.jquery.com/jquery-2.1.0.min.js'></script>
											<script src='//maxcdn.bootstrapcdn.com/bootstrap/3.2.0/js/bootstrap.min.js'></script>
											<link rel='stylesheet' href='//maxcdn.bootstrapcdn.com/bootstrap/3.2.0/css/bootstrap.min.css'>
											<div class='container'>
													<h1 class='text-center'>AiSatanDevice Admin Panel: </h1>
													
													<div class='progress'>
														<div id='batteryStats' class='progress-bar progress-bar-success' role='progressbar' aria-valuenow='" + receiver.bStatus + @"' aria-valuemin='0' aria-valuemax='100' style='width: " + receiver.bStatus + @"%'>
															<span>Battery: " + receiver.bStatus + @"%</span>													  
														</div>
													</div>
													<div class='text-center'>
														<div class='btn-group'>
															<a href='/?p" + tmp + @"' class='btn btn-warning'>
																<span class='glyphicon glyphicon-fast-backward'></span>
																Previous
															</a>
															<a href='/?r" + tmp + @"' class='btn btn-success'>
																<span class='glyphicon glyphicon-play'></span>
																Play/Pause
															</a>
															<a href='/?s" + tmp + @"' class='btn btn-danger'>
																<span class='glyphicon glyphicon glyphicon-stop'></span>
																Stop
															</a>
															<a href='/?n" + tmp + @"' class='btn btn-primary'>
																<span class='glyphicon glyphicon-fast-forward'></span>
																Next
															</a>
														</div>
													</div>
													<div class='progress' style='width: 5%; background-color: #000000; height: 100px'>
														<div id='batteryStats' style='background-color: " + bColor + @"; width:100%; height: " + (100-int.Parse(receiver.bStatus)) + @"%' class='progress-bar progress-bar-success' role='progressbar' aria-valuenow='" + receiver.bStatus + @"' aria-valuemin='0' aria-valuemax='100'>
															<span>Battery: " + receiver.bStatus + @"%</span>													  
														</div>
													</div>
											</div>
										</body>
										<script>
$(document).ready(function() {
	if(" + receiver.bStatus + @" < 50)
	{
		$('#batteryStats').addClass('progress-bar-warning').removeClass('progress-bar-success');
	}
	if(" + receiver.bStatus + @" < 20)
	{
		$('#batteryStats').addClass('progress-bar-danger').removeClass('progress-bar-warning');
	}
});
									" + tScript + @"
									</script>
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
		public NotificationManager notificationManager { get; set; }
	}
}
