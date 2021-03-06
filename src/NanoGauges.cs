﻿using System;
using System.Collections.Generic;
using UnityEngine;
using NanoGaugesAdapter;

namespace Nereid
{
   namespace NanoGauges
   {
      [KSPAddon(KSPAddon.Startup.Flight, false)]
      public class NanoGauges : MonoBehaviour
      {
         public static readonly String RESOURCE_PATH = "Nereid/NanoGauges/Resource/";

         public static readonly Configuration configuration = new Configuration();
         // cached configuration (needs a restart to take effect)
         private static readonly bool trimIndicatorsEnabled;

         public static readonly FARAdapter farAdapter = new FARAdapter();

         private static readonly Gauges gauges;

         public static readonly SnapinManager snapinManager;

         private IButton toolbarButton;
         private ApplicationLauncherButton stockToolbarButton = null;

         private ConfigWindow configWindow;

         private volatile bool destroyed = false;

         private readonly TrimIndicators trimIndicators;

         static NanoGauges()
         {
            Log.SetLevel(Log.LEVEL.INFO);
            Log.Info("static init of NanoGauges");
            configuration.Load();
            gauges = new Gauges();
            snapinManager = new SnapinManager(gauges);
            Log.SetLevel(configuration.GetLogLevel());
            trimIndicatorsEnabled = configuration.trimIndicatorsEnabled;
         }

         public NanoGauges()
         {
            Log.Info("new instance of NanoGauges");
            this.trimIndicators = new TrimIndicators();
         }

         public void Awake()
         {
            farAdapter.Plugin();
            gauges.ShowGauges();
            // use of stock toolbar
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
         }

         public void Start()
         {
            Log.Info("starting NanoGauges");
            if (trimIndicatorsEnabled)
            {
               trimIndicators.Init();
            }
            if (!configuration.useStockToolbar)
            {
               AddToolbarButtons();
            }
         }

         private void OnGUIAppLauncherReady()
         {
            if (destroyed) return;
            if (configuration.useStockToolbar)
            {
               if (ApplicationLauncher.Ready && stockToolbarButton == null)
               {
                  Log.Info("creating stock toolbar button");
                  stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                  OnAppLaunchToggleOn,
                  OnAppLaunchToggleOff,
                  DummyVoid,
                  DummyVoid,
                  DummyVoid,
                  DummyVoid,
                  ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                  (Texture)GameDatabase.Instance.GetTexture(RESOURCE_PATH + "ToolbarIcon", false));
                  if (stockToolbarButton == null) Log.Warning("no stock toolbar button registered");
               }

            }
         }

         void OnAppLaunchToggleOn()
         {
            createConfigOnce();
            if (configWindow != null)
            {
               configWindow.SetVisible(true);
            }
         }

         void OnAppLaunchToggleOff()
         {
            if (configWindow != null)
            {
               configWindow.SetVisible(false);
            }
         }

         private void DummyVoid() { }


         private void AddToolbarButtons()
         {
            Log.Detail("adding toolbar buttons");
            String iconOn = RESOURCE_PATH + "IconOn_24";
            String iconOff = RESOURCE_PATH + "IconOff_24";
            toolbarButton = ToolbarManager.Instance.add("NanoGauges", "button");
            if (toolbarButton != null)
            {
               toolbarButton.TexturePath = iconOff;
               toolbarButton.ToolTip = "Open NanoGauges Configuration";
               toolbarButton.OnClick += (e) =>
               {
                  createConfigOnce();
                  if (configWindow != null) configWindow.registerToolbarButton(toolbarButton, iconOn, iconOff);
                  toggleConfigVisibility();
               };

               toolbarButton.Visibility = new GameScenesVisibility( GameScenes.FLIGHT );
            }
            else
            {
               Log.Error("toolbar button was null");
            }
         }

         private void toggleConfigVisibility()
         {
            configWindow.SetVisible(!configWindow.IsVisible());
         }

         private void createConfigOnce()
         {
            if (configWindow == null)
            {
               configWindow = new ConfigWindow(gauges);
               configWindow.CallOnWindowClose(OnBrowserClose);

            }
         }

         public void OnBrowserClose()
         {
            if (stockToolbarButton != null)
            {
               stockToolbarButton.toggleButton.Value = false;
            }
         }

         internal void OnDestroy()
         {
            Log.Info("destroying Nano Gauges");
            destroyed = true;
            Log.Info("log level is " + Log.GetLogLevel());
            if (toolbarButton!=null)
            {
               toolbarButton.Destroy();
            }
            if (stockToolbarButton != null)
            {
               Log.Detail("removing stock toolbar button");
               ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
            }
            gauges.SaveWindowPositions();
            configuration.Save();
            farAdapter.Unplug();
         }


         private void SetGaugeSet(GaugeSet.ID id)
         {
            Log.Detail("SetGaugeSet to "+id);
            configuration.SetGaugeSet(id);
            gauges.ReflectGaugeSetChange();
         }

         public static HashSet<AbstractGauge> GetCluster(AbstractGauge gauge)
         {
            return gauges.GetCluster(gauge);
         }

         public void Update()
         {
            if (trimIndicatorsEnabled)
            {
               trimIndicators.Update();
            }

            gauges.Update();

            // check for keys
            //
            KeyCode hotkey = configuration.GetKeyCodeForHotkey();
            bool hotkeyPressed = Input.GetKey(hotkey);
            gauges.ShowCloseButtons(hotkeyPressed);

            if(hotkeyPressed)
            {
               AbstractGauge g = gauges.GetGauge(Constants.WINDOW_ID_GAUGE_AOA);
            }

            // Hotkeys for Gaugesets
            if(hotkeyPressed)
            {
               if(Input.GetKeyDown(KeyCode.Alpha1))
               {
                  SetGaugeSet(GaugeSet.ID.STANDARD);
               }
               else if(Input.GetKeyDown(KeyCode.Alpha2))
               {
                  SetGaugeSet(GaugeSet.ID.LAUNCH);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha3))
               {
                  SetGaugeSet(GaugeSet.ID.LAND);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha4))
               {
                  SetGaugeSet(GaugeSet.ID.DOCK);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha5))
               {
                  SetGaugeSet(GaugeSet.ID.ORBIT);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha6))
               {
                  SetGaugeSet(GaugeSet.ID.FLIGHT);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha7))
               {
                  SetGaugeSet(GaugeSet.ID.SET1);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha8))
               {
                  SetGaugeSet(GaugeSet.ID.SET2);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha9))
               {
                  SetGaugeSet(GaugeSet.ID.SET3);
               }
               else if (Input.GetKeyDown(KeyCode.Alpha0))
               {
                  gauges.EnableAllGauges();
               }
               else if (Input.GetKeyDown(KeyCode.Backspace))
               {
                  gauges.ResetPositions();
               }
               else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Delete))
               {
                  if(gauges.Hidden())
                  {
                     gauges.Unhide();
                  }
                  else
                  {
                     gauges.Hide();
                  }
               }
               else if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Insert))
               {
                  gauges.AutoLayout();
               }
            }
         }
      }
   }
}