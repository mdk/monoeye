/* MonoEye
 *
 * GPL license, Copyright (C) 2007 by:
 *
 * Authors:
 *      Michael Dominic K. <mdk@mdk.am>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public
 * License as published by the Free Software Foundation; either
 * version 2 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
 */

using System;
using Gtk;
using System.IO;
using System.Net;
using System.Text;

namespace MonoEye {

        public class MainWindow : Dialog {

                Image image;
                Fetcher currentFetcher;
                Entry entry;
                HBox hbox;

                public MainWindow () : base ()
                {
                        Frame frame = new Frame ();
                        image = new Image ();
                        hbox = new HBox (false, 6);
                        Button button = new Button ("See");
                        button.Clicked += OnSeeClicked;
                        entry = new Entry ();

                        hbox.PackStart (entry, true, true, 0);
                        hbox.PackEnd (button, false, false, 0);

                        frame.Add (image);
                        
                        VBox.PackStart (frame, true, true, 0);
                        VBox.PackEnd (hbox, false, false, 0);
                        VBox.Spacing = 6;

                        BorderWidth = 6;
                        Title = "MonoEye";
                        HasSeparator = false;
                }

                void OnSeeClicked (object o, EventArgs args)
                {
                        if (entry.Text != String.Empty) {
                                currentFetcher = new Fetcher (entry.Text, new GLib.IdleHandler (OnFetchFinishedIdle));
                                currentFetcher.Execute ();
                                hbox.Sensitive = false;                
                        }
                }

                bool OnFetchFinishedIdle ()
                {
                        if (currentFetcher == null)
                                return false;

                        if (currentFetcher.Pixbuf == null)
                                return false;

                        Gdk.Threads.Enter ();
                        image.Pixbuf = currentFetcher.Pixbuf;
                        hbox.Sensitive = true;
                        Gdk.Threads.Leave ();

                        return false;
                }

        }

}
