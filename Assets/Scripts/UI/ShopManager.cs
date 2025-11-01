using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Samples.Purchasing.IAP5.Minimal
{
    /// <summary>
    /// Paywall manager specialized for NON-CONSUMABLE products.
    /// - Call BuyProductById from UI Buttons (add productId as parameter in Inspector).
    /// - Call RestorePurchases from UI Buttons to restore previous purchases.
    /// - On init it will fetch products and unlock any already-owned non-consumables.
    /// 
    /// Where to add your JSON save:
    /// - SavePurchaseLocally(productId) -> call inside GrantEntitlement after unlocking.
    /// - LoadLocalPurchases() -> call on Awake/Start to rehydrate local save state.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        // Replace with your own list of product IDs for non-consumables.
        [Header("Product IDs (Non-Consumables)")]
        [Tooltip("List product IDs exactly as defined on Play Console")]
        public List<string> nonConsumableProductIds = new List<string>
        {
            "com.trobertsDev.OutOfTheLoop.1", // no ads (one time)
            "com.trobertsDev.OutOfTheLoop.3", // hat (tophat)
            "com.trobertsDev.OutOfTheLoop.4", // hat(banana)
            "com.trobertsDev.OutOfTheLoop.5", // costume (fancy suit)
            "com.trobertsDev.OutOfTheLoop.6" // trail (banana)
        };

        [SerializeField] private Sprite[] productImages;
        [SerializeField] private GameObject[] lockPanels;

        StoreController m_StoreController;

        // Keeps runtime unlocked state in memory; mirror it to your JSON save system.
        // Key = productId, Value = unlocked (true/false)
        Dictionary<string, bool> unlockedProducts = new Dictionary<string, bool>(StringComparer.Ordinal);

        [SerializeField] private GameObject buyPanel;
        [SerializeField] private Image productImage;
        private Sprite currentSelectedSprite;
        private string currentSelectedProductId;

        #region Unity lifecycle
        protected void Awake()
        {
            // First, load any locally saved purchases (so UI/logic is responsive before store connect).
            LoadLocalPurchases();

            // Initialize unlocking of any known local purchases in-memory (UI might rely on this).
            ApplyLocalUnlocks();

            // Start IAP initialization/connect to Play.
            InitializeIAP();
        }
        #endregion

        #region Initialization / Fetching products
        async void InitializeIAP()
        {
            Debug.Log("IAP: Initializing...");

            m_StoreController = UnityIAPServices.StoreController();

            // Wire events from the sample controller
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;

            try
            {
                await m_StoreController.Connect();
                Debug.Log("IAP: Connected to store.");
            }
            catch (Exception ex)
            {
                Debug.Log("IAP: Connect failed: " + ex.Message);
                // still attempt to fetch products — controller might be partially usable depending on implementation
            }

            // Build the ProductDefinition list limited to your non-consumables
            var productsToFetch = nonConsumableProductIds
                .Select(id => new ProductDefinition(id, ProductType.NonConsumable))
                .ToList();

            m_StoreController.FetchProducts(productsToFetch);

            // After fetching, attempt to unlock any owned (non-consumable) products by checking receipts.
            // Note: FetchProducts is usually asynchronous — depending on the StoreController impl you might
            // want to wait for a callback; here we give a small delay path-free approach by calling UnlockOwnedProducts()
            // The StoreController's GetProducts() should return up-to-date products after FetchProducts completes.
            // If your StoreController provides a callback for "fetch complete", consider calling UnlockOwnedProducts there.
            await System.Threading.Tasks.Task.Delay(500); // small wait to allow product list populate in many environments
            UnlockOwnedProducts();
        }
        #endregion

        #region Purchase flow (UI callable)

        public void OpenBuyPanel(string productId)
        {
            buyPanel.SetActive(true);
            currentSelectedProductId = productId;

            if (SceneManager.GetActiveScene().name != "CosmeticsPicker") return; // return if not in cosmetics scene so not to change product image

            if(currentSelectedProductId == nonConsumableProductIds[1]) //top hat
            {
                currentSelectedSprite = productImages[0];
            }
            else if (currentSelectedProductId == nonConsumableProductIds[2]) //banana hat
            {
                currentSelectedSprite = productImages[1];
            }
            else if (currentSelectedProductId == nonConsumableProductIds[3]) //fancy suit
            {
                currentSelectedSprite = productImages[2];
            }
            else if (currentSelectedProductId == nonConsumableProductIds[4]) //trail
            {
                currentSelectedSprite = productImages[3];
            }

            productImage.sprite = currentSelectedSprite;
        }

        public void CloseBuyPanel()
        {
            buyPanel.SetActive(false);
            currentSelectedProductId = null;
            currentSelectedSprite = null;

            productImage.sprite = null;
        }

        /// <summary>
        /// UI button can call this and pass the productId string as parameter.
        /// Example: Button -> OnClick() -> PaywallManager.BuyProductById -> "com.yourcompany.yourgame.no_ads"
        /// </summary>
        public void BuyProductById()
        {
            if (string.IsNullOrEmpty(currentSelectedProductId))
            {
                Debug.Log("BuyProductById: productId is null/empty.");
                return;
            }

            // Ensure it's one of our non-consumables (optional guard)
            if (!nonConsumableProductIds.Contains(currentSelectedProductId))
            {
                Debug.Log($"BuyProductById: {currentSelectedProductId} is not in nonConsumableProductIds list.");
                // still allow purchase if you want; returning here enforces only allowed list
                // return;
            }

            InitiatePurchase(currentSelectedProductId);
        }

        // Triggers the underlying store controller purchase
        void InitiatePurchase(string productId)
        {
            var product = m_StoreController?.GetProducts().FirstOrDefault(p => p.definition.id == productId);

            if (product != null)
            {
                Debug.Log($"Initiating purchase for {productId}");
                m_StoreController?.PurchaseProduct(product);
            }
            else
            {
                Debug.Log($"The product service has no product with the ID {productId} (not fetched or wrong id).");
            }

            CloseBuyPanel();
        }

        // Expose restore to UI
        public void RestorePurchases()
        {
            Debug.Log("RestorePurchases called.");
            m_StoreController.RestoreTransactions(OnTransactionsRestored);
        }

        void OnTransactionsRestored(bool success, string error)
        {
            Debug.Log("Transactions restored: " + success + (string.IsNullOrEmpty(error) ? "" : $", error: {error}"));
            if (success)
            {
                // After restore, ensure store-owned products are unlocked
                UnlockOwnedProducts();
            }
        }
        #endregion

        #region Purchase event handlers (from sample)
        void OnPurchasePending(PendingOrder order)
        {
            // If you want to validate pending orders before confirming, do it here.
            // For non-consumables you may want to confirm after entitlement is granted.
            Debug.Log($"OnPurchasePending: {order.CartOrdered.Items().First().Product.definition.id}");
            // For this sample we confirm immediately and then grant entitlement in OnPurchaseConfirmed
            m_StoreController.ConfirmPurchase(order);
        }

        void OnPurchaseConfirmed(Order order)
        {
            // This is called when store confirms purchase (or fails)
            switch (order)
            {
                case FailedOrder failedOrder:
                    {
                        var pid = failedOrder.CartOrdered.Items().First().Product.definition.id;
                        Debug.Log($"Purchase confirmation failed: {pid}, {failedOrder.FailureReason}, {failedOrder.Details}");
                        break;
                    }
                case ConfirmedOrder confirmed:
                    {
                        var pid = confirmed.CartOrdered.Items().First().Product.definition.id;
                        Debug.Log($"Purchase completed: {pid}");

                        // Grant the entitlement (unlock the item) and save locally
                        GrantEntitlement(pid);

                        break;
                    }
                default:
                    {
                        // Other order types (if any)
                        Debug.Log("OnPurchaseConfirmed: Unknown order type.");
                        break;
                    }
            }
        }
        #endregion

        #region Entitlement / Unlock logic
        /// <summary>
        /// Single place to apply a purchased product to the user's game state.
        /// - Unlock visuals/features.
        /// - Call SavePurchaseLocally(productId) to persist in your JSON save system.
        /// - Optionally call server validation here (recommended for production).
        /// </summary>
        void GrantEntitlement(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return;

            Debug.Log($"GrantEntitlement called for {productId}");

            // Set in-memory unlocked flag
            unlockedProducts[productId] = true;

            ApplyUnlockToGame(productId);

            // Persist to local JSON save system:
            // Implement SavePurchaseLocally to actually write your save data (e.g., call your JSON save manager)
            SavePurchaseLocally(productId);

            // Optional: send purchase token/receipt to your backend for server-side validation and granting
            // e.g., StartCoroutine(SendReceiptToServer(...));
        }

        // Apply the unlocked state to your game (UI, local runtime data)
        void ApplyUnlockToGame(string productId)
        {
            if (SceneManager.GetActiveScene().name == "CosmeticsPicker") return;
            // TODO ACTUALLY do logic for these
            Debug.Log($"Applying unlock for product: {productId}");
            if (productId == "com.trobertsDev.OutOfTheLoop.1") ;//disable ads
            else if (productId == "com.trobertsDev.OutOfTheLoop.3") //hat (top hat)
                lockPanels[0].SetActive(false);
            else if (productId == "com.trobertsDev.OutOfTheLoop.4") //hat (banana)
                lockPanels[1].SetActive(false);
            else if (productId == "com.trobertsDev.OutOfTheLoop.5") //costume (suit)
                lockPanels[2].SetActive(false);
            else if (productId == "com.trobertsDev.OutOfTheLoop.6") //trail (banana)
                lockPanels[3].SetActive(false);
        }

        #endregion

        #region Unlock owned products by checking receipts (store truth)
        /// <summary>
        /// Iterates fetched products and unlocks any non-consumables that have a receipt.
        /// This is the reliable method to reconcile purchases made previously on other devices.
        /// </summary>
        void UnlockOwnedProducts()
        {
            if (m_StoreController == null)
            {
                Debug.Log("UnlockOwnedProducts: store controller is null.");
                return;
            }

            var products = m_StoreController.GetProducts();
            if (products == null)
            {
                Debug.Log("UnlockOwnedProducts: no products returned by controller.");
                return;
            }

            foreach (var p in products)
            {
                try
                {
                    // Only care about non-consumables in this manager
                    if (p.definition.type != ProductType.NonConsumable)
                        continue;

                    // If product has a receipt, Play reports it as owned — unlock it.
                    // Note: .hasReceipt is true when store records an ownership that can be restored.
                    bool owned = false;
                    // safe access: some product implementations expose .hasReceipt (UnityEngine.Purchasing.Product)
                    var unityProduct = p as UnityEngine.Purchasing.Product;
                    if (unityProduct != null)
                    {
                        owned = !string.IsNullOrEmpty(unityProduct.receipt);
                    }
                    else
                    {
                        // Fallback: some store SDK wrappers provide a 'hasReceipt' or 'receipt' property on the product
                        // If not available, you may need to use your StoreController's purchase/restore API directly.
                        // For now, we try to inspect .hasReceipt via reflection as a last resort:
                        var hasReceiptProp = p.GetType().GetProperty("hasReceipt");
                        if (hasReceiptProp != null)
                        {
                            var val = hasReceiptProp.GetValue(p);
                            if (val is bool b) owned = b;
                        }
                    }

                    if (owned)
                    {
                        Debug.Log($"UnlockOwnedProducts: owned -> {p.definition.id}");
                        GrantEntitlement(p.definition.id);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"UnlockOwnedProducts: exception for product {p.definition.id}: {ex.Message}");
                }
            }
        }
        #endregion

        #region Local JSON save integration (stubs)
        // -------------------------------------------------------
        // Replace the internals of these stubs with calls to your JSON save system.
        // -------------------------------------------------------

        /// <summary>
        /// Persist that the productId is owned in your JSON save file.
        /// Called from GrantEntitlement after unlocking.
        /// </summary>
        void SavePurchaseLocally(string productId)
        {
            Debug.Log($"[SavePurchaseLocally] {productId}");

            SaveSystem.Instance.SaveLocalUnlocks(unlockedProducts);
        }

        /// <summary>
        /// Load local save file into `unlockedProducts` dictionary.
        /// Called at Awake to rehydrate local unlocked state ASAP.
        /// </summary>
        void LoadLocalPurchases()
        {
            Debug.Log("[LoadLocalPurchases] Loading local purchases (if any).");

            SaveFile save = SaveSystem.Instance.LoadGame();
            unlockedProducts = save.localUnlocks.savedUnlockedProducts;

            // For demonstration, if nothing saved we ensure dictionary contains our product ids set to false.
            foreach (var id in nonConsumableProductIds)
            {
                if (!unlockedProducts.ContainsKey(id))
                    unlockedProducts[id] = false;
            }
        }

        /// <summary>
        /// Apply the local saved unlock status to the game immediately.
        /// This keeps UI consistent while we contact the store.
        /// </summary>
        void ApplyLocalUnlocks()
        {
            foreach (var kv in unlockedProducts)
            {
                if (kv.Value)
                {
                    ApplyUnlockToGame(kv.Key);
                }
            }
        }
        // -------------------------------------------------------
        #endregion
    }
}
