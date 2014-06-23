﻿using UnityEngine;
using System.Collections;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

using MiniJSON;
using System.Collections.Generic;

public class client : MonoBehaviour {
	//UDP/IPの設定
	public int myport;
	public string IP;
	public int    port;
	private UdpClient 	udpip = null;
	private IPEndPoint 	remoteEP   = null;
	
	//ストリーム
	private  string stream = "";
	private byte[] res = new byte[10000];
	
	//Jsonデータ
	private string json;
	private string status;
	private string obj_name;
	public Vector3 pos;
	public Vector3 rote;
	
	//マルチスレッド用
	private Thread read_thread;
	
	//一時保存変数
	private Vector3 tmp_p;
	private Vector3 tmp_r;
	private Vector3 read_tmp_p = Vector3.zero;
	private Vector3 read_tmp_r = Vector3.zero;
	
	public bool send = false;
	public bool read = false;
	
	private void read_stream(){//**マルチスレッド関数**
		Debug.Log("Start read stream!");
		while(true){
			//マルチスレッドの速度？
			Thread.Sleep(10);
			//ストリームの受信
			stream = read_message();
			Debug.Log(stream);
			var jsonData = MiniJSON.Json.Deserialize(stream) as Dictionary<string,object>;
			if(jsonData != null){
				//pos  = new Vector3(float.Parse(jsonData["x_p"].ToString()),float.Parse(jsonData["y_p"].ToString()),float.Parse(jsonData["z_p"].ToString()));
				//rote = new Vector3(float.Parse(jsonData["x_r"].ToString()),float.Parse(jsonData["y_r"].ToString()),float.Parse(jsonData["z_r"].ToString()));
				
				read_tmp_p = new Vector3(float.Parse(jsonData["x_p"].ToString()),float.Parse(jsonData["y_p"].ToString()),float.Parse(jsonData["z_p"].ToString()));
				read_tmp_r = new Vector3(float.Parse(jsonData["x_r"].ToString()),float.Parse(jsonData["y_r"].ToString()),float.Parse(jsonData["z_r"].ToString()));
				if(read_tmp_p == pos && read_tmp_r == rote){
					read = false;
				}else{
					pos = read_tmp_p;
					rote = read_tmp_r;
					read = true;
				}
				
			}else{
				Debug.Log("Not Json");
			}
		}
	}
	
	private bool initialize_cilent(){
		//UDP/IPの初期化
		udpip = new UdpClient(myport);
		if (udpip == null) {
			Debug.Log("Make client fall.");
			return false;
		} else {
			Debug.Log("Make client success.");
			try{
				remoteEP = new IPEndPoint(IPAddress.Parse(IP),port);
				udpip.Connect(remoteEP);
				Debug.Log("Connect success.");
				return true;
			}catch(WebException e){
				Debug.Log(e);
				return false;
			}
		}
	}
	
	private void close_cilent(){
		if (udpip != null) {
			udpip.Close();
		} else {
			Debug.Log("Error");
		}
	}
	
	private void initialize_thread(){
		read_thread = new Thread(new ThreadStart(read_stream));
		if (read_thread == null) {
			Debug.Log ("Error");
		} else {
			read_thread.Start();
		}
	}
	
	private void close_thread(){
		if (read_thread != null) {
			read_thread.Abort(); 
		} else {
			Debug.Log("Error");
		}
	}
	
	private void send_massage(string text){
		if (read == false) {
			byte[] send_byte = Encoding.UTF8.GetBytes (text);
			udpip.Send(send_byte,send_byte.Length);
		} else {
			Debug.Log("Get stream fall.");
		}
	}

	private string read_message(){
		res = udpip.Receive(ref remoteEP);
		return System.Text.Encoding.Default.GetString(res);
	}
	
	private string make_json(string status_mode){
		json = "{" +	"\"type\":\"" + status_mode + 		"\"," +
			"\"name\":\"" + gameObject.name + 	"\"";
		switch (status_mode){
		case "setup":
			status = "send";
			break;
		case "send":
			json += ",";
			json +=	"\"x_p\":" + transform.position.x + 	"," +
				"\"y_p\":" + transform.position.y + 	"," +
					"\"z_p\":" + transform.position.z + 	"," +
					"\"x_r\":" + transform.eulerAngles.x + 	"," +
					"\"y_r\":" + transform.eulerAngles.y + 	"," +
					"\"z_r\":" + transform.eulerAngles.z;
			break;
		}
		json += "}";
		
		//Debug.Log(json);
		return json;
	}
	
	// Use this for initialization
	void Awake () {
		myport = 1000 + int.Parse(gameObject.name);
		status = "setup";
		obj_name = gameObject.name;
		tmp_p = transform.position;
		tmp_r = transform.eulerAngles;
		
		initialize_cilent();
		initialize_thread();
	}
	
	void Start(){
		//send_massage(gameObject.name);
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position != tmp_p || transform.eulerAngles != tmp_r) {
			tmp_p = transform.position;
			tmp_r = transform.eulerAngles;
			send_massage (make_json (status));
			pos = tmp_p;
			rote = tmp_r;
			
			read = true;
		} else {
			read = false;
		}
		
		transform.position = pos;
		transform.eulerAngles = rote;
	}
	
	void OnApplicationQuit() {
		close_thread ();
		close_cilent ();
		Debug.Log("exit");
	}
}