using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using raptor;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Avalonia.Input;
using ReactiveUI;
using System.Reactive;
using interpreter;
using RAPTOR_Avalonia_MVVM.Controls;
using RAPTOR_Avalonia_MVVM.Views;
using Avalonia.Threading;

namespace RAPTOR_Avalonia_MVVM.ViewModels
{
    public class GraphDialogViewModel : ViewModelBase
    {

        //public static void checkOpen()
        //{
        //    if(DotnetGraphControl.dngw == null)
        //    {
        //        throw new Exception("Graph window not open!");
        //    }
        //}

        public static void DrawLine(int x1, int y1, int x2, int y2,Color_Type c)
        {

            //checkOpen();

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawLine(x1, y1, x2, y2, c);
            }, DispatcherPriority.Background);
        }

        public static void DrawBox(int x1, int y1, int x2, int y2, Color_Type hue, bool filled)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawBox(x1, y1, x2, y2, hue, filled);
            }, DispatcherPriority.Background);

        }

        public static void DrawCircle(int x1, int y1, int rad, Color_Type hue, bool filled)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawCircle(x1, y1, rad, hue, filled);
            }, DispatcherPriority.Background);

        }

        public static void DrawEllipse(int x1, int y1, int x2, int y2, Color_Type hue, bool filled)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawEllipse(x1, y1, x2, y2, hue, filled);
            }, DispatcherPriority.Background);

        }
        public static void DrawArc(int x1, int y1, int x2, int y2, int startx, int starty, int endx, int endy, Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawArc(x1, y1, x2, y2, startx, starty, endx, endy, hue);
            }, DispatcherPriority.Background);

        }
        public static void DrawEllipseRotate(int x1, int y1, int x2, int y2, double angle, Color_Type hue, bool filled)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawEllipseRotate(x1, y1, x2, y2, angle, hue, filled);
            }, DispatcherPriority.Background);
        }

        public static void DisplayText(int x1, int y1, string text, Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DisplayText(x1, y1, text, hue);
            }, DispatcherPriority.Background);
        }

        public static void DisplayNumber(int x1, int y1, double number, Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DisplayNumber(x1, y1, number, hue);
            }, DispatcherPriority.Background);
        }

        public static void FontSize(int size)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.SetFontSize(size);
            }, DispatcherPriority.Background);
        }

        public static void WaitForMouseButton(MouseButton b)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.WaitForMouseButton(b);
            }, DispatcherPriority.Background);
        }
        public static void WaitForKey()
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.WaitForKey();
            }, DispatcherPriority.Background);
        }

        public static void SetWindowTitle(string title)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.SetWindowTitle(title);
            }, DispatcherPriority.Background);
        }

        public static void ClearWindow(Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.ClearWindow(hue);
            }, DispatcherPriority.Background);
        }

        public static void PlaySound(string s)
        {


            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.PlaySound(s);

            }, DispatcherPriority.Background);
        }
        public static void PlaySoundBackground(string s)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.PlaySoundBackground(s);
            }, DispatcherPriority.Background);
        }
        public static void PlaySoundBackgroundLoop(string s)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.PlaySoundBackgroundLoop(s);
            }, DispatcherPriority.Background);
        }

        public static double GetWindowHeight()
        {
            

            return DotnetGraphControl.dngw.Height;
        }

        public static double GetWindowWidth()
        {
            

            return DotnetGraphControl.dngw.Width;
        }

        public static void DelayFor(int seconds)
        {
            DotnetGraphControl.DelayFor(seconds);
        }

        public static void FloodFill(int x, int y, Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.FloodFill(x, y, hue);
            }, DispatcherPriority.Background);
        }

        public static void PutPixel(int x, int y, Color_Type hue)
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.PutPixel(x, y, hue);
            }, DispatcherPriority.Background);
        }

        public static numbers.value GetMaxHeight()
        {
            
            double h = 0;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if(DotnetGraph.dotnetgraph == null)
                {
                    DotnetGraph d = new DotnetGraph(1, 1);
                    h = d.Screens.Primary.Bounds.Size.Height;
                }
                else
                {
                    h = DotnetGraph.dotnetgraph.Screens.Primary.Bounds.Size.Height;
                }

            }).Wait(-1);
            
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = h };
            
        }

        public static numbers.value GetMaxWidth()
        {

            //DotnetGraphControl.dngw.GetMaxWidth();
            double w = 0;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (DotnetGraph.dotnetgraph == null)
                {
                    DotnetGraph d = new DotnetGraph(1, 1);
                    w = d.Screens.Primary.Bounds.Size.Width;
                }
                else
                {
                    w = DotnetGraph.dotnetgraph.Screens.Primary.Bounds.Size.Width;
                }

            }).Wait(-1);
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = w };
        }

        public static void OpenGraphWindow(int w, int h)
        {
            //Dispatcher.UIThread.InvokeAsync(() =>
            //{
                DotnetGraphControl.OpenGraphWindow(w, h);
                //DotnetGraphControl.dngw.OpenGraphWindow(w, h);
            //}).Wait(-1);

        }

        public static void CloseGraphWindow()
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.CloseGraphWindow();
            }, DispatcherPriority.Background);
        }

        public static void FreezeGraphWindow()
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.FreezeGraphWindow();
            }, DispatcherPriority.Background);
        }

        public static void UnFreezeGraphWindow()
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.UnfreezeGraphWindow();
            }, DispatcherPriority.Background);
        }

        public static void UpdateGraphWindow()
        {
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.FreezeGraphWindow();
            }, DispatcherPriority.Background);
        }



        public static numbers.value GetMouseX()
        {
            

            int x = DotnetGraphControl.dngw.GetMouseX();
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = x };
        }
        public static numbers.value GetMouseY()
        {
            

            int y = DotnetGraphControl.dngw.GetMouseY();
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = y };
        }
        public static numbers.value LoadBitmap(string fileName)
        {
            // "../../../sample_640×426.bmp"
            

            int i = DotnetGraphControl.dngw.LoadBitmap(fileName);
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = i };

        }
        public static void DrawBitmap(int index, int x, int y, int width, int height)
        {
            // 0, 100, 100, 300, 300
            

            Dispatcher.UIThread.Post(() =>
            {
                DotnetGraphControl.dngw.DrawBitmap(index, x, y, width, height);
            }, DispatcherPriority.Background);
        }
        public static bool KeyDown(Avalonia.Input.Key button)
        {
            

            return DotnetGraphControl.dngw.Key_Down(button);
            
        }
        public static bool KeyHit()
        {
            

            return DotnetGraphControl.dngw.KeyHit();

        }

        public static numbers.value? GetKey()
        {
            

            Key k = DotnetGraphControl.dngw.GetKey();

            int x = getAsciiValue((int)k);
            
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = getAsciiValue((int)k) };
        }

        public static numbers.value? GetKeyString()
        {
            

            Key k = DotnetGraphControl.dngw.GetKey();

            return new numbers.value() { Kind = numbers.Value_Kind.String_Kind, S = k + "" };
        }

        public static numbers.value GetPixel(int x, int y)
        {
            

            Color_Type c = new Color_Type();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                c = DotnetGraphControl.dngw.GetPixel(x, y);

            }).Wait(-1);

            return new numbers.value() { Kind = numbers.Value_Kind.String_Kind, S = c + "" };
        }

        public static void SaveGraphWindow(string filename)
        {
            

            DotnetGraphControl.dngw.saveGraphWindow(filename);
        }

        public static bool IsOpen()
        {
            if(DotnetGraphControl.dngw == null)
            {
                return false;
            }
            return DotnetGraphControl.dngw.IsOpen();
        }

        public static int getAsciiValue(int a)
        {
            switch (a)
            {
                case int n when (n >= (int)Key.A && n <= (int)Key.Z):
                    return n + 53;
                case int n when (n >= (int)Key.D0 && n <= (int)Key.D9):
                    return n + 14;
                case (int)Key.Add:
                    return 43;
                case (int)Key.Back:
                    return 8;
                case (int)Key.Delete:
                    return 127;
                case (int)Key.Escape:
                    return 27;
                case (int)Key.LeftCtrl:
                    return 17;
                case (int)Key.RightCtrl:
                    return 18;
            }

            return -1;
        }

        public static numbers.value GetClosestColor(int r, int g, int b)
        {
            int c = DotnetGraphControl.GetClosestColor(r, g, b);

            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = c , S = (Color_Type)c + ""};
        }

        public static bool MouseButtonDown(MouseButton button)
        {
            

            return DotnetGraphControl.dngw.MouseButtonDown(button);
        }

        public static bool MouseButtonPressed(MouseButton button)
        {
            

            return DotnetGraphControl.dngw.MouseButtonPressed(button);
        }

        public static bool MouseButtonReleased(MouseButton button)
        {
            
            return DotnetGraphControl.dngw.MouseButtonReleased(button);
        }

        public async static Task GetMouseButton(MouseButton button)
        {
            await DotnetGraphControl.dngw.GetMouseButton(button);
        }

        public static numbers.value GetFontHeight()
        {
            int x = DotnetGraphControl.dngw.GetFontHeight();
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = x };
        }
        public static numbers.value GetFontWidth()
        {
            int x = DotnetGraphControl.dngw.GetFontWidth();
            return new numbers.value() { Kind = numbers.Value_Kind.Number_Kind, V = x };

        }
    }


}