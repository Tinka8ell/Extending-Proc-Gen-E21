using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

/* ThreadedDataRequester game object (MonoBehaviour)
 * Singleton threaded data requester to decouple creating data from the processing of it
 * Contains a queue of returned data
 * Currently threads request some data using RequestData
 * This starts the creation of the data on a separate thread with a callback action
 * The thread creates the data and enqueues the data and callback
 * Every frame, any queued data is then actioned by that callback
 * 
 * May be an enhancement would be to throttle this and only process the first 'n' on the queue?
 */
public class ThreadedDataRequester : MonoBehaviour {

	static ThreadedDataRequester instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	// Create singleton on Awake()
	void Awake() {
		instance = FindObjectOfType<ThreadedDataRequester> ();
	}

	// Create a delegate to create the data and start it on another thread
	public static void RequestData(Func<object> generateData, Action<object> callback) {
		ThreadStart threadStart = delegate {
			instance.DataThread(generateData, callback);
		};
		new Thread (threadStart).Start ();
	}

	/* Main method of the delegated thread
	 * Create the data requested
	 * Enqueue it and the callback
	 */
	void DataThread(Func<object> generateData, Action<object> callback) {
		object data = generateData ();
		lock (dataQueue) {
			dataQueue.Enqueue(new ThreadInfo (callback, data));
		}
	}
		
	/* Each frame:
	 * Wake up each queue data using it's callback
	 *
	 * may be consider throttling this to only do a maximum on 'n' queued items?
	 */
	void Update() {
		if (dataQueue.Count > 0) {
			for (int i = 0; i < dataQueue.Count; i++) {
				ThreadInfo threadInfo = dataQueue.Dequeue ();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	// What goes on the queue
	struct ThreadInfo {
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo(Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

	}
}
