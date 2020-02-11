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

		bool becomeVisible = (isVisible && CanShow.Free) || (isVisible && ignoreCanShow);
		UpdateVisibility( becomeVisible );
	}

	void UpdateVisibility(bool becomeVisible )
	{
		_isVisible.Setter( becomeVisible );
	}
}