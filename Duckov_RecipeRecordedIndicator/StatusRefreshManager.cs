using System;
using System.Collections;
using UnityEngine;

namespace Duckov_RecipeRecordedIndicator
{
    public class StatusRefreshManager : MonoBehaviour
    {
        private static StatusRefreshManager? _instance;

        private Coroutine? _refreshCoroutine;
        private bool _stopped;

        public static StatusRefreshManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var obj = new GameObject("StatusRefreshManager");
                _instance = obj.AddComponent<StatusRefreshManager>();
                DontDestroyOnLoad(obj);
                return _instance;
            }
        }

        private void Start()
        {
            _stopped = false;
            _refreshCoroutine = StartCoroutine(RefreshCoroutine());
        }

        private void OnDestroy()
        {
            if (_refreshCoroutine == null) return;
            _stopped = true;
            StopCoroutine(_refreshCoroutine);
            _refreshCoroutine = null;
        }

        public static void DestroyInstance()
        {
            if (_instance == null) return;
            Destroy(_instance.gameObject);
            _instance = null;
        }

        public event Action? OnTriggerRefresh;

        private IEnumerator RefreshCoroutine()
        {
            while (!_stopped)
            {
                yield return new WaitForSeconds(1);
                OnRefresh();
            }
        }

        private void OnRefresh()
        {
            OnTriggerRefresh?.Invoke();
        }
    }
}