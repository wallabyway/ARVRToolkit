//
// Copyright (c) Autodesk, Inc. All rights reserved.
// 
// This computer source code and related instructions and comments are the
// unpublished confidential and proprietary information of Autodesk, Inc.
// and are protected under Federal copyright and state trade secret law.
// They may not be disclosed to, copied or used by any third party without
// the prior written consent of Autodesk, Inc.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
#if !UNITY_WSA
using System.Net;
#elif UNITY_WSA
using UnityEngine.Networking;
#endif
using System.Linq;
using UnityEngine;
using SimpleJSON;
using Unity.IO.Compression;

// Defines are UNITY_WSA for Hololens for UnityWebRequest,
//             COROUTINE for using WWW,
//                       for WebClient otherwise
//#if !UNITY_WSA // standard
//#elif UNITY_WSA && !COROUTINE // UnityWebRequest
//#else
//#endif


namespace Autodesk.Forge.ARKit {

	#region Enums
	public enum SceneLoadingStatus {
		eNew,
		ePending,
		eReceived,
		eWaitingMaterial,
		eWaitingTexture,
		eCancelled,
		eError,

		eInstanceTree,
		eMesh,
		eMaterial,
		eTexture,
		eProperties
	}

	#endregion

	#region Events
	public delegate void FireRequestDelegate (object sender, IRequestInterface args);

	#endregion

	#region IRequestInterface Interface
	public interface IRequestInterface {

		#region Properties
		SceneLoadingStatus resolved { get; set; }

		IForgeLoaderInterface loader { get; set; }
		SceneLoadingStatus state { get; set; }
		Uri uri { get; set; }
		DateTime emitted { get; set; }
#if !UNITY_WSA
		WebClient client { get; set; }
#elif UNITY_WSA
		UnityWebRequest client { get; set; }
#endif
		GameObject gameObject { get; set; }
		JSONNode lmvtkDef { get; set; }

		FireRequestDelegate fireRequestCallback { get; set; }

		#endregion

		#region Events
		//event AsyncRequestCompleted AsyncCompleted ;

		#endregion

		#region Interface
		void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null);
#if UNITY_WSA
		IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) ;
#endif
		void CancelRequest ();
		void ProcessResponse (AsyncCompletedEventArgs e);
		string GetName ();
		GameObject BuildScene (string nam, bool saveToDisk = false);

		#endregion

	}

	#endregion

	#region RequestObjectInterface

	public abstract class RequestObjectInterface : IRequestInterface {

		#region Properties
		public SceneLoadingStatus resolved { get; set; }

		public IForgeLoaderInterface loader { get; set; }
		public SceneLoadingStatus state { get; set; }
		public Uri uri { get; set; }
		public DateTime emitted { get; set; }
		public bool compression { get; set;  }
		public string bearer { get; set; }
#if !UNITY_WSA
		public WebClient client { get; set; }
#elif UNITY_WSA
		public UnityWebRequest client { get; set; }
		protected MonoBehaviour mb { get; set; } // The surrogate MonoBehaviour that we'll use to manage this coroutine.
#endif
		public GameObject gameObject { get; set; }
		public JSONNode lmvtkDef { get; set; }

		public FireRequestDelegate fireRequestCallback { get; set; }

		#endregion

		#region Constructors
		public RequestObjectInterface (IForgeLoaderInterface _loader, Uri _uri, string _bearer) {
			state = SceneLoadingStatus.eNew;
			loader = _loader;
			uri = _uri;
			//Debug.Log ("URI request: " + uri); 
			bearer = _bearer;
#if UNITY_WSA
			mb =GameObject.FindObjectOfType<MonoBehaviour> () ;
#endif
			compression = false;
		}

		#endregion

		#region IRequestInterface Interface
#if !UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback = null) {
			emitted = DateTime.Now;
			try {
				using ( client = new WebClient () ) {
					//client.Headers.Add ("Connection", "keep-alive") ;
					if ( callback != null )
						client.DownloadDataCompleted += new DownloadDataCompletedEventHandler (callback);
					if ( !string.IsNullOrEmpty (bearer) )
						client.Headers.Add ("Authorization", "Bearer " + bearer);
					if ( RequestObjectInterface.IsHoloLens || ForgeLoaderConstants._forceHololens )
						client.Headers.Add ("x-ads-device", "hololens");
					if ( RequestObjectInterface.IsDAQRI || ForgeLoaderConstants._forceDAQRI )
						client.Headers.Add ("x-ads-device", "daqri");
					client.Headers.Add ("Keep-Alive", "timeout=15, max=100");
					if ( compression == true )
						client.Headers.Add ("Accept-Encoding", "gzip, deflate");
					state = SceneLoadingStatus.ePending;
					client.DownloadDataAsync (uri, this);
				}
			} catch ( Exception ex ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + ex.Message);
				state = SceneLoadingStatus.eError;
			}
		}
#elif UNITY_WSA
		public virtual void FireRequest (Action<object, AsyncCompletedEventArgs> callback =null) {
			emitted = DateTime.Now;
			mb.StartCoroutine (_FireRequest_ (callback)) ;
		}

		public virtual IEnumerator _FireRequest_ (Action<object, AsyncCompletedEventArgs> callback =null) {
			//using ( client =new UnityWebRequest (uri.AbsoluteUri) ) {
			using ( client =UnityWebRequest.Get (uri.AbsoluteUri) ) {
				//client.SetRequestHeader ("Connection", "keep-alive") ;
				//client.method =UnityWebRequest.kHttpVerbGET ;
				//if ( callback != null )
				//	client.DownloadDataCompleted +=new DownloadDataCompletedEventHandler (callback) ;
				if ( !string.IsNullOrEmpty (bearer) )
					client.SetRequestHeader ("Authorization", "Bearer " + bearer) ;
				if ( RequestObjectInterface.IsHoloLens || ForgeLoaderConstants._forceHololens )
					client.SetRequestHeader ("x-ads-device", "hololens") ;
				if ( RequestObjectInterface.IsDAQRI || ForgeLoaderConstants._forceDAQRI )
					client.SetRequestHeader ("x-ads-device", "daqri") ;
				//client.SetRequestHeader ("Keep-Alive", "timeout=15, max=100");
				if ( compression == true )
					client.SetRequestHeader ("Accept-Encoding", "gzip, deflate");
				state =SceneLoadingStatus.ePending ;
				//client.DownloadDataAsync (uri, this) ;
				#if UNITY_2017_2_OR_NEWER
				yield return client.SendWebRequest () ;
				#else
				yield return client.Send () ;
				#endif

				if ( client.isNetworkError || client.isHttpError ) {
					Debug.Log (ForgeLoader.GetCurrentMethod () + " " + client.error + " - " + client.responseCode) ;
					state =SceneLoadingStatus.eError ;
				} else {
					//client.downloadHandler.data
					//client.downloadHandler.text
					if ( callback != null ) {
						DownloadDataCompletedEventArgs args =new DownloadDataCompletedEventArgs (null, false, this) ;
						args.Result =client.downloadHandler.data ;
						callback (this, args) ;
					}
				}
			}
		}
#endif

		public void CancelRequest () {
			state = SceneLoadingStatus.eCancelled;
#if !UNITY_WSA
			client.CancelAsync ();
#elif UNITY_WSA
			//client.Abort () ;
#endif
		}

		public abstract void ProcessResponse (AsyncCompletedEventArgs e);

		public abstract string GetName ();

		public virtual GameObject BuildScene (string name, bool saveToDisk = false) {
			state = resolved;
			return (gameObject);
		}

		#endregion

		#region Methods
		public static string buildName (string tp, int dbid, int fragId, string pathid) {
			return (String.Join ("-", new string [] { tp, dbid.ToString (), fragId.ToString (), pathid }));
		}

		public static string decodeName (string name, ref int dbid, ref int fragId, ref string pathid) {
			string [] arr = name.Split (new char [1] { '-' });
			dbid = System.Convert.ToInt32 (arr [1]);
			fragId = System.Convert.ToInt32 (arr [2]);
			pathid = arr [3]; //.Replace ("_", ":") ;
			return (arr [0]);
		}

		public static bool IsHoloLens {
			get { return (SystemInfo.deviceModel.ToUpperInvariant ().Contains ("HOLOLENS")); }
		}

		public static bool IsDAQRI {
			get { return (SystemInfo.deviceModel.ToUpperInvariant ().Contains ("DAQRI")); }
		}

		public static byte[] Decompress (byte[] gzip) {
			// Create a GZIP stream with decompression mode.
			// ... Then create a buffer and write into while reading from the GZIP stream.
			using ( GZipStream stream =new GZipStream (new MemoryStream (gzip), CompressionMode.Decompress) ) {
				const int size =4096 ;
				byte [] buffer =new byte [size] ;
				using ( MemoryStream memory =new MemoryStream () ) {
					int count =0 ;
					do {
						count =stream.Read (buffer, 0, size) ;
						if ( count > 0 )
							memory.Write (buffer, 0, count) ;
					}
					while ( count > 0 ) ;
					return (memory.ToArray ()) ;
				}
			}
		}

		#endregion

	}

	#endregion

	#region RequestQueueMgr
	public class RequestQueueMgr : IEnumerable {

		#region Fields
		protected List<IRequestInterface> _queue = new List<IRequestInterface> ();

		#endregion

		#region Constructors
		public RequestQueueMgr () { }

		~RequestQueueMgr () {
			foreach ( var item in _queue )
				item.CancelRequest ();
		}

		#endregion

		#region RequestQueueMgr Methods
		public void Add (IRequestInterface item) {
			_queue.Add (item);
			item.FireRequest (AsyncRequestCompleted);
		}

		public void Register (IRequestInterface item) {
			_queue.Add (item);
		}

		public void FireRequest (IRequestInterface item) {
			item.FireRequest (AsyncRequestCompleted);
		}

		public void AsyncRequestCompleted (object sender, AsyncCompletedEventArgs args) {
			if ( args == null || args.UserState == null )
				return;
			IRequestInterface item = args.UserState as IRequestInterface;
			if ( args.Error != null ) {
				Debug.Log (ForgeLoader.GetCurrentMethod () + " " + args.Error.Message);
				item.state = SceneLoadingStatus.eError;
				return;
			}
			//item.state =item.resolved ;
			item.ProcessResponse (args);
		}

		#endregion

		#region IEnumerable
		IEnumerator IEnumerable.GetEnumerator () {
			return ((IEnumerator)GetEnumerator ());
		}

		public RequestQueueMgrEnumerator GetEnumerator () {
			return (new RequestQueueMgrEnumerator (_queue));
		}

		public RequestQueueMgrEnumerator GetNotPendingEnumerator () {
			return (new RequestQueueMgrEnumerator (_queue.Where (x => x.state != SceneLoadingStatus.ePending).ToList ()));
		}

		public RequestQueueMgrEnumerator GetCompletedEnumerator () {
			return (new RequestQueueMgrEnumerator (_queue.Where (x => x.state == SceneLoadingStatus.eReceived).ToList ()));
		}

		public RequestQueueMgrEnumerator GetBuiltEnumerator () {
			return (new RequestQueueMgrEnumerator (_queue.Where (x => (x.state == SceneLoadingStatus.eInstanceTree || x.state == SceneLoadingStatus.eMesh || x.state == SceneLoadingStatus.eMaterial)).ToList ()));
		}

		public RequestQueueMgrEnumerator GetTypeEnumerator (SceneLoadingStatus which = SceneLoadingStatus.ePending) {
			return (new RequestQueueMgrEnumerator (_queue.Where (x => x.state == which).ToList ()));
		}

		public int Count () {
			return (_queue.Where (x => 1 == 1).Count ());
		}

		public int Count (SceneLoadingStatus which) {
			return (_queue.Where (x => x.state == which).Count ());
		}

		public int Count (SceneLoadingStatus [] which) {
			return (_queue.Where (x => which.Contains (x.state)).Count ());
		}

		#endregion

	}

	#endregion

	#region RequestQueueMgrEnumerator
	public class RequestQueueMgrEnumerator : IEnumerator {

		#region Fields
		public List<IRequestInterface> _queue;

		// Enumerators are positioned before the first element
		// until the first MoveNext() call.
		protected int _position = -1;

		#endregion

		#region Constructors
		public RequestQueueMgrEnumerator (List<IRequestInterface> queue) {
			_queue = queue;
		}

		#endregion

		#region IEnumerator
		public bool MoveNext () {
			_position++;
			return (_position < _queue.Count);
		}

		public void Reset () {
			_position = -1;
		}

		object IEnumerator.Current {
			get {
				return (Current);
			}
		}

		public IRequestInterface Current {
			get {
				try {
					return (_queue.ToArray () [_position]);
				} catch ( IndexOutOfRangeException ) {
					throw new InvalidOperationException ();
				}
			}
		}

		#endregion

	}

	#endregion

}