using System;

namespace Fizz.UI.Core {
	public static class Singleton<T> where T: class {
		public static T Create () {
			if (m_instance == null) {
				m_instance = (T)Activator.CreateInstance (typeof(T), true);
			}
			return m_instance;
		}

		public static void Destroy () {
			m_instance = null;
		}

		public static T instance {
			get {
				return Create ();
			}
		}

		static Singleton() {
		}

		private static T m_instance = null;
	}
}