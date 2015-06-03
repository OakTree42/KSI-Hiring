using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;
using System.Reflection;

// Cost math currently works, but need a way to determine viable levels
// Cost variations based on dead kerbal career types
// Level viable based on level of AC
// Cost versus actual funds
// Change font of 'cost' area to something more... noticable.
// See about tool tips for career types to make it easier to put more data.
// HAve text color change if costs more than the player has. 
// Have a hire button - This is going to be tricky as we will want to generate a kerbal 
// potentially give it XP.

namespace KSI
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexSpaceCentre : MonoBehaviour
    {
        private void Start() { gameObject.AddComponent<KerAstronautComplex>(); }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AstronautComplexEditor : AstronautComplexSpaceCentre
    {
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KerAstronautComplex : MonoBehaviour
    {


        private Rect _windowRect = default(Rect);
        private AstronautComplexApplicantPanel _applicantPanel;
        private float KStupidity = 50;
        private float KCourage = 50;
        private bool KFearless = false;
        private int KCareer = 0;
        private string[] KCareerStrings = { "Pilot", "Scientist", "Engineer" };
        private int KLevel = 0;
        private string[] KLevelStringsZero = new string[1] { "Level 0" };
        private string[] KLevelStringsOne = new string[2] { "Level 0", "Level 1" };
        private string[] KLevelStringsTwo = new string[3] { "Level 0", "Level 1", "Level 2" };
        private string[] KLevelStringsAll = new string[6] { "Level 0", "Level 1", "Level 2", "Level 3", "Level 4", "Level 5" };
        private int KGender = 0;
        private GUIContent KMale = new GUIContent("Male", AssetBase.GetTexture("kerbalicon_recruit"));
        private GUIContent KFemale = new GUIContent("Female", AssetBase.GetTexture("kerbalicon_recruit_female"));
        Color basecolor = GUI.color;
        private float ACLevel = 0;
        private double KDead;
        private double DCost = 1;
        KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
        private bool hTest = true;
        private bool hasKredits = true;


        private void Start()
        {

            try
            {
                SetupSkin();
                var complex = UIManager.instance.gameObject.GetComponentsInChildren<CMAstronautComplex>(true).FirstOrDefault();
                if (complex == null) throw new Exception("Could not find astronaut complex");

                _applicantPanel = new AstronautComplexApplicantPanel(complex);
                _windowRect = _applicantPanel.PanelArea;
                _applicantPanel.Hide();
                GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexShown);
                GameEvents.onGUIAstronautComplexDespawn.Add(AstronautComplexHidden);
                enabled = false;

            }
            catch (Exception e) 
            {
                Debug.LogError("KSI: Encountered unhandled exception: " + e);
                Destroy(this);
            }

        }


        private void dCheck()
        {
            KDead = 0;
            // 10 percent for dead and 5 percent for missing, note can only have dead in some career modes.
            foreach (ProtoCrewMember kerbal in roster.Crew)
            {
                if (kerbal.rosterStatus.ToString() == "Dead")
                {
                    if (kerbal.experienceTrait.Title == KCareerStrings[KCareer])
                    {
                        KDead += 1;
                    }
                }
                if (kerbal.rosterStatus.ToString() == "Missing")
                {
                    if (kerbal.experienceTrait.Title == KCareerStrings[KCareer])
                    {
                        KDead += 0.5;
                    }
                }
            }
        }

        private void AstronautComplexShown()
        {

            enabled = true;
        }

        private void AstronautComplexHidden()
        {
            enabled = false;
        }

        private void OnDestroy()
        {
            GameEvents.onGUIAstronautComplexDespawn.Remove(AstronautComplexHidden);
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexShown);
        }

        private void SetupSkin()
        {
            _customWindowSkin = new GUIStyle(HighLogic.Skin.window)
            {
                contentOffset = Vector2.zero,
                padding = new RectOffset() { left = 0, right = HighLogic.Skin.window.padding.right, top = 0, bottom = 0 },
            };
        }


        private void DumpAssetBaseTextures()
        {
            // note: there are apparently null entries in this list for some reason, hence the check
            //            FindObjectOfType<AssetBase>().textures.Where(t => t != null).ToList().ForEach(t => _log.Debug("AssetBase: " + t.name));
        }



        private GUIStyle _customWindowSkin;


        private readonly Texture2D _portraitMale = AssetBase.GetTexture("kerbalicon_recruit");
        private readonly Texture2D _portraitFemale = AssetBase.GetTexture("kerbalicon_recruit_female");

        private void OnGUI()
        {
            
            GUI.skin = HighLogic.Skin;
            var roster = HighLogic.CurrentGame.CrewRoster;
            GUIContent[] KGendArray = new GUIContent[2] { KMale, KFemale };
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                hasKredits = false;
                ACLevel = 5;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                hasKredits = false;
                ACLevel = 1;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                hasKredits = true;
                ACLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex);                
            }


            GUILayout.BeginArea(_windowRect, _customWindowSkin);
            {
                GUILayout.Label("Kerbal Science Institute: Placement Services"); // Testing Renaming Label Works

                // Gender selection 
                GUILayout.BeginHorizontal("box");
                KGender = GUILayout.Toolbar(KGender, KGendArray);
                GUILayout.EndHorizontal();

                // Career selection
                GUILayout.BeginVertical("box");
                KCareer = GUILayout.Toolbar(KCareer, KCareerStrings);
                GUILayout.Label("Pilots can use SAS and ships at vector markers.");
                GUILayout.Label("Scientists can reset experiements and science gains.");
                GUILayout.Label("Engineers help drills, repack chutes and fix some items.");
                GUILayout.Label("Note: Some mods give additional effects to some careers.");
                GUI.contentColor = basecolor;
                GUILayout.EndVertical();

                // Courage Brains and BadS flag selections
                GUILayout.BeginVertical("box");
                GUILayout.Label("Courage:  " + Math.Truncate(KCourage));
                KCourage = GUILayout.HorizontalSlider(KCourage, 0, 100);
                GUILayout.Label("Stupidity:  " + Math.Truncate(KStupidity));
                KStupidity = GUILayout.HorizontalSlider(KStupidity, 0, 100);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Is this Kerbal Fearless?");
                KFearless = GUILayout.Toggle(KFearless, "Fearless");
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                // Level selection
                GUILayout.BeginVertical("box");
                GUILayout.Label("Select Your Level:");

                // If statements for level options
                if (ACLevel == 0) { KLevel = GUILayout.Toolbar(KLevel, KLevelStringsZero); }
                if (ACLevel == 0.5) { KLevel = GUILayout.Toolbar(KLevel, KLevelStringsOne); }
                if (ACLevel == 1) { KLevel = GUILayout.Toolbar(KLevel, KLevelStringsTwo); }
                if (ACLevel == 5) { GUILayout.Label("Level 5 - Manditory for Sandbox or Science Mode."); }
                GUILayout.EndVertical();

                if (hasKredits == true)
                {
                    GUILayout.BeginHorizontal("window");
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();

                    if (costMath() <= Funding.Instance.Funds)
                    {
                        GUILayout.Label("Cost: " + costMath(), HighLogic.Skin.textField);
                    }
                    else
                    {
                        GUI.color = Color.red;
                        GUILayout.Label("Insufficient Funds - Cost: " + costMath(), HighLogic.Skin.textField);
                        GUI.color = basecolor;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (hTest)
                {
                    if (GUILayout.Button(hireStatus(), GUILayout.Width(200f)))
                        kHire();
                }
                if (!hTest)
                {
                    GUILayout.Button(hireStatus(), GUILayout.Width(200f));
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }
        }

        private void gameCheck()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                hasKredits = false;
                ACLevel = 5;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                hasKredits = false;
                ACLevel = 5;

            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                hasKredits = true;
                ACLevel = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex);
            }

        }

        private string hireStatus()
        {

            string bText = "Hire Applicant";
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                double kredits = Funding.Instance.Funds;
                if (costMath() > kredits)
                {
                    bText = "Not Enough Funds!";
                    hTest = false;
                }
                if (HighLogic.CurrentGame.CrewRoster.GetActiveCrewCount() >= GameVariables.Instance.GetActiveCrewLimit(ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.AstronautComplex)))
                {
                    bText = "Roster is Full!";
                    hTest = false;
                }
                else
                {
                    hTest = true;
                }
            }
            return bText;
        }

        private void kHire()
        {

            ProtoCrewMember newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
            int loopcount = 0;
            if ((KGender == 0) && (KCareer == 0))
            {
                while ((newKerb.experienceTrait.Title != "Pilot") || (newKerb.gender.ToString() != "Male"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            if ((KGender == 0) && (KCareer == 1))
            {
                while ((newKerb.experienceTrait.Title != "Scientist") || (newKerb.gender.ToString() != "Male"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            if ((KGender == 0) && (KCareer == 2))
            {
                while ((newKerb.experienceTrait.Title != "Engineer") || (newKerb.gender.ToString() != "Male"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            if ((KGender == 1) && (KCareer == 0))
            {
                while ((newKerb.experienceTrait.Title != "Pilot") || (newKerb.gender.ToString() != "Female"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            if ((KGender == 1) && (KCareer == 1))
            {
                while ((newKerb.experienceTrait.Title != "Scientist") || (newKerb.gender.ToString() != "Female"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            if ((KGender == 1) && (KCareer == 2))
            {
                while ((newKerb.experienceTrait.Title != "Engineer") || (newKerb.gender.ToString() != "Female"))
                {
                    HighLogic.CurrentGame.CrewRoster.Remove(newKerb);
                    newKerb = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
                    loopcount++;
                }
            }
            Debug.Log("KSI :: KIA MIA Stat is: " + KDead);
            Debug.Log("KSI :: " + newKerb.experienceTrait.TypeName + " " + newKerb.name + " has been created in: " + loopcount.ToString() + " loops.");
            newKerb.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            newKerb.experience = 0;
            newKerb.experienceLevel = 0;
            newKerb.courage = KCourage / 100;
            newKerb.stupidity = KStupidity / 100;
            if (KFearless)
            {
                newKerb.isBadass = true;
            }
            Debug.Log("KSI :: Status set to Available, courage and stupidity set, fearless trait set.");

            if (KLevel == 1)
            {
                newKerb.flightLog.AddEntry("Orbit,Kerbin");
                newKerb.flightLog.AddEntry("Suborbit,Kerbin");
                newKerb.flightLog.AddEntry("Flight,Kerbin");
                newKerb.flightLog.AddEntry("Land,Kerbin");
                newKerb.flightLog.AddEntry("Recover");
                newKerb.ArchiveFlightLog();
                newKerb.experience = 2;
                newKerb.experienceLevel = 1;
                Debug.Log("KSI :: Level set to 1.");
            }
            if (KLevel == 2)
            {
                newKerb.flightLog.AddEntry("Orbit,Kerbin");
                newKerb.flightLog.AddEntry("Suborbit,Kerbin");
                newKerb.flightLog.AddEntry("Flight,Kerbin");
                newKerb.flightLog.AddEntry("Land,Kerbin");
                newKerb.flightLog.AddEntry("Recover");
                newKerb.flightLog.AddEntry("Flyby,Mun");
                newKerb.flightLog.AddEntry("Orbit,Mun");
                newKerb.flightLog.AddEntry("Land,Mun");
                newKerb.flightLog.AddEntry("Flyby,Minmus");
                newKerb.flightLog.AddEntry("Orbit,Minmus");
                newKerb.flightLog.AddEntry("Land,Minmus");
                newKerb.ArchiveFlightLog();
                newKerb.experience = 8;
                newKerb.experienceLevel = 2;
                Debug.Log("KSI :: Level set to 2.");
            }
            if (ACLevel == 5)
            {
                newKerb.experience = 9999;
                newKerb.experienceLevel = 5;
                Debug.Log("KSI :: Level set to 5 - Non-Career Mode default.");
            }


            GameEvents.onGUIAstronautComplexSpawn.Fire(); // Refreshes the AC so that new kerbal shows on the available roster.
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Funding.Instance.AddFunds(-costMath(), TransactionReasons.CrewRecruited);
                Debug.Log("KSI :: Funds deducted.");
            }
            Debug.Log("KSI :: Hiring Function Completed.");
        }


        private int costMath()
        {
            dCheck();
            float fearcost = 0;
            float basecost = 25000;
            float couragecost = (50 - KCourage) * 150;
            float stupidcost = (KStupidity - 50) * 150;
            float diffcost = HighLogic.CurrentGame.Parameters.Career.FundsLossMultiplier;
            if (KFearless == true)
            {
                fearcost += 10000;
            }
            DCost = 1 + (KDead * 0.1f);
            

            double currentcost = (basecost - couragecost - stupidcost + fearcost) * (KLevel + 1) * DCost * diffcost;
            // double finaldouble = (Math.Round(currentcost));
            int finalcost = Convert.ToInt32(currentcost); //Convert.ToInt32(finaldouble);
            return finalcost;
        }
    }




    public class AstronautComplexApplicantPanel
    {
        private readonly Transform _applicantPanel;

        public AstronautComplexApplicantPanel(CMAstronautComplex astronautComplex)
        {
            if (astronautComplex == null) throw new ArgumentNullException("astronautComplex");
            _applicantPanel = astronautComplex.transform.Find("CrewPanels/panel_applicants");

            if (_applicantPanel == null)
                throw new ArgumentException("No applicant panel found on " + astronautComplex.name);
        }

        public void Hide(bool tf = true)
        {
            _applicantPanel.gameObject.SetActive(!tf);
        }


        public Rect PanelArea
        {
            get
            {
                // whole panel is an EzGUI BTButton and we'll be needing to know about its renderer to come up
                // with screen coordinates
                var button = _applicantPanel.GetComponent<BTButton>() as SpriteRoot;

                if (button == null)
                    throw new Exception("AstronautComplexApplicantPanel: Couldn't find BTButton on " +
                                        _applicantPanel.name);

                var uiCam = UIManager.instance.uiCameras.FirstOrDefault(uic => (uic.mask & (1 << _applicantPanel.gameObject.layer)) != 0);
                if (uiCam == null)
                    throw new Exception("AstronautComplexApplicantPanel: Couldn't find a UICamera for panel");

                var screenPos = uiCam.camera.WorldToScreenPoint(_applicantPanel.position);

                return new Rect(screenPos.x, Screen.height - screenPos.y, button.width, button.height);
            }
        }
    }
}


