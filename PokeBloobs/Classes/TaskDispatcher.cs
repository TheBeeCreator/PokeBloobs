using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PokeBloobs.Classes
{
    public class TaskDispatcher : MonoBehaviour
    {
        public static TaskDispatcher Instance { get; private set; }

        private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void Enqueue(Action action) => _queue.Enqueue(action);

        void Update()
        {
            while (_queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }

        public static void RunCoroutine(IEnumerator routine)
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(routine);
            }
            else
            {
                Debug.LogError("TaskDispatcher Instance is null! Make sure it's attached to a GameObject.");
            }
        }
    }
}
