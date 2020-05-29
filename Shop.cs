using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

public class Shop : MonoBehaviour
{
    public GameObject weaponsPortal;

    private bool shopIsOpen = false;

    private Text price;
    public short[] pricesOfWeapons;

    public Animation animeShopButton;

    private List<int> lockedItems = new List<int>();

    private void Start() {
        // check on available items in shop
        SaveSystem.sv.availableWeaponsInShop.Sort ();

        for (int i = 0; i < 14; i++) {
            Transform item = gameObject.transform.GetChild (5).GetChild (i);
            int itemID = System.Convert.ToInt32 (item.gameObject.name.Remove (0, 4));

            if (SaveSystem.sv.availableWeaponsInShop.BinarySearch (itemID) < 0) {
                item.GetComponent <Image> ().color = new Color (0.2039216f, 0.345098f, 0.4470588f, 0.1882353f / 2);
                item.GetChild (0).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f / 2);
                item.GetChild (1).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f / 2);
                item.GetChild (4).gameObject.SetActive (true);
                item.GetChild (4).GetComponent <Text> ().text = LangSystem.lng.locked;

                lockedItems.Add (itemID);
            }
        }

        SaveSystem.sv.availableWeaponsInShop.Sort ();
        lockedItems.Sort ();
    }

    public void ShopOpenAndClose () {
        if (shopIsOpen) {
            animeShopButton.Play ("ShopClose");
        }
        else {
            animeShopButton.Play ("ShopOpen");
            StartCoroutine (UpdatingItemsStates ());
        }

        shopIsOpen = !shopIsOpen;
    }

    public void CashingPrice (Text price) =>
        this.price = price;

	public void BuyWeapon (int itemID) {
        short price = System.Convert.ToInt16 (this.price.text.TrimStart ('£'));
        // sound
        AudioSystem.inst.Click(GameManager.inst.myPlayerData.money >= price && lockedItems.BinarySearch (itemID) < 0);
        // exit if we dont have money, or this item is locked for us
        if (GameManager.inst.myPlayerData.money < price || lockedItems.BinarySearch (itemID) > -1)
            return;

        GameManager.inst.myPlayerData.money -= price;

        if (GameManager.inst.myCharacterControl.isDead)
        {
            StartCoroutine (WaitUntilSpawn (itemID));
            ShopOpenAndClose ();
            return;
        }
        // spawn buyed item
		GameObject newPortal = PhotonNetwork.Instantiate (weaponsPortal.name
        , GameManager.inst.myPlayerData.gameObject.transform.position + new Vector3 (0.45f * ((GameManager.inst.myCharacterControl.facingRight) ? 1 : -1), 0.1f, 0)
        , Quaternion.identity);
        WeaponsPortal script = newPortal.GetComponent <WeaponsPortal> ();
		script.pView.RPC ("SpawnOfSpawner", RpcTarget.AllBuffered, false);
        script.OrderOfSpawn ((byte) itemID);

        ShopOpenAndClose ();
    }

    IEnumerator UpdatingItemsStates () {
        while (shopIsOpen)
        {
            // check on available items in shop
            for (int i = 0; i < 14; i++) {
                // item in shop
                Transform item = gameObject.transform.GetChild (5).GetChild (i);

                if (lockedItems.BinarySearch (System.Convert.ToInt32 (item.gameObject.name.Remove (0, 4))) > -1) // if locked
                    continue;

                // price of i item in shop
                short price = System.Convert.ToInt16 (item.GetChild (3).GetComponent <Text> ().text.TrimStart ('£'));

                if (GameManager.inst.myPlayerData.money < price) { // if we don't have money for buy
                    item.GetComponent <Image> ().color = new Color (0.2039216f, 0.345098f, 0.4470588f, 0.1882353f / 2);
                    item.GetChild (0).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f / 2);
                    item.GetChild (1).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f / 2);
                } else {
                    item.GetComponent <Image> ().color = new Color (0.2039216f, 0.345098f, 0.4470588f, 0.1882353f);
                    item.GetChild (0).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f);
                    item.GetChild (1).GetComponent <Image> ().color = new Color (1f, 1f, 1f, 0.8941177f);
                }
            }

            yield return new WaitForSeconds (0.1f);
        }
    }

    IEnumerator WaitUntilSpawn (int itemID) {
        // wait
        while (GameManager.inst.myCharacterControl.isDead) {
            yield return new WaitForSeconds (0.4f);
        }
        // spawn buyed item
		GameObject newPortal = PhotonNetwork.Instantiate (weaponsPortal.name
        , GameManager.inst.myPlayerData.gameObject.transform.position + new Vector3 (0.45f * ((GameManager.inst.myCharacterControl.facingRight) ? 1 : -1), 0.1f, 0)
        , Quaternion.identity);
        WeaponsPortal script = newPortal.GetComponent <WeaponsPortal> ();
		script.pView.RPC ("SpawnOfSpawner", RpcTarget.AllBuffered, false);
        script.OrderOfSpawn ((byte) itemID);
    }
}
