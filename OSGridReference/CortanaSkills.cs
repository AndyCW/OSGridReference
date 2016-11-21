using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OSGridReference
{
    public class CortanaSkills
    {
        public void Activated(IActivatedEventArgs args)
        {
            Frame rootFrame = CreateRootFrame();
            if (args.Kind == ActivationKind.Protocol)
            {
                HandleSkill(args, rootFrame);
            }
        }

        private async void HandleSkill(IActivatedEventArgs args, Frame rootFrame)
        {
            ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;

            if (protocolArgs != null)
            {
                Uri link = protocolArgs.Uri;
                string location = QueryParameterValue(link.Query, "location");

                rootFrame.Navigate(typeof(MainPage), "protocol");
            }
            Window.Current.Activate();
        }

        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }

        private string QueryParameterValue(string query, string parameter)
        {
            string value = string.Empty;

            string[] parts = query.Split('&');

            foreach (string p in parts)
            {
                if (p.StartsWith(parameter))
                {
                    value = p.Substring(p.IndexOf('=') + 1);
                    break;
                }
            }

            return value;
        }

        private DateTime TimeStampToDateTime(long timeStamp)
        {
            return DateTime.FromFileTime(timeStamp);
        }
    }
}
