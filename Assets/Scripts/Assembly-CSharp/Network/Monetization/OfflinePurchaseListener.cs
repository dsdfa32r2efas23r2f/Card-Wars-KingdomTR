using System;
using System.Collections.Generic;
using UnityEngine;

public class OfflinePurchaseListener : IPurchaseListener
{
	private const string OfflineIapTag = "[OFFLINE_IAP]";

	private static int localHandleCounter = 1000;

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}

	public void Fetch()
	{
	}

	public PurchaseManager.ProductDataRequestResult GetProductData(string[] productIds, PurchaseManager.ReceivedProductDataCallback callback)
	{
		if (callback != null)
		{
			List<PurchaseManager.ProductData> list = new List<PurchaseManager.ProductData>();
			if (productIds != null)
			{
				foreach (string text in productIds)
				{
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					PurchaseManager.ProductData productData = new PurchaseManager.ProductData();
					productData.ProductIdentifier = text;
					productData.Title = text;
					productData.Description = "Offline local product";
					productData.Price = "0.00";
					productData.FormattedPrice = "$0.00";
					productData.CurrencyCode = "USD";
					productData.CurrencySymbol = "$";
					productData.CountryCode = "US";
					list.Add(productData);
				}
			}
			callback(true, list, null);
		}
		return PurchaseManager.ProductDataRequestResult.Success;
	}

	public void GettingProductDataTimedOut()
	{
	}

	public void PurchaseProduct(string productId, PurchaseManager.ProductPurchaseCallback callback)
	{
		Debug.Log(OfflineIapTag + " Auto-approve purchase for product: " + productId);
		if (callback != null)
		{
			PurchaseManager.TransactionData transactionData = new PurchaseManager.TransactionData();
			transactionData.NativeTransaction = BuildFakeTransaction(productId);
			callback(PurchaseManager.ProductPurchaseResult.Success, transactionData, null);
		}
	}

	public void ConsumeProduct(string productId)
	{
	}

	public int VerifyReceiptGameServer(Session session, PurchaseManager.TransactionData transaction, string receipt, string productId, string transactionId, string partial, PurchaseManager.VerifyGMReceiptCallback callback)
	{
		int num = ++localHandleCounter;
		Debug.Log(OfflineIapTag + " Local receipt verified for product: " + productId + ", handle=" + num);
		if (callback != null)
		{
			callback(transaction, num, true);
		}
		return num;
	}

	public void ProcessOldPurchases()
	{
	}

	public void RestorePurchases(PurchaseManager.RestorePurchasesCallback callback = null)
	{
		if (callback != null)
		{
			callback(true);
		}
	}

	public void ExecSaveCurrentOldPurchases()
	{
	}

	public void RequestProcessOldPurchases()
	{
	}

	public void ExecProcessOldPurchases()
	{
	}

	private static Dictionary<string, object> BuildFakeTransaction(string productId)
	{
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		string text = "offline-order-" + num + "-" + Guid.NewGuid().ToString("N");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["packageName"] = Application.identifier;
		dictionary["orderId"] = text;
		dictionary["productId"] = productId;
		dictionary["developerPayload"] = "offline";
		dictionary["type"] = "inapp";
		dictionary["purchaseTime"] = num;
		dictionary["purchaseState"] = 0;
		dictionary["purchaseToken"] = "offline-token-" + Guid.NewGuid().ToString("N");
		dictionary["signature"] = "offline-signature";
		dictionary["originalJson"] = "{\"orderId\":\"" + text + "\",\"productId\":\"" + productId + "\",\"source\":\"offline\"}";
		return dictionary;
	}
}
