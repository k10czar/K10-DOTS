using System;
using Unity.Mathematics;
using UnityEngine;

public class ScreenVisibility : MonoBehaviour
{
	[SerializeField] Vector3 _viewportPosition;

	private readonly BoolState _isVisible = new BoolState( true );
	public IBoolStateObserver IsVisible => _isVisible;

	private readonly Semaphore _canShow = new Semaphore();
	public ISemaphore CanShow => _canShow;

	private readonly StateRequester _lockValue = new StateRequester();
	public IStateRequesterInfo LockValueState => _lockValue;
	private bool _lockedValue;

	private void Awake()
	{
		_canShow.OnFalseState.Register( () => UpdateVisibility(false) );
	}

	void OnEnable()
	{
		ScreenVisibilityCheckSystem.Add( this );
	}

	void OnDisable()
	{
		ScreenVisibilityCheckSystem.Remove( this );
		_isVisible.SetFalse();
	}

	public void SetData( float3 viewportPosition, bool isVisible, bool ignoreCanShow = false )
	{
		_viewportPosition = viewportPosition;

		bool becomeVisible = _lockValue.Requested ? _lockedValue : ( isVisible && CanShow.Free) || (isVisible && ignoreCanShow);
		UpdateVisibility( becomeVisible );
	}

	void UpdateVisibility(bool becomeVisible )
	{
		_isVisible.Setter( becomeVisible );
	}

	public void LockValue(bool lockedValue, object key)
	{
		_lockValue.Request( key );
		_lockedValue = lockedValue;
	}

	public void RemoveLockValue(object key){
		_lockValue.RemoveRequest( key );
	}
}