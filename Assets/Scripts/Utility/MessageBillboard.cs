using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MessageBucket = System.Collections.Generic.List<MessageBillboard.Message>;
using MessageBucketPool = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<MessageBillboard.Message>>;

public class MessageBillboard : SingletonBehaviour<MessageBillboard> {

	public class Message : UnityEngine.Object
	{
		public string ID { get { return GetType().ToString(); } }		
	}

	MessageBucketPool readPool = new MessageBucketPool();
	MessageBucketPool writePool = new MessageBucketPool();

	MessageBucket GetBucket(MessageBucketPool pool, string key, bool create = true)
	{
		MessageBucket queue = null;
		if(pool.TryGetValue(key, out queue) == false && create)
		{
			queue = new MessageBucket();
			pool.Add(key, queue);
		}
		return queue;
	}

	public void AddMessage(Message message)
	{
		MessageBucket queue = GetBucket(writePool, message.ID);
		queue.Add(message);
	}

	public List<MessageType> GetMessages<MessageType>()
	{
		MessageBucket bucket = GetBucket(readPool, typeof(MessageType).ToString());
		return bucket.Cast<MessageType>().ToList();
	}

	public MessageBucket GetAllMessages()
	{
		MessageBucket allMessages = new MessageBucket();
		foreach(var pair in readPool)
		{
			allMessages.AddRange(pair.Value);
		}
		return allMessages;
	}

	float debugTick = 0;
	private void LateUpdate()
	{
		//at the end of every frame we should flip the pools
		var temp = readPool;
		readPool = writePool;
		writePool = temp;

		//clear the new write pool for next frame
		writePool.Clear();

		//Dump some debug every so often
        /*
		debugTick += Time.deltaTime;
		if(debugTick > 0.0f)
		{
			Debug.Log(string.Format("Billboard read({0}) write({1})", readPool.Count, writePool.Count));
		}
        */
	}

}
