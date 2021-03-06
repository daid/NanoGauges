﻿using System;
using UnityEngine;


namespace Nereid
{
   namespace NanoGauges
   {
      public abstract class VerticalGauge : AbstractClosableGauge
      {
         public const int SCALE_HEIGHT = 400;
         public const int SCALE_WIDTH = 42;

         private readonly Texture2D skin;
         private Rect skinBounds = new Rect(0, 0, WIDTH, HEIGHT); 
         private readonly Texture2D scale;
         private Rect scaleBounds = new Rect(0, 0, SCALE_WIDTH, SCALE_HEIGHT);

         private Damper damper;
         private bool autoLimiterEnabled = false;

         private readonly PowerOffFlag offFlag;
         private readonly LimiterFlag limitFlag;

         public VerticalGauge(int id, Texture2D skin, Texture2D scale, bool damped = true, float dampfactor=0.002f)
            : base(id)
         {
            this.damper = new Damper(dampfactor); 
            this.damper.SetEnabled(damped);
            this.scale = scale;
            this.skin = skin;

            if (scale == null) Log.Error("no scale for gauge "+id+" defined");
            if (skin == null) Log.Error("no skin for gauge " + id + " defined");

            offFlag = new PowerOffFlag(this);
            limitFlag = new LimiterFlag(this);
         }

         protected abstract float GetScaleOffset();

         protected int GetScaleHeight()
         {
            return scale.height;
         }

         protected float GetCenterOffset()
         {
            return (scale.height - HEIGHT) / (2.0f * scale.height);
         }

         protected float GetLowerOffset()
         {
            return 0.0f;
         }

         protected float GetOffset(int y)
         {
            int d = (scale.height-y) - HEIGHT / 2;
            return ((float)d) / (float)scale.height;
         }

         protected float GetUpperOffset()
         {
            return ((float)scale.height - (float)HEIGHT) / (float)scale.height;
         }

         protected override void OnWindow(int id)
         {
            // check for on/off
            AutomaticOnOff();
            // check for limits
            //AutomaticLimiter();

            float h = (float)HEIGHT / (float)(scale.height);
            damper.SetValue(GetScaleOffset());

            // scale
            Rect off = new Rect(0, damper.GetValue(), 1.0f, 0.25f);
            GUI.DrawTextureWithTexCoords(skinBounds, scale, off, false);
            //
            // flags
            if(NanoGauges.configuration.gaugeMarkerEnabled)
            {
               offFlag.Draw(GetWidth() / 2 - 4, 0);
               limitFlag.Draw(0, 0);
            }
            //
            // skin
            GUI.DrawTexture(skinBounds, skin);
         }

         public override void On()
         {
            offFlag.Up();
         }

         public override void Off()
         {
            offFlag.Down();
         }

         public override bool IsOn()
         {
            return offFlag.IsUp();
         }

         public override void InLimits()
         {
            limitFlag.Up();
         }

         public override void OutOfLimits()
         {
            limitFlag.Down();
         }

         public override bool IsInLimits()
         {
            return limitFlag.IsUp();
         }

         public override void Reset()
         {
            damper.Reset();
            limitFlag.Up();
         }

         protected virtual void AutomaticOnOff()
         {
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.parts.Count>0)
            {
               On();
            }
            else
            {
               Off();
            }
         }

         // not working right now
         protected virtual void AutomaticLimiter()
         {
            if(autoLimiterEnabled)
            {
               if(damper.IsInLimits())
               {
                  InLimits();
               }
               else
               {
                  OutOfLimits();
               }
            }
         }

         protected void SetAutoLimiterEnabled(bool enabled)
         {
            this.autoLimiterEnabled = enabled;
         }

      }
   }
}
