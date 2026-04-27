using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PurchaseManager : Singleton<PurchaseManager>
{
	public enum ProductDataRequestResult
	{
		Success,
		CannotMakePayment
	}

	public enum ProductPurchaseResult
	{
		Success,
		Failed,
		VerificationFailed,
		Cancelled
	}

	public class ProductData
	{
		public string ProductIdentifier;

		public string Title;

		public string Description;

		public string Price;

		public string CurrencySymbol;

		public string CurrencyCode;

		public string FormattedPrice;

		public string CountryCode;

		public override string ToString()
		{
			return string.Format("<ProductData>\nID: {0}\nTitle: {1}\nDescription: {2}\nPrice: {3}\nCurrency Symbol: {4}\nFormatted Price: {5}\nCurrency Code: {6}\nCountry Code: {7}", ProductIdentifier, Title, Description, Price, CurrencySymbol, FormattedPrice, CurrencyCode, CountryCode);
		}
	}

	public class TransactionData
	{
		public object NativeTransaction;

		public override string ToString()
		{
			string text = PurchaseManager.GetTransactionField(NativeTransaction, "productId");
			string text2 = PurchaseManager.GetTransactionField(NativeTransaction, "type");
			string text3 = PurchaseManager.GetTransactionField(NativeTransaction, "orderId");
			return string.Format("<Transaction> ID: {0}, type: {1}, transactionIdentifier: {2}", text, text2, text3);
		}
	}

	public delegate void ReceivedProductDataCallback(bool success, List<ProductData> list, string err);

	public delegate void FinalProductPurchaseCallback(ProductPurchaseResult result);

	public delegate void ProductPurchaseCallback(ProductPurchaseResult result, TransactionData transaction, string err);

	public delegate void RestorePurchasesCallback(bool success);

	public delegate void VerifyGMReceiptCallback(TransactionData store, int handle, bool success);

	private const float productDataTimeoutLimit = 5f;

	private bool m_Purchasing;

	public List<ProductData> m_Products = new List<ProductData>();

	private bool AmazonDevice;

	private IPurchaseListener m_Listener;

	private FinalProductPurchaseCallback m_PurchaseCallback;

	private static int m_VerifyPurchaseHandle = -1;

	private ProductData m_RequestedProduct;

	private bool m_ReceiptVerificationEnd;

	private TransactionData m_storeKit;

	private bool m_success;

	private string m_oldReceipt;

	private static string m_OldPurchaseProduct = string.Empty;

	private static bool m_WaitForOldPurchase;

	private static bool m_WaitForOldConsume;

	public bool InPurchaseProcess
	{
		get
		{
			return m_Purchasing;
		}
	}

	public bool IsAmazon
	{
		get
		{
			return AmazonDevice;
		}
		private set
		{
		}
	}

	public int getLastHandle
	{
		get
		{
			int verifyPurchaseHandle = m_VerifyPurchaseHandle;
			m_VerifyPurchaseHandle = -1;
			return verifyPurchaseHandle;
		}
	}

	public static event Action<string> RedeemOldProductEvent;

	public void GetPriceInfo(string ProductID, out float Price, out string CurrencyType)
	{
		foreach (ProductData product in m_Products)
		{
			if (!(product.ProductIdentifier == ProductID))
			{
				continue;
			}
			string text = string.Empty;
			bool flag = false;
			for (int i = 0; i < product.Price.Length; i++)
			{
				if (char.IsDigit(product.Price[i]))
				{
					text += product.Price[i];
				}
				if (char.IsPunctuation(product.Price[i]))
				{
					flag = true;
				}
			}
			Price = float.Parse(text, CultureInfo.InvariantCulture.NumberFormat);
			if (flag)
			{
				Price /= 100f;
			}
			CurrencyType = product.CurrencyCode;
			if (CurrencyType == null)
			{
				CurrencyType = "USD";
			}
			return;
		}
		Price = 0f;
		CurrencyType = "USD";
	}

	public void GetformattedPrice(string ProductID, out string Price)
	{
		foreach (ProductData product in m_Products)
		{
			if (product.ProductIdentifier == ProductID)
			{
				Price = product.FormattedPrice;
				return;
			}
		}
		Price = "0";
	}

	public void GetProductItem(string ProductID, out ProductData product)
	{
		foreach (ProductData product2 in m_Products)
		{
			if (product2.ProductIdentifier == ProductID)
			{
				product = product2;
				return;
			}
		}
		product = null;
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
		DetectAmazonDevice();
		m_Listener = new OfflinePurchaseListener();
	}

	private void OnEnable()
	{
		if (m_Listener != null)
		{
			m_Listener.OnEnable();
		}
	}

	private void OnDisable()
	{
		if (m_Listener != null)
		{
			m_Listener.OnDisable();
		}
	}

	private void OnDestroy()
	{
		if (m_Listener != null)
		{
			m_Listener.OnDisable();
		}
	}

	private void Update()
	{
		if (m_Listener != null)
		{
			m_Listener.Fetch();
		}
		if (m_ReceiptVerificationEnd)
		{
			m_ReceiptVerificationEnd = false;
			VerifyCallbackGM();
		}
		if (m_WaitForOldPurchase)
		{
			m_WaitForOldPurchase = false;
			RedeemOldPurchase(m_OldPurchaseProduct);
			Singleton<PurchaseManager>.Instance.ConsumeProduct(m_OldPurchaseProduct);
		}
		if (m_WaitForOldConsume)
		{
			m_WaitForOldConsume = false;
			Debug.Log("[IAP_DISABLED] Skip old purchase consume for product: " + m_OldPurchaseProduct);
		}
	}

	private void DetectAmazonDevice()
	{
		AmazonDevice = false;
	}

	public ProductDataRequestResult GetProductData(string[] a_StringIDs, ReceivedProductDataCallback a_Callback)
	{
		if (a_StringIDs.Length > 0)
		{
		}
		return m_Listener.GetProductData(a_StringIDs, a_Callback);
	}

	private IEnumerator StartGetProductDataTimer()
	{
		yield return new WaitForSeconds(5f);
		m_Listener.GettingProductDataTimedOut();
	}

	public void PurchaseProduct(string a_productID, FinalProductPurchaseCallback a_Callback)
	{
		m_Purchasing = true;
		GetProductItem(a_productID, out m_RequestedProduct);
		if (m_RequestedProduct != null)
		{
		}
		if (m_Listener != null)
		{
			m_PurchaseCallback = a_Callback;
			m_Listener.PurchaseProduct(a_productID, InternalPurchaseCallback);
		}
	}

	private void InternalPurchaseCallback(ProductPurchaseResult result, TransactionData transaction, string err)
	{
		if (!m_Purchasing)
		{
			return;
		}
		m_Purchasing = false;
		switch (result)
		{
		case ProductPurchaseResult.Success:
		{
			string transactionField = GetTransactionField(transaction.NativeTransaction, "originalJson");
			string transactionField2 = GetTransactionField(transaction.NativeTransaction, "productId");
			string transactionField3 = GetTransactionField(transaction.NativeTransaction, "signature");
			if (string.IsNullOrEmpty(transactionField2))
			{
				transactionField2 = ((m_RequestedProduct != null) ? m_RequestedProduct.ProductIdentifier : string.Empty);
			}
			if (string.IsNullOrEmpty(transactionField))
			{
				transactionField = "offline-receipt-" + transactionField2;
			}
			if (transactionField == m_oldReceipt)
			{
				m_PurchaseCallback(ProductPurchaseResult.Cancelled);
				break;
			}
			m_oldReceipt = transactionField;
			Session theSession = SessionManager.Instance.theSession;
			m_ReceiptVerificationEnd = false;
			m_VerifyPurchaseHandle = Singleton<PurchaseManager>.Instance.VerifyReceiptGameServer(theSession, transaction, transactionField, transactionField2, transactionField3, string.Empty, VerifyCallbackSet);
			break;
		}
		case ProductPurchaseResult.Failed:
			m_PurchaseCallback(result);
			break;
		case ProductPurchaseResult.Cancelled:
			m_PurchaseCallback(result);
			break;
		case ProductPurchaseResult.VerificationFailed:
			break;
		}
	}

	private void VerifyCallbackSet(TransactionData storekit, int handle, bool success)
	{
		m_storeKit = storekit;
		m_success = success;
		m_ReceiptVerificationEnd = true;
		m_VerifyPurchaseHandle = handle;
	}

	private void VerifyCallbackGM()
	{
		if (!m_success)
		{
			m_PurchaseCallback(ProductPurchaseResult.VerificationFailed);
			return;
		}
		string transactionField = GetTransactionField(m_storeKit.NativeTransaction, "productId");
		if (string.IsNullOrEmpty(transactionField))
		{
			transactionField = ((m_RequestedProduct != null) ? m_RequestedProduct.ProductIdentifier : string.Empty);
		}
		float Price;
		string CurrencyType;
		GetPriceInfo(transactionField, out Price, out CurrencyType);
		Singleton<PurchaseManager>.Instance.ConsumeProduct(transactionField);
		m_PurchaseCallback(ProductPurchaseResult.Success);
		m_storeKit = null;
		m_success = false;
	}

	public void ProcessOldPurchases()
	{
		List<CurrencyPackageData> database = CurrencyPackageDataManager.Instance.GetDatabase();
		List<string> list = new List<string>();
		foreach (CurrencyPackageData item in database)
		{
			list.Add(item.ID);
		}
		string[] a_StringIDs = list.ToArray();
		GetProductData(a_StringIDs, null);
	}

	public void ConsumeProduct(string a_productID)
	{
		if (m_Listener != null)
		{
			m_Listener.ConsumeProduct(a_productID);
		}
	}

	public void RestorePurchases(RestorePurchasesCallback callback = null)
	{
		Debug.Log("calling KFF.RestorePurchases");
		StartCoroutine(CoroutineRestorePurchase(callback));
	}

	private IEnumerator CoroutineRestorePurchase(RestorePurchasesCallback callback)
	{
		yield return null;
		if (m_Listener != null)
		{
			m_Listener.RestorePurchases(callback);
		}
	}

	public int VerifyReceiptGameServer(Session session, TransactionData transaction, string receipt, string productid, string transactionid, string partial, VerifyGMReceiptCallback callback)
	{
		if (m_Listener != null)
		{
			return m_Listener.VerifyReceiptGameServer(session, transaction, receipt, productid, transactionid, partial, callback);
		}
		return -1;
	}

	public void ExecuteSaveCurrentOldPurchases()
	{
	}

	public static bool ValidateRedeemOldPurchaseEvent()
	{
		return PurchaseManager.RedeemOldProductEvent != null;
	}

	public static void RedeemOldPurchase(string a_ProductID)
	{
		if (ValidateRedeemOldPurchaseEvent())
		{
			PurchaseManager.RedeemOldProductEvent(a_ProductID);
		}
	}

	public static void StartRedeemOldPurchase(string a_ProductID, int handle)
	{
		m_WaitForOldPurchase = true;
		m_OldPurchaseProduct = a_ProductID;
		m_VerifyPurchaseHandle = handle;
	}

	public static void StartConsumeOldPurchase(string a_ProductID)
	{
		m_WaitForOldConsume = true;
		m_OldPurchaseProduct = a_ProductID;
	}

	public ProductDataRequestResult GetProductData(ReceivedProductDataCallback a_Callback)
	{
		List<CurrencyPackageData> database = CurrencyPackageDataManager.Instance.GetDatabase();
		List<string> list = new List<string>();
		foreach (CurrencyPackageData item in database)
		{
			list.Add(item.ID);
		}
		string[] a_StringIDs = list.ToArray();
		return GetProductData(a_StringIDs, a_Callback);
	}

	private static string GetTransactionField(object nativeTransaction, string key)
	{
		if (nativeTransaction == null || string.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		Dictionary<string, object> dictionary = nativeTransaction as Dictionary<string, object>;
		object value = null;
		if (dictionary != null && dictionary.TryGetValue(key, out value) && value != null)
		{
			return value.ToString();
		}
		return string.Empty;
	}
}
