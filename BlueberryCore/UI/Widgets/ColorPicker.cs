
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BlueberryCore.UI
{
    public class ColorPicker : Window
    {
        private Color pickerColor, pickerOldColor;

        public Action<Color> OnClose;
        private readonly Action<Chanel, int> OnChanelChange;
        private Dictionary<Chanel, ChanelBar> chanels = new Dictionary<Chanel, ChanelBar>();
        private HexBar hexBar;
        private SaturationValuePane svpicker;
        private HuePane hpicker;
        private ColorPreview cpreview, oldcpreview;
        private ColorPickerStyle style;
        private bool textureUpdateRequested = true;
        private readonly Effect shader;

        public int Hue { get { return chanels[Chanel.hue].GetValue(); } }
        public int Saturation { get { return chanels[Chanel.saturation].GetValue(); } }
        public int Value { get { return chanels[Chanel.value].GetValue(); } }

        //Constructors

        public ColorPicker(Color color, Skin skin, string stylename = "default") : this(color, skin.Get<ColorPickerStyle>(stylename)) { }

        public ColorPicker(Color color, ColorPickerStyle style) : base("color", style)
        {
            SetStyle(style);
            SetSize(PreferredWidth, PreferredHeight);

            var topPane = new Table();
            topPane.Add(CreatePickPanes()).PadRight(10);
            topPane.Add(CreateChanelBarPane()).PadLeft(10);
            Add(topPane).PadTop(GetPadTop()).Row();

            Add(CreateButtonsPane()).Right();

            shader = Core.libraryContent.Load<Effect>("shader");

            if (color == null)
            {
                pickerColor = Color.Black;
            }
            else
            {
                pickerColor = color == default ? Color.Black : color;
                UpdateRGBChanels();
                UpdateHSVChanels();
                UpdateAlphaChanel();
            }
            pickerOldColor = pickerColor;
        }
        
        #region Panel build methods

        protected Table CreateChanelBarPane()
        {
            var chanelBarTable = new Table();

            var red = new ChanelBar(this, Chanel.red);
            chanels.Add(Chanel.red, red);
            chanelBarTable.Add(red).PadBottom(5).Row();

            var green = new ChanelBar(this, Chanel.green);
            chanels.Add(Chanel.green, green);
            chanelBarTable.Add(green).PadTop(5).PadBottom(5).Row();

            var blue = new ChanelBar(this, Chanel.blue);
            chanels.Add(Chanel.blue, blue);
            chanelBarTable.Add(blue).PadTop(5).PadBottom(5).Row();

            var alpha = new ChanelBar(this, Chanel.alpha);
            chanels.Add(Chanel.alpha, alpha);
            chanelBarTable.Add(alpha).PadTop(10).PadBottom(10).Row();

            var hue = new ChanelBar(this, Chanel.hue);
            chanels.Add(Chanel.hue, hue);
            chanelBarTable.Add(hue).PadTop(5).PadBottom(5).Row();

            var saturation = new ChanelBar(this, Chanel.saturation);
            chanels.Add(Chanel.saturation, saturation);
            chanelBarTable.Add(saturation).PadTop(5).PadBottom(5).Row();

            var value = new ChanelBar(this, Chanel.value);
            chanels.Add(Chanel.value, value);
            chanelBarTable.Add(value).PadTop(5).Row();

            return chanelBarTable;
        }

        protected Table CreatePickPanes()
        {
            svpicker = new SaturationValuePane(this);
            hpicker = new HuePane(this);

            var pickersPane = new Table();
            pickersPane.Add(svpicker).Size(170).PadRight(20);
            pickersPane.Add(hpicker).Width(15).FillY().ExpandY();

            var previewAndHexPane = new Table();
            previewAndHexPane.Add(oldcpreview = new OldColorPreview(this)).Pad(10).PadRight(2).Size(25);
            previewAndHexPane.Add(cpreview = new CurrentColorPreview(this)).Pad(10).PadLeft(2).Size(25);
            previewAndHexPane.Add(hexBar = new HexBar(this));

            var table = new Table();
            table.Add(pickersPane).Top().Row();
            table.Add(previewAndHexPane);

            return table;
        }

        protected Table CreateButtonsPane()
        {
            var buttonsPane = new Table();

            var cancelButton = new TextButton("Close", style.button);
            cancelButton.OnClicked += (a) =>
            {
                OnClose?.Invoke(pickerOldColor);
                Close();
            };
            buttonsPane.Add(cancelButton).Width(50).Pad(10);

            var confirmButton = new TextButton("OK", style.button);
            confirmButton.OnClicked += (a) =>
            {
                OnClose?.Invoke(pickerColor);
                Close();
            };
            buttonsPane.Add(confirmButton).Width(50).Pad(10);

            return buttonsPane;
        }

        #region Preview panes

        private class CurrentColorPreview : ColorPreview
        {
            public CurrentColorPreview(ColorPicker picker) : base(picker)
            {
            }

            public override Color GetPreviewColor()
            {
                return picker.pickerColor;
            }
        }

        private class OldColorPreview : ColorPreview
        {
            public OldColorPreview(ColorPicker picker) : base(picker)
            {
            }

            public override Color GetPreviewColor()
            {
                return picker.pickerOldColor;
            }
        }

        #endregion

        #endregion

        protected void UpdateRGBChanels()
        {
            chanels[Chanel.red].UpdateValue(pickerColor.R);
            chanels[Chanel.green].UpdateValue(pickerColor.G);
            chanels[Chanel.blue].UpdateValue(pickerColor.B);
            hexBar.UpdateValue(pickerColor.PackedValue);
            svpicker.UpdateAmounts(chanels[Chanel.saturation].GetValue(), chanels[Chanel.value].GetValue());
            hpicker.UpdateAmount(chanels[Chanel.hue].GetValue());
        }

        protected void UpdateHSVChanels()
        {
            var hsv = pickerColor.ToHSV();
            int h = Convert.ToInt32(hsv.X), s = Convert.ToInt32(hsv.Y), v = Convert.ToInt32(hsv.Z);
            chanels[Chanel.hue].UpdateValue(h);
            chanels[Chanel.saturation].UpdateValue(s);
            chanels[Chanel.value].UpdateValue(v);
            hexBar.UpdateValue(pickerColor.PackedValue);
            svpicker.UpdateAmounts(chanels[Chanel.saturation].GetValue(), chanels[Chanel.value].GetValue());
            hpicker.UpdateAmount(chanels[Chanel.hue].GetValue());
        }

        protected void UpdateAlphaChanel()
        {
            chanels[Chanel.alpha].UpdateValue(pickerColor.A);
            hexBar.UpdateValue(pickerColor.PackedValue);
        }

        protected void UpdateTextures()
        {
            foreach(KeyValuePair<Chanel, ChanelBar> chanel in chanels)
            {
                chanel.Value.UpdateLineTexture();
            }
            svpicker.UpdateTexture();
            hpicker.UpdateTexture();
            cpreview.UpdateTexture();
            oldcpreview.UpdateTexture();
        }

        public void ChanelValueChanged(object sender, Chanel chanel, int value)
        {
            //Update chanel, that triggered event to sync slider and field values
            chanels[chanel].UpdateValue(value);
            switch(chanel)
            {
                case Chanel.red:
                    pickerColor.R = Convert.ToByte(value);
                    UpdateHSVChanels();
                    break;
                case Chanel.green:
                    pickerColor.G = Convert.ToByte(value);
                    UpdateHSVChanels();
                    break;
                case Chanel.blue:
                    pickerColor.B = Convert.ToByte(value);
                    UpdateHSVChanels();
                    break;
                case Chanel.alpha:
                    pickerColor.A = Convert.ToByte(value);
                    UpdateAlphaChanel();
                    break;
                default:
                    switch (chanel)
                    {
                        case Chanel.hue:
                            pickerColor.FromHSV(value, chanels[Chanel.saturation].GetValue(), chanels[Chanel.value].GetValue(), (byte)chanels[Chanel.alpha].GetValue());
                            break;
                        case Chanel.saturation:
                            pickerColor.FromHSV(chanels[Chanel.hue].GetValue(), value, chanels[Chanel.value].GetValue(), (byte)chanels[Chanel.alpha].GetValue());
                            break;
                        case Chanel.value:
                            pickerColor.FromHSV(chanels[Chanel.hue].GetValue(), chanels[Chanel.saturation].GetValue(), value, (byte)chanels[Chanel.alpha].GetValue());
                            break;
                    }
                    UpdateRGBChanels();

                    break;
            }
            textureUpdateRequested = true;
        }

        public void SetStyle(ColorPickerStyle style)
        {
            this.style = style;
        }

        public override WindowStyle GetStyle()
        {
            return style;
        }
        
        public static string GetChanelName(Chanel chanel)
        {
            switch (chanel)
            {
                case Chanel.red:
                    return "Red";
                case Chanel.green:
                    return "Green";
                case Chanel.blue:
                    return "Blue";
                case Chanel.hue:
                    return "Hue";
                case Chanel.saturation:
                    return "Sat";
                case Chanel.value:
                    return "Val";
                case Chanel.alpha:
                    return "Alpha";
                default:
                    return "";
            }
        }

        public static int GetChanelMaxValue(Chanel chanel)
        {
            switch (chanel)
            {
                case Chanel.red:
                    return 255;
                case Chanel.green:
                    return 255;
                case Chanel.blue:
                    return 255;
                case Chanel.hue:
                    return 360;
                case Chanel.saturation:
                    return 100;
                case Chanel.value:
                    return 100;
                case Chanel.alpha:
                    return 255;
                default:
                    return 0;
            }
        }

        public override void Draw(Graphics graphics, float parentAlpha)
        {
            Validate();
            if (textureUpdateRequested)
            {
                UpdateTextures();
                textureUpdateRequested = false;
            }
            base.Draw(graphics, parentAlpha);
        }

        #region ILayout

        public override float PreferredWidth
        {
            get
            {
                return 450;
            }
        }

        public override float PreferredHeight
        {
            get
            {
                return 320;
            }
        }

        protected override void SizeChanged()
        {
            base.SizeChanged();
            textureUpdateRequested = true;
        }

        #endregion

        #region Subclasses

        private class HexBar : Table
        {
            protected ColorPicker picker;
            protected HexField hexField;

            public HexBar(ColorPicker picker)
            {
                var label = new Label("Hex", picker.style.label);
                label.SetAlign(UI.Align.left);
                Add(label).Width(40).SpaceRight(10);
                Add(hexField = new HexField(picker)).Width(90);
                hexField.OnChange += (text) =>
                {
                    if (text.Length == 8)
                    {
                        uint value = Convert.ToUInt32(text, 16);
                        var color = new Color(value);
                        picker.ChanelValueChanged(this, Chanel.red, color.R);
                        picker.ChanelValueChanged(this, Chanel.green, color.G);
                        picker.ChanelValueChanged(this, Chanel.blue, color.B);
                        picker.ChanelValueChanged(this, Chanel.alpha, color.A);
                    }
                };
            }

            protected class HexField : TextField
            {
                protected ColorPicker picker;

                public HexField(ColorPicker picker) : base("", picker.style.input)
                {
                    this.picker = picker;
                    SetTextFieldFilter(new HexFilter());
                    SetMaxLength(8);
                }

                public override void SetText(string str)
                {
                    base.SetText(str.ToUpper());
                }

                protected override InputListener CreateListener()
                {
                    return new Listener(this);
                }

                private class Listener : TextFieldClickListener
                {
                    public Listener(TextField t) : base(t)
                    {
                        
                    }

                    public override bool KeyTyped(InputEvent ev, int keycode, char character)
                    {
                        return base.KeyTyped(ev, keycode, char.ToUpper(character));
                    }
                }
            }

            public void UpdateValue(uint value)
            {
                var str = value.ToString("X");
                while(str.Length < 8)
                {
                    str = str.Insert(0, "0");
                }
                hexField.SetText(str);
            }
        }

        private class HuePane : Element
        {
            protected ColorPicker picker;
            protected float amountY = 1;
            protected RenderTarget2D texture;
            protected bool sizeInvalid = true;
            private Listener listener;

            public HuePane(ColorPicker picker)
            {
                this.picker = picker;
                AddListener(listener = new Listener(this));
            }

            #region ILayout

            public override void Invalidate()
            {
                sizeInvalid = true;
            }

            public override void Layout()
            {
                if (sizeInvalid)
                {
                    if (texture != null)
                        texture.Dispose();
                    texture = new RenderTarget2D(Core.graphicsDevice, (int)GetWidth(), (int)GetHeight(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    sizeInvalid = false;
                }
            }

            #endregion

            #region Listener

            private class Listener : InputListener
            {
                private HuePane pane;
                public bool isActive;

                public Listener(HuePane pane)
                {
                    this.pane = pane;
                }

                public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
                {
                    isActive = true;
                    UpdateAmountY(y);
                    FocusManager.ResetFocus(pane.GetStage());
                    return true;
                }

                public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
                {
                    UpdateAmountY(y);
                }

                public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
                {
                    isActive = false;
                }

                protected void UpdateAmountY(float y)
                {
                    pane.amountY = SimpleMath.Clamp(y, 0, pane.GetHeight()) / pane.GetHeight();
                    pane.picker.ChanelValueChanged(pane, Chanel.hue, Convert.ToInt32(pane.amountY * GetChanelMaxValue(Chanel.hue)));
                }
            }

            #endregion

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                Validate();

                //if (graphics.spriteBatch.HasBegun)
                //    graphics.spriteBatch.End();

                var col = new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha));

                graphics.Shader = picker.shader;
                picker.shader.Parameters["mode"].SetValue(1);
                //graphics.spriteBatch.Begin();

                graphics.Draw(texture, new Vector2(GetX(), GetY()), texture.Bounds, col);

                //graphics.spriteBatch.End();
                graphics.Shader = null;

                graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth(), GetHeight(), new Color(Color.Black, col.A));
                if (picker.style != null)
                {
                    var selector = picker.style.barSelectorVertical;
                    if (selector != null)
                    {
                        selector.Draw(graphics, GetX() + 1, GetY() + amountY * GetHeight() - selector.MinHeight / 2 + 1, selector.MinWidth, selector.MinHeight, col);
                    }
                }
            }

            public void UpdateTexture()
            {
                var device = Graphics.graphicsDevice;
                var previousTarget = (RenderTarget2D)device.GetRenderTargets()[0].RenderTarget;

                device.SetRenderTarget(texture);
                device.Clear(new Color(picker.pickerColor, 1f));
                device.SetRenderTarget(previousTarget);
            }

            public void UpdateAmount(int h)
            {
                if (!listener.isActive)
                {
                    amountY = (float)h / GetChanelMaxValue(Chanel.hue);
                }
            }
        }

        private class SaturationValuePane : Element
        {
            protected ColorPicker picker;
            //amountY is 1 because picker starts with black
            protected float amountX, amountY = 1;
            protected RenderTarget2D texture;
            protected bool sizeInvalid = true;
            private Listener listener;

            public SaturationValuePane(ColorPicker picker)
            {
                this.picker = picker;
                AddListener(listener = new Listener(this));
            }

            #region ILayout

            public override void Invalidate()
            {
                sizeInvalid = true;
            }

            public override void Layout()
            {
                if (sizeInvalid)
                {
                    if (texture != null)
                        texture.Dispose();
                    texture = new RenderTarget2D(Core.graphicsDevice, (int)GetWidth(), (int)GetHeight(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    sizeInvalid = false;
                }
            }

            #endregion

            #region Listener

            private class Listener : InputListener
            {
                private SaturationValuePane pane;
                public bool isActive;

                public Listener(SaturationValuePane pane)
                {
                    this.pane = pane;
                }

                public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
                {
                    isActive = true;
                    UpdateAmountXY(x, y);
                    FocusManager.ResetFocus(pane.GetStage());
                    return true;
                }

                public override void TouchUp(InputEvent ev, float x, float y, int pointer, int button)
                {
                    isActive = false;
                }

                public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
                {
                    UpdateAmountXY(x, y);
                }

                protected void UpdateAmountXY(float x, float y)
                {
                    pane.amountX = SimpleMath.Clamp(x, 0, pane.GetWidth()) / pane.GetWidth();
                    pane.amountY = SimpleMath.Clamp(y, 0, pane.GetHeight()) / pane.GetHeight();
                    pane.picker.ChanelValueChanged(pane, Chanel.saturation, Convert.ToInt32(pane.amountX * GetChanelMaxValue(Chanel.saturation)));
                    pane.picker.ChanelValueChanged(pane, Chanel.value, GetChanelMaxValue(Chanel.value) - Convert.ToInt32(pane.amountY * GetChanelMaxValue(Chanel.value)));
                }
            }

            #endregion

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                Validate();

                //if (graphics.spriteBatch.HasBegun)
                //    graphics.spriteBatch.End();

                var col = new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha));

                graphics.Shader = picker.shader;
                picker.shader.Parameters["hsv"].SetValue(new Vector3((float)picker.Hue / 360, (float)picker.Saturation / 100, (float)picker.Value / 100));
                picker.shader.Parameters["mode"].SetValue(0);
                //graphics.spriteBatch.Begin();

                graphics.Draw(texture, new Vector2(GetX(), GetY()), texture.Bounds, col);

                //graphics.spriteBatch.End();
                graphics.Shader = null;
                
                graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth(), GetHeight(), new Color(Color.Black, col.A));
                if (picker.style != null)
                {
                    var cross = picker.style.cross;
                    if (cross != null)
                    {
                        cross.Draw(graphics, GetX() + amountX * GetWidth() - cross.MinWidth / 2, GetY() + amountY * GetHeight() - cross.MinHeight / 2, cross.MinWidth, cross.MinHeight, col);
                    }
                }
            }

            public void UpdateTexture()
            {
                var device = Graphics.graphicsDevice;
                var previousTarget = (RenderTarget2D)device.GetRenderTargets()[0].RenderTarget;
                
                device.SetRenderTarget(texture);
                device.Clear(new Color(picker.pickerColor, 1f));
                device.SetRenderTarget(previousTarget);
            }

            public void UpdateAmounts(int s, int v)
            {
                //is dragging now
                if (!listener.isActive)
                {
                    amountX = (float)s / GetChanelMaxValue(Chanel.saturation);
                    amountY = 1f - (float)v / GetChanelMaxValue(Chanel.value);
                }
            }
        }

        private class ChanelBar : Table
        {
            protected ColorPicker picker;
            protected ChanelLine valueLine;
            protected TextField valueField;
            protected Chanel chanel;
            
            public ChanelBar(ColorPicker picker, Chanel chanel)
            {
                this.picker = picker;
                this.chanel = chanel;
                var label = new Label(GetChanelName(chanel), picker.style.label);
                label.SetAlign(UI.Align.left);
                Add(label).Width(40).SpaceRight(10);
                Add(valueLine = new ChanelLine(picker, chanel)).Size(100, 14).SpaceRight(10);
                Add(valueField = new TextField("0", picker.style.input)).Width(30);
                valueLine.OnChange += (value) =>
                {
                    picker.ChanelValueChanged(this, chanel, value);
                };
                valueField.SetTextFieldFilter(new TextField.DigitsOnlyFilter());
                valueField.SetMaxLength(3);
                valueField.OnChange += (text) =>
                {
                    int.TryParse(text, out int value);
                    int parsedvalue = SimpleMath.Clamp(value, 0, GetChanelMaxValue(chanel));
                    if (value > GetChanelMaxValue(chanel))
                    {
                        valueField.SetText(parsedvalue + "");
                        valueField.SetCursorPosition(3);
                    }
                    picker.ChanelValueChanged(this, chanel, parsedvalue);
                };
            }

            protected class ChanelLine : Element
            {
                protected ColorPicker picker;
                protected Chanel chanel;

                private float amountX;
                private int MaxValue { get; }
                private RenderTarget2D texture;
                private bool sizeInvalid = true;

                public Action<int> OnChange;

                public ChanelLine(ColorPicker picker, Chanel chanel)
                {
                    this.picker = picker;
                    this.chanel = chanel;
                    MaxValue = GetChanelMaxValue(chanel);
                    AddListener(new Listener(this));
                }

                #region ILayout

                public override void Invalidate()
                {
                    sizeInvalid = true;
                }

                public override void Layout()
                {
                    if (sizeInvalid)
                    {
                        if (texture != null)
                            texture.Dispose();
                        texture = new RenderTarget2D(Core.graphicsDevice, (int)GetWidth(), (int)GetHeight(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                        sizeInvalid = false;
                    }
                }

                #endregion

                #region Listener

                protected class Listener : InputListener
                {
                    protected ChanelLine line;

                    public Listener(ChanelLine line)
                    {
                        this.line = line;
                    }

                    public override bool TouchDown(InputEvent ev, float x, float y, int pointer, int button)
                    {
                        UpdateAmountX(x);
                        line.GetStage()?.SetKeyboardFocus(null);
                        FocusManager.ResetFocus(line.GetStage());
                        return true;
                    }

                    public override void TouchDragged(InputEvent ev, float x, float y, int pointer)
                    {
                        UpdateAmountX(x);
                    }

                    protected void UpdateAmountX(float x)
                    {
                        line.amountX = SimpleMath.Clamp(x, 0, line.GetWidth()) / line.GetWidth();
                        line.OnChange(Convert.ToInt32(line.amountX * line.MaxValue));
                    }
                }

                #endregion

                public override void Draw(Graphics graphics, float parentAlpha)
                {
                    Validate();
                    
                    //if (graphics.spriteBatch.HasBegun)
                    //    graphics.spriteBatch.End();

                    var col = new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha));

                    graphics.Shader = picker.shader;
                    picker.shader.Parameters["mode"].SetValue((int)chanel + 2);
                    picker.shader.Parameters["dimensions"].SetValue(new Vector2(texture.Width, texture.Height));
                    //graphics.spriteBatch.Begin();

                    graphics.Draw(texture, new Vector2(GetX(), GetY()), texture.Bounds, col);

                    //graphics.spriteBatch.End();
                    graphics.Shader = null;

                    graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth(), GetHeight(), new Color(Color.Black, col.A));
                    if (picker.style != null)
                    {
                        var selector = picker.style.barSelector;
                        if (selector != null)
                        {
                            selector.Draw(graphics, GetX() + amountX * GetWidth() - selector.MinWidth / 2, GetY(), selector.MinWidth, selector.MinHeight, col);
                        }
                    }
                }

                public void UpdateTexture()
                {
                    var device = Graphics.graphicsDevice;
                    var previousTarget = (RenderTarget2D)device.GetRenderTargets()[0].RenderTarget;

                    device.SetRenderTarget(texture);
                    device.Clear(new Color(picker.pickerColor, 1f));
                    device.SetRenderTarget(previousTarget);
                }

                public void SetValue(int value)
                {
                    amountX = (float)value / MaxValue;
                }

                public int GetValue()
                {
                    return Convert.ToInt32(amountX * MaxValue);
                }
            }

            public void UpdateValue(int value)
            {
                value = SimpleMath.Clamp(value, 0, GetChanelMaxValue(chanel));
                valueField.SetText(value + "");
                valueLine.SetValue(value);
            }

            public void UpdateLineTexture()
            {
                valueLine.UpdateTexture();
            }

            public int GetValue()
            {
                return valueLine.GetValue();
            }
        }

        private class ColorPreview : Element
        {
            protected ColorPicker picker;
            protected RenderTarget2D texture;
            protected bool sizeInvalid = true;

            public ColorPreview(ColorPicker picker)
            {
                this.picker = picker;
            }

            #region ILayout

            public override void Invalidate()
            {
                sizeInvalid = true;
            }

            public override void Layout()
            {
                if (sizeInvalid)
                {
                    if (texture != null)
                        texture.Dispose();
                    texture = new RenderTarget2D(Core.graphicsDevice, (int)GetWidth(), (int)GetHeight(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                    sizeInvalid = false;
                }
            }

            #endregion

            public override void Draw(Graphics graphics, float parentAlpha)
            {
                Validate();

                //if (graphics.spriteBatch.HasBegun)
                //    graphics.spriteBatch.End();

                var col = new Color(color.R, color.G, color.B, (int)(color.A * parentAlpha));

                graphics.Shader = picker.shader;
                picker.shader.Parameters["mode"].SetValue(9);
                picker.shader.Parameters["dimensions"].SetValue(new Vector2(texture.Width, texture.Height));
                //graphics.spriteBatch.Begin();

                graphics.Draw(texture, new Vector2(GetX(), GetY()), texture.Bounds, col);

                //graphics.spriteBatch.End();
                graphics.Shader = null;

                graphics.DrawRectangleBorder(GetX(), GetY(), GetWidth(), GetHeight(), new Color(Color.Black, col.A));
            }

            public virtual Color GetPreviewColor()
            {
                return Color.White;
            }

            public void UpdateTexture()
            {
                var device = Graphics.graphicsDevice;
                var previousTarget = (RenderTarget2D)device.GetRenderTargets()[0].RenderTarget;

                device.SetRenderTarget(texture);
                device.Clear(GetPreviewColor());
                device.SetRenderTarget(previousTarget);
            }
        }

        #endregion
    }

    public enum Chanel
    {
        red, green, blue, alpha, hue, saturation, value
    }

    public class HexFilter : TextField.ITextFieldFilter
    {
        public bool AcceptChar(TextField textField, char c)
        {
            return (char.IsDigit(c) || c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F' || c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f');
        }
    }

    public class ColorPickerStyle : WindowStyle
    {
        public IDrawable cross, barSelector, barSelectorVertical;
        public LabelStyle label;
        public TextFieldStyle input;
        public TextButtonStyle button;

        public ColorPickerStyle()
        {
            
        }

        public ColorPickerStyle(WindowStyle style, IDrawable cross, IDrawable barSelector, IDrawable barSelectorVertical, LabelStyle label, TextFieldStyle input, TextButtonStyle button)
        {
            this.cross = cross;
            this.barSelector = barSelector;
            if (style != null)
            {
                this.background = style.background;
                this.titleFont = style.titleFont;
                this.titleFontColor = style.titleFontColor;
            }
            this.label = label;
            this.input = input;
            this.button = button;
        }
    }
}