using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSN
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Messenger : MonoBehaviour
    {
        private static Messenger _events;
        /// <summary>
        /// The instance of the messenger.
        /// </summary>
        public static Messenger Events
        {
            get
            {
                if (_events == null)
                    _events = new Messenger();

                return _events;
            }

            set { }
        }

        private static List<Action<object[]>> _handlers = new List<Action<object[]>> ();
        public static List<Action<object[]>> Handlers { get { return _handlers; } }

        public void Start()
        {
            Events = this;
        }

        /// <summary>
        /// Send a global event to all listeners.
        /// </summary>
        /// <param name="name">Name of the event.</param>
        /// <param name="args">Arguments of the event.</param>
        public void Send(string name, object[] args)
        {
            _handlers.Find(d => d.Method.Name == name).DynamicInvoke(args);
        }

        /// <summary>
        /// Adds a method as a listener function.
        /// </summary>
        /// <param name="method">The method to add as listener.</param>
        /// <returns>The instance of the messenger.</returns>
        public static Messenger Add(Action<object[]> method)
        {
            if (!_handlers.Contains(method))
                _handlers.Add(method);

            return Events;
        }

        /// <summary>
        /// Adds a method as a listener function.
        /// </summary>
        /// <param name="messenger">The instance of the messenger.</param>
        /// <param name="method">The method to add as listener.</param>
        /// <returns>The instance of the messenger.</returns>
        public static Messenger operator +(Messenger messenger, Action<object[]> method)
        {
            return Add(method);
        }

        /// <summary>
        /// Removes a method as a listener function.
        /// </summary>
        /// <param name="method">The method to remove as listener.</param>
        /// <returns>The instance of the messenger.</returns>
        public static Messenger Remove(Action<object[]> method)
        {
            if (_handlers.Contains(method))
                _handlers.Remove(method);

            return Events;
        }

        /// <summary>
        /// Removes a method as a listener function.
        /// </summary>
        ///       /// <param name="messenger">The instance of the messenger.</param>
        /// <param name="method">The method to remove as listener.</param>
        /// <returns>The instance of the messenger.</returns>
        public static Messenger operator -(Messenger messenger, Action<object[]> method)
        {
            return Remove(method);
        }
    }
}
