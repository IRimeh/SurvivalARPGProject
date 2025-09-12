using System;
using UnityEngine;

namespace Events
{
	public abstract class ParameterEvent <T> : ScriptableObject
	{
		private Action<T> _action = delegate { };

		public void Invoke(T pParam)
		{
			_action.Invoke(pParam);
		}

		public void Register(Action<T> pAction)
		{
			_action += pAction;
		}

		public void Unregister(Action<T> pAction)
		{
			_action -= pAction;
		}
	}
}
