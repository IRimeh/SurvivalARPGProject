using System;
using UnityEngine;

namespace Events
{
	[CreateAssetMenu(fileName = "BaseEvent", menuName = "ScriptableObjects/Events/BaseEvent")]
	public class BaseEvent : ScriptableObject
	{
		private Action _action = delegate { };

		public void Invoke() {
			_action.Invoke();
		}

		public void Register(Action pAction) { 
			_action += pAction;
		}

		public void Unregister(Action pAction) {
			_action -= pAction;
		}
	}
}
