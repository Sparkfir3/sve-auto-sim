using UnityEngine;

namespace Sparkfire.Utility
{
	[DefaultExecutionOrder(-5000)]
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		[field: SerializeField]
#if ODIN_INSPECTOR
		[field: Sirenix.OdinInspector.LabelText("Use DontDestroyOnLoad")]
#endif
		public bool UseDontDestroyOnLoad { get; private set; } = true;

		protected virtual void Awake()
		{
			if(Instance != null && Instance != this as T)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this as T;
			
			if(UseDontDestroyOnLoad)
			{
				transform.parent = null;
				DontDestroyOnLoad(gameObject);
			}
		}

		protected virtual void OnDestroy()
		{
			if(Instance != this)
				return;
			Instance = null;
		}
	}
}
