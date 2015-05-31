﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using System.Reflection;
using UnityEngine;

namespace KSI
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class KSICore : MonoBehaviour
    {
        // Initiate Variables
        internal static string ImageFolder = "KSI/Images/";
        private static ApplicationLauncherButton KSIButton = null;
        internal bool KSIGuiOn = false;
        internal bool KSITooltip = false;

        void Awake()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                DontDestroyOnLoad(this);
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
            }
        }

        void OnGUIAppLauncherReady()
        {

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && KSIButton == null)
            {

                string IconFile = "OTG";
                KSIButton = ApplicationLauncher.Instance.AddModApplication(
                    BTOn,
                    BTOff,
                    BTHoverOn,
                    BTHoverOff,
                    null, null,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture)GameDatabase.Instance.GetTexture(ImageFolder + IconFile, false));
            }
            KSIGuiOn = false;
        }


        void OnGUIAppLauncherDestroyed()
        {
            if (KSIButton != null)
            {
                BTOff();
                ApplicationLauncher.Instance.RemoveModApplication(KSIButton);
                KSIButton = null;
            }
        }

        void BTOn()
        {
            if(KSIButton == null)
            {
                Debug.LogError("KSI :: BTOn called without a button.");
                return;
            }
            KSIGuiOn = true;
        }

        void BTOff()
        {
            if (KSIButton == null)
            {
                Debug.LogError("KSI :: BTOff called without a button.");
                return;
            }
            KSIGuiOn = false;
        }

        void BTHoverOn()
        {
            if (KSITooltip == false)
            {
                KSITooltip = true;
            }
        }

        void BTHoverOff()
        {
            if (KSITooltip == true)
            {
                KSITooltip = false;
            }
       }

        void OnGUI()
        {
            if ((KSIGuiOn) && (HighLogic.LoadedScene == GameScenes.SPACECENTER))
            {
                KSIGUI();
            }
        }

        void KSIGUI()
        {

            GUI.BeginGroup(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 250, 500, 500));
            GUILayout.BeginVertical("box");
            GUILayout.Label("This is a place holder for now.");
            GUILayout.Label("Eventually this will hold options to modify the way");
            GUILayout.Label("the new Astronaut Complex hire overlay works.");
            GUILayout.Label("This is a button for the Kerbal Science Institute: Placement Services.");
            GUILayout.Label("The reason there is an acorn as the logo is it is created by OakTree42.");
            GUILayout.Label("If you do not close this window it may return when you reload the KSC screen.");
            if (GUILayout.Button("Close this Window", GUILayout.Width(200f)))
                BTOff();
            GUILayout.EndVertical();
            GUI.EndGroup();

        }

        void GTest1()
        {
            GUI.Box(new Rect(0, 0, 150, 10), "Kerbal Science Institute");
        }
    }
}
