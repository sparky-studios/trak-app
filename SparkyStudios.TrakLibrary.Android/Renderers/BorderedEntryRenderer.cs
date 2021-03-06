﻿using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using SparkyStudios.TrakLibrary.Controls;
using SparkyStudios.TrakLibrary.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderedEntry), typeof(BorderedEntryRenderer))]

namespace SparkyStudios.TrakLibrary.Droid.Renderers
{
    class BorderedEntryRenderer : EntryRenderer
    {
        public BorderedEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null && e.NewElement is BorderedEntry entry)
            {
                UpdateBorder(entry);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BorderedEntry.BorderColorProperty.PropertyName)
            {
                UpdateBorder((BorderedEntry) sender);
            }
        }

        private void UpdateBorder(BorderedEntry entry)
        {
            var gradientDrawable = new GradientDrawable();
            gradientDrawable.SetStroke(entry.BorderThickness, entry.BorderColor.ToAndroid());
            gradientDrawable.SetColor(entry.BackgroundColor.ToAndroid());
            Control.SetBackground(gradientDrawable);
            
            Control.SetPadding(Control.PaddingLeft, Control.PaddingTop, Control.PaddingRight,  
                Control.PaddingBottom);
        }
    }
}