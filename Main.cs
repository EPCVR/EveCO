using System;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using BepInEx.Configuration;
using TMPro;
using Photon;

namespace EveCO
{
    [BepInPlugin("com.evora.eveco", "EveCO", "0.0.1")]
    public class Main : BaseUnityPlugin
    {
        bool isMenuCreated;
        GameObject ?menuObj;
        List<GameObject> btnObjs = new List<GameObject>();
        public float pageSwitchcooldown = 0f;

        public int curentCategoryIndex = -1;
        public int currentPageIndex = 0;

        List<List<List<string>>> allCategories = new List<List<List<string>>>();
        List<string> categoryNames = new List<string>{ "Movement", "Extras" };

        public static Main instance;

        public ConfigEntry<bool> speedBoostEnabled;
        public ConfigEntry<bool> flyEnabled;
        public ConfigEntry<bool> ghostMonkeEnabled;

        void Awake()
        {
            instance = this;


            List<List<string>> movementPages = new List<List<string>>() {

                new List<string> { "Speed Boost", "Fly" },
                new List<string> { "Ghost Monke" },
            };

        List<List<string>> extraPages = new List<List<string>>() {
                new List<string> { "Quit", "Disconnect" },
            };

            allCategories.Add(movementPages);
            allCategories.Add(extraPages);

            speedBoostEnabled = Config.Bind("Settings", "Speed Boost", false, "Gives a slight boost");
            flyEnabled = Config.Bind("Settings", "Fly", false, "Allows you to fly");
            ghostMonkeEnabled = Config.Bind("Settings", "Ghost Monke", false, "Makes you invisible");

            Harmony harmony = new Harmony("com.evora.eveco");
            harmony.PatchAll();

            Debug.Log("Menu Opened");
        }

        void Start()
        {
            isMenuCreated = false;
        }

        void Update()
        {
            if(pageSwitchcooldown > 0f)
            {
                pageSwitchcooldown -= Time.deltaTime;
            }

            if (!isMenuCreated && ControllerInputPoller.instance.leftControllerPrimaryButton)
            {
                CreateMenu();
            }
            else if (isMenuCreated && !ControllerInputPoller.instance.leftControllerPrimaryButton)
            {
                DestroyMenu();
            }

            if (speedBoostEnabled.Value) Mods.Speedboost();
            if (flyEnabled.Value) Mods.Fly();
            if (ghostMonkeEnabled.Value) Mods.GhostMonke();

        }

        public void CreateMenu()
        {
            isMenuCreated = true;

            var player = GorillaLocomotion.GTPlayer.Instance;

            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menuObj.transform.parent = player.LeftHand.controllerTransform;
            menuObj.transform.localPosition = Vector3.zero;
            menuObj.transform.localRotation = Quaternion.identity;
            menuObj.transform.localScale = new Vector3(0.03f, 0.3f, 0.45f);

            Destroy(menuObj.GetComponent<Rigidbody>());
            Destroy(menuObj.GetComponent<Collider>());

            var rend = menuObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.black;

            if(curentCategoryIndex == -1)
            {
                float zOffset = 0.15f;
                foreach(string categoryName in categoryNames)
                {
                    AddButton(zOffset, 0f, 0.2f, categoryName);
                    zOffset -= 0.5f;
                }
                return;
            }

            List<string> currentButtons = allCategories[curentCategoryIndex][currentPageIndex];

            float zOffset2 = 0.15f;

            foreach(string btnName in currentButtons)
            {
                AddButton(zOffset2, 0f, 0.2f, btnName);
                zOffset2 -= 0.05f;
            }

            AddButton(-0.1f, 0.135f, 0.1f,"Back");
            AddButton(-0.15f, 0.06f, 0.1f, "Previous");
            AddButton(-0.15f, -0.06f, 0.1f, "Next");
        }

        public void DestroyMenu()
        {
            isMenuCreated = false;

            GameObject.Destroy(menuObj);
                DestroyAllButtons();
        }

        void AddButton(float zOffset, float yOffset, float sOffset,string btnName)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            var player = GorillaLocomotion.GTPlayer.Instance;

            var follow = btnObj.AddComponent<FollowMenu>();
            follow.target = player.LeftHand.controllerTransform;
            follow.position = new Vector3(0.015f, yOffset, zOffset); 
            follow.rotation = Quaternion.identity;

            btnObj.transform.localScale = new Vector3(0.03f, sOffset, 0.04f);

            var rend = btnObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.grey;

            btnObj.GetComponent<Collider>().isTrigger = true;
            btnObj.layer = 18;

            var trigger = btnObj.AddComponent<ButtonTrigger>();
            trigger.btnIdentifier = btnName;

            var textObj = new GameObject("ButtonLabel");
            textObj.transform.SetParent(btnObj.transform);
            textObj.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            textObj.transform.localRotation = Quaternion.Euler(0f,  -90f, -90f);

            var text = textObj.AddComponent<TextMeshPro>();
            text.text = btnName;
            text.fontSize = 30;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.enableAutoSizing = true;
            text.rectTransform.sizeDelta = new Vector2(50f, 40f);
            text.transform.localScale = new Vector3(0.01f, 0.1f, 0.3f);

            btnObjs.Add(btnObj);
        }

        void DestroyAllButtons()
        {
            foreach (var btnObj in btnObjs)
            {
                GameObject.Destroy(btnObj);
            }
        }

        public void NextPage()
        {
            List<List<string>> currentCategory = allCategories[curentCategoryIndex];
            currentPageIndex = (currentPageIndex + 1) % currentCategory.Count;
            DestroyMenu();
            CreateMenu();
        }

        public void PreviousPage()
        {
            List<List<string>> currentCategory = allCategories[curentCategoryIndex];
            currentPageIndex = (currentPageIndex - 1 + currentCategory.Count) % currentCategory.Count;
            DestroyMenu();
            CreateMenu();
        }
    }

    public class  FollowMenu : MonoBehaviour
    {
        public Transform target;
        public Vector3 position;
        public Quaternion rotation;
        void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.TransformPoint(position);
                transform.rotation = target.rotation * rotation;
            }
        }
    }

    public class ButtonTrigger : GorillaPressableButton
    {
        
        public string btnIdentifier;
        bool isToggled;
        bool isTogglable;

        void Start()
        {
            switch (btnIdentifier)
            {
                case "Speed Boost":
                    isTogglable = true;
                    isToggled = Main.instance.speedBoostEnabled.Value;
                    break;

                case "Fly":
                    isTogglable = true;
                    isToggled = Main.instance.flyEnabled.Value;
                    break;

                case "Ghost Monke":
                    isTogglable = true;
                    isToggled = Main.instance.ghostMonkeEnabled.Value;
                    break;

                case "Next":
                    isTogglable = false;
                    break;

                case "Previous":
                    isTogglable = false;
                    break;

                case "Back":
                    isTogglable = false;
                    break;

                case "Movement":
                    isTogglable = false;
                    break;

                case "Extras":
                    isTogglable = false;
                    break;
            }

            if (isTogglable)
            {
                GetComponent<Renderer>().material.color = isToggled ? Color.white : Color.grey;
            }
            else
            {
                                GetComponent<Renderer>().material.color = Color.grey;
            }
        }
        



        public override void ButtonActivationWithHand(bool isLeftHand)
        {
            base.ButtonActivationWithHand(isLeftHand);

            if (!isLeftHand)
            {
                if (isTogglable)
                {
                    isToggled = !isToggled;
                    GetComponent<Renderer>().material.color = isToggled ? Color.white : Color.grey;
                }
                switch (btnIdentifier)
                {
                    case "Speed Boost":
                        Debug.Log("Speedboost");
                        Main.instance.speedBoostEnabled.Value = isToggled;
                        break;

                    case "Fly":
                        Debug.Log("Fly");
                        Main.instance.flyEnabled.Value = isToggled;
                        break;

                    case "Ghost Monke":
                        Debug.Log("GM");
                        Main.instance.ghostMonkeEnabled.Value = isToggled;
                        break;

                    case "Next":
                        if (Main.instance.pageSwitchcooldown <= 0f)
                        {
                            Main.instance.NextPage();
                            Main.instance.pageSwitchcooldown = 0.5f;
                        }
                        break;

                    case "Previous":
                        if (Main.instance.pageSwitchcooldown <= 0f)
                        {
                            Main.instance.PreviousPage();
                            Main.instance.pageSwitchcooldown = 0.5f;
                        }
                        break;

                    case "Movement":
                        Main.instance.curentCategoryIndex = 0;
                        Main.instance.currentPageIndex = 0;
                        Main.instance.DestroyMenu();
                        Main.instance.CreateMenu();
                        break;

                    case "Extras":
                        Main.instance.curentCategoryIndex = 1;
                        Main.instance.currentPageIndex = 0;
                        Main.instance.DestroyMenu();
                        Main.instance.CreateMenu();
                        break;

                    case "Back":
                        Main.instance.curentCategoryIndex = -1;
                        Main.instance.currentPageIndex = 0;
                        Main.instance.DestroyMenu();
                        Main.instance.CreateMenu();
                        break;

                    case "Quit":
                        Application.Quit();
                        break;

                    case "Disconnect":
                        Photon.Pun.PhotonNetwork.Disconnect();
                        break;
                }

                Main.instance.Config.Save();
            }
        }
    }
}
