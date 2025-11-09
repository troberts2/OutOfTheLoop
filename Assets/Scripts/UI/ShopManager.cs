using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [SerializeField] private Button yesButton; // to buy cosmetics

        [SerializeField] private Sprite[] productImages;
        [SerializeField] private GameObject[] lockPanels;

        StoreController m_StoreController;

        // Keeps runtime unlocked state in memory; mirror it to your JSON save system.
        // Key = productId, Value = unlocked (true/false)
        Dictionary<string, bool> unlockedProducts = new Dictionary<string, bool>(StringComparer.Ordinal);

        [SerializeField] private GameObject buyPanel;
        [SerializeField] private Image productImage;
        [SerializeField] private GameObject noAdsButton; // to be disabled afer no ads is bought
        [SerializeField] private CodelessIAPButton iapButton; // to change when a product is selected
        private Sprite currentSelectedSprite;
        private string currentSelectedProductId;

        #region Unity lifecycle
        protected void Start()
        {
            // First, load any locally saved purchases (so UI/logic is responsive before store connect).
            LoadLocalPurchases();

            // Start IAP initialization/connect to Play.
            InitializeIAP();
        }
        #endregion

        #region Initialization / Fetching products
        async void InitializeIAP()
        {
            Debug.Log("IAP: Initializing...");

            m_StoreController = UnityIAPServices.StoreController();
            m_StoreController.OnPurchasesFetched += PurchasesFetchedSuccess;
            m_StoreController.OnPurchasesFetchFailed += PurchasesFetchedFailed;


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

            RestorePurchases();

            await System.Threading.Tasks.Task.Delay(500); // small wait to allow product list populate in many environments
            
        }

        public void RestorePurchases()
        {
            // Build the ProductDefinition list limited to your non-consumables
            var productsToFetch = nonConsumableProductIds
                .Select(id => new ProductDefinition(id, ProductType.NonConsumable))
                .ToList();

            m_StoreController.FetchProducts(productsToFetch);

            m_StoreController.FetchPurchases();
        }

        private void PurchasesFetchedSuccess(Orders orders)
        {
            var purchases = m_StoreController.GetPurchases();
            UnlockOwnedProducts(purchases);
        }

        private void PurchasesFetchedFailed(PurchasesFetchFailureDescription description)
        {
            Debug.Log(description.Message);
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
                iapButton.productId = "3";
                iapButton.onOrderConfirmed.RemoveAllListeners();
                iapButton.onOrderConfirmed.AddListener(GetTopHat);
                iapButton.button = yesButton; // top hat
            }
            else if (currentSelectedProductId == nonConsumableProductIds[2]) //banana hat
            {
                currentSelectedSprite = productImages[1];
                iapButton.productId = "4";
                iapButton.onOrderConfirmed.RemoveAllListeners();
                iapButton.onOrderConfirmed.AddListener(GetBananaHat);
                iapButton.button = yesButton; // banana hat
            }
            else if (currentSelectedProductId == nonConsumableProductIds[3]) //fancy suit
            {
                currentSelectedSprite = productImages[2];
                iapButton.productId = "5";
                iapButton.onOrderConfirmed.RemoveAllListeners();
                iapButton.onOrderConfirmed.AddListener(GetSuitCostume);
                iapButton.button = yesButton; // suit
            }
            else if (currentSelectedProductId == nonConsumableProductIds[4]) //trail
            {
                currentSelectedSprite = productImages[3];
                iapButton.productId = "5";
                iapButton.onOrderConfirmed.RemoveAllListeners();
                iapButton.onOrderConfirmed.AddListener(GetBananaTrail);
                iapButton.button = yesButton; // banana trail
            }

            productImage.sprite = currentSelectedSprite;
        }

        public void CloseBuyPanel()
        {
            buyPanel.SetActive(false);
            currentSelectedProductId = null;
            currentSelectedSprite = null;

            if(SceneManager.GetActiveScene().name != "CosmeticsPicker") { return; }
            productImage.sprite = null;
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

            // Persist to local JSON save system:
            // Implement SavePurchaseLocally to actually write your save data (e.g., call your JSON save manager)
            SavePurchaseLocally();
        }

        public void GetAdFree(ConfirmedOrder order)
        {
            //unlock in memory
            GrantEntitlement(currentSelectedProductId);

            //actually disable ads
            AdManager.Instance.DisableAds();

            //close buy panel
            CloseBuyPanel();

            //Disable ad free button
            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                noAdsButton.SetActive(false);
            }
        }

        public void GetTopHat(ConfirmedOrder order)
        {
            GrantEntitlement(currentSelectedProductId);

            if(SceneManager.GetActiveScene().name == "CosmeticsPicker")
            {
                lockPanels[0].SetActive(false);
            }

            //close buy panel
            CloseBuyPanel();
        }

        public void GetBananaHat(ConfirmedOrder order)
        {
            GrantEntitlement(currentSelectedProductId);

            if (SceneManager.GetActiveScene().name == "CosmeticsPicker")
            {
                lockPanels[1].SetActive(false);
            }

            //close buy panel
            CloseBuyPanel();
        }

        public void GetSuitCostume(ConfirmedOrder order)
        {
            GrantEntitlement(currentSelectedProductId);

            if (SceneManager.GetActiveScene().name == "CosmeticsPicker")
            {
                lockPanels[2].SetActive(false);
            }

            //close buy panel
            CloseBuyPanel();
        }

        public void GetBananaTrail(ConfirmedOrder order)
        {
            GrantEntitlement(currentSelectedProductId);

            if (SceneManager.GetActiveScene().name == "CosmeticsPicker")
            {
                lockPanels[3].SetActive(false);
            }

            //close buy panel
            CloseBuyPanel();
        }

        #endregion

        #region Unlock owned products by checking receipts (store truth)
        /// <summary>
        /// Iterates fetched products and unlocks any non-consumables that have a receipt.
        /// This is the reliable method to reconcile purchases made previously on other devices.
        /// </summary>
        void UnlockOwnedProducts(ReadOnlyObservableCollection<Order> purchases)
        {
            if (m_StoreController == null)
            {
                Debug.Log("UnlockOwnedProducts: store controller is null.");
                return;
            }

            Debug.Log("unlocked products from store");
            foreach (Order order in purchases)
            {
                string orderProductID = order.CartOrdered.Items()[0].Product.definition.id;
                unlockedProducts[orderProductID] = true;
                Debug.Log("Unlocked Product from receipt: " + orderProductID);
            }

            SavePurchaseLocally();

            ApplyLocalUnlocks();
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
        void SavePurchaseLocally()
        {
            Debug.Log($"[SavePurchaseLocally] {currentSelectedProductId}");

            SaveSystem.Instance.SaveLocalUnlocks(unlockedProducts);
        }

        /// <summary>
        /// Load local save file into `unlockedProducts` dictionary.
        /// Called at Awake to rehydrate local unlocked state ASAP.
        /// </summary>
        void LoadLocalPurchases()
        {
            Debug.Log("[LoadLocalPurchases] Loading local purchases (if any).");

            unlockedProducts = SaveSystem.Instance.LoadUnlocks();

            ApplyLocalUnlocks();
        }

        /// <summary>
        /// Apply the local saved unlock status to the game immediately.
        /// This keeps UI consistent while we contact the store.
        /// </summary>
        void ApplyLocalUnlocks()
        {
            bool isTester = SaveSystem.Instance.LoadGame().testerFlag.isTester;
            foreach (var kv in unlockedProducts)
            {

                if (kv.Value || isTester)
                {
                    ApplyUnlockToGame(kv.Key);
                }
            }
        }
        // -------------------------------------------------------

        private void ApplyUnlockToGame(string productID)
        {
            //apply ad free always if user has
            if (productID == nonConsumableProductIds[0])
            {// ad free
                GetAdFree(null);
            }

            if(SceneManager.GetActiveScene().name != "CosmeticsPicker")
            {
                return;
            }

            if (productID == nonConsumableProductIds[1])
            {// top hat
                GetTopHat(null);
            }

            if (productID == nonConsumableProductIds[2])
            {// banana hat
                GetBananaHat(null);
            }

            if (productID == nonConsumableProductIds[3])
            {// Suit
                GetSuitCostume(null);
            }

            if (productID == nonConsumableProductIds[4])
            {// banana trail
                GetBananaTrail(null);
            }
        }
        #endregion
    }
}
