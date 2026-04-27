using System;
using System.Collections;
using UnityEngine;

public class VersionChecker : Singleton<VersionChecker>
{
	public float TimeBetweenChecks;

	private bool mChecking;

	private float mCheckTimer = -1f;

	private void Update()
	{
		if (!Singleton<TownEnvironmentHolder>.Instance.Loaded || !Singleton<TownController>.Instance.IsIntroDone() || mChecking)
		{
			return;
		}
		if (mCheckTimer <= 0f)
		{
			if (!Singleton<MouseOrbitCamera>.Instance.IsZoomedInToBuilding())
			{
				CheckVersion();
			}
		}
		else
		{
			mCheckTimer -= Time.deltaTime;
		}
	}

	public void CheckVersion(Action callbackWhenUpToDate = null)
	{
		StartCoroutine(CheckVersionCo(callbackWhenUpToDate));
	}

	private IEnumerator CheckVersionCo(Action callbackWhenUpToDate)
	{
		//Skip version checking
		mChecking = true;
		Singleton<BusyIconPanelController>.Instance.Show();
		mCheckTimer = TimeBetweenChecks;
		mChecking = false;
		Singleton<BusyIconPanelController>.Instance.Hide();
		if (callbackWhenUpToDate != null)
		{
			callbackWhenUpToDate();
		}
		yield break;
	}

}
