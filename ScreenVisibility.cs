using System;
using Unity.Mathematics;
using UnityEngine;

public class ScreenVisibility : MonoBehaviour
{
	[SerializeField] Vector3 _viewportPosition;

	private readonly BoolState _isVisible = new BoolState( true );
	public IBoolStateObserver IsVisible => _isVisible;

	void OnEnable()
	{
		ScreenVisibilityCheckSystem.Add( this );
	}

	void OnDisable()
	{
		ScreenVisibilityCheckSystem.Remove( this );
		_isVisible.SetFalse();
	}

	public void SetData( float3 viewportPosition, bool isVisible )
	{
		_viewportPosition = viewportPosition;
		_isVisible.Setter( isVisible );
	}
}